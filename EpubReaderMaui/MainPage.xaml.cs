using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System.Text.Json;
using VersOne.Epub;

namespace EpubReaderMaui;

public class ReaderProgress
{
    public int Chapter { get; set; }
}

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
        var result = await FilePicker.PickAsync();
        if (result == null)
            return;

        using var stream = await result.OpenReadAsync();
        var book = await EpubReader.ReadBookAsync(stream);

        _chapterTitles.Clear();
        _chapterHtml.Clear();
        ChaptersView.ItemsSource = null;

        // COVER (if exists)
        if (book.CoverImage != null && book.CoverImage.Length > 0)
        {
            string base64 = Convert.ToBase64String(book.CoverImage);
            string coverHtml = $@"
<html>
<body style='text-align:center; padding:20px;'>
    <img src='data:image/jpeg;base64,{base64}' style='max-width:100%; height:auto;' />
</body>
</html>";
            _chapterHtml.Add(coverHtml);
            _chapterTitles.Add("COVER");
        }

        // CHAPTERS from ReadingOrder
        int chapterNumber = 1;
        foreach (var chapter in book.ReadingOrder)
        {
            string html = chapter.Content;
            if (string.IsNullOrWhiteSpace(html))
                continue;

            _chapterHtml.Add(html);
            _chapterTitles.Add($"CHAPTER {chapterNumber}");
            chapterNumber++;
        }

        if (_chapterHtml.Count == 0)
            return;

        ChaptersView.ItemsSource = _chapterTitles;

        // Load last progress
        _currentChapter = LoadProgress();
        if (_currentChapter < 0 || _currentChapter >= _chapterHtml.Count)
            _currentChapter = 0;

        ChaptersView.SelectedItem = _chapterTitles[_currentChapter];
        DisplayChapter(_currentChapter);
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

        SaveProgress();
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

    private void SaveProgress()
    {
        try
        {
            var progress = new ReaderProgress { Chapter = _currentChapter };
            string json = JsonSerializer.Serialize(progress);
            string path = Path.Combine(FileSystem.AppDataDirectory, "progress.json");
            File.WriteAllText(path, json);
        }
        catch
        {
            // ignore errors for now
        }
    }

    private int LoadProgress()
    {
        try
        {
            string path = Path.Combine(FileSystem.AppDataDirectory, "progress.json");
            if (!File.Exists(path))
                return 0;

            string json = File.ReadAllText(path);
            var progress = JsonSerializer.Deserialize<ReaderProgress>(json);
            return progress?.Chapter ?? 0;
        }
        catch
        {
            return 0;
        }
    }
}