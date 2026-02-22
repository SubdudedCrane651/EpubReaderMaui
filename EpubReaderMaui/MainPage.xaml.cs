using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using VersOne.Epub;
using System.IO;

namespace EpubReaderMaui;

public partial class MainPage : ContentPage
{
    private readonly List<string> _chapterTitles = new();
    private readonly List<string> _chapterHtml = new();
    private int _currentChapter = 0;

    public MainPage()
    {
        InitializeComponent();
    }

    private async void OpenButton_Clicked(object sender, EventArgs e)
    {
        var result = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Select EPUB file"
        });

        if (result == null)
            return;

        using var stream = await result.OpenReadAsync();
        var book = await EpubReader.ReadBookAsync(stream);

        _chapterTitles.Clear();
        _chapterHtml.Clear();
        ChaptersView.ItemsSource = null;

        foreach (var chapter in book.ReadingOrder)
        {
            string html = chapter.Content; // XHTML

            if (string.IsNullOrWhiteSpace(html))
                continue;

            _chapterHtml.Add(html);

            // VersOne.Epub 2.x: FilePath is the ONLY path property
            string title = Path.GetFileNameWithoutExtension(chapter.FilePath);
            _chapterTitles.Add(title);
        }

        ChaptersView.ItemsSource = _chapterTitles;

        _currentChapter = 0;
        ChaptersView.SelectedItem = _chapterTitles[0];
        DisplayChapter(0);
    }

    private void DisplayChapter(int index)
    {
        if (index < 0 || index >= _chapterHtml.Count)
            return;

        _currentChapter = index;

        string bodyHtml = _chapterHtml[index];

        string finalHtml = $@"
<html>
<head>
<meta charset='utf-8'>
<style>
body {{
    font-family: sans-serif;
    font-size: 17px;
    line-height: 1.6;
    padding: 16px;
}}
img {{
    max-width: 100%;
}}
</style>
</head>
<body>
{bodyHtml}
<div style='text-align:center; margin-top:20px; font-weight:bold;'>
Chapter {index + 1} / {_chapterHtml.Count}
</div>
</body>
</html>";

        HtmlView.Source = new HtmlWebViewSource { Html = finalHtml };
        PageLabel.Text = $"Chapter {index + 1} / {_chapterHtml.Count}";
    }

    private void NextButton_Clicked(object sender, EventArgs e)
    {
        if (_currentChapter < _chapterHtml.Count - 1)
        {
            _currentChapter++;
            ChaptersView.SelectedItem = _chapterTitles[_currentChapter];
            DisplayChapter(_currentChapter);
        }
    }

    private void PrevButton_Clicked(object sender, EventArgs e)
    {
        if (_currentChapter > 0)
        {
            _currentChapter--;
            ChaptersView.SelectedItem = _chapterTitles[_currentChapter];
            DisplayChapter(_currentChapter);
        }
    }

    private void ChaptersView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is string selected)
        {
            int index = _chapterTitles.IndexOf(selected);
            if (index >= 0)
                DisplayChapter(index);
        }
    }
}