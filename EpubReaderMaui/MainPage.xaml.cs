using Microsoft.Maui.Controls;
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
    private bool _chaptersVisible = true;
    private ReaderProgress _progress = new();

    public MainPage()
    {
        InitializeComponent();
    }

    // -------------------------------
    // OPEN EPUB
    // -------------------------------
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

        // COVER PAGE
        if (book.CoverImage != null && book.CoverImage.Length > 0)
        {
            string base64 = Convert.ToBase64String(book.CoverImage);
            string coverHtml = $@"
<html>
<head>
<meta charset='utf-8'>
<style>
body {{
    font-family: sans-serif;
    text-align:center;
    padding:20px;
}}
img {{
    max-width:100%;
    height:auto;
}}
</style>
</head>
<body>
    <img src='data:image/jpeg;base64,{base64}' />
</body>
</html>";

            _chapterHtml.Add(coverHtml);
            _chapterTitles.Add("COVER");
        }

        // CHAPTERS
        int chapterNumber = 1;
        foreach (var chapter in book.ReadingOrder)
        {
            string html = chapter.Content;
            if (string.IsNullOrWhiteSpace(html))
                continue;

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
{html}
</body>
</html>";

            _chapterHtml.Add(finalHtml);
            _chapterTitles.Add($"CHAPTER {chapterNumber}");
            chapterNumber++;
        }

        if (_chapterHtml.Count == 0)
            return;

        ChaptersView.ItemsSource = _chapterTitles;

        // LOAD PROGRESS
        _progress = LoadProgressInternal();
        if (_progress.Chapter < 0 || _progress.Chapter >= _chapterHtml.Count)
            _progress.Chapter = 0;

        _currentChapter = _progress.Chapter;
        ChaptersView.SelectedItem = _chapterTitles[_currentChapter];
        DisplayChapter(_currentChapter);
    }

    // -------------------------------
    // DISPLAY CHAPTER
    // -------------------------------
    private void DisplayChapter(int index)
    {
        if (index < 0 || index >= _chapterHtml.Count)
            return;

        _currentChapter = index;

        HtmlView.Source = new HtmlWebViewSource { Html = _chapterHtml[index] };
        PageLabel.Text = $"Chapter {index + 1} / {_chapterHtml.Count}";

        _progress.Chapter = _currentChapter;
        SaveProgressInternal();
    }

    // -------------------------------
    // NAVIGATION
    // -------------------------------
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

    // -------------------------------
    // TOGGLE CHAPTER LIST
    // -------------------------------
    private void ToggleChapters_Clicked(object sender, EventArgs e)
    {
        _chaptersVisible = !_chaptersVisible;

        if (_chaptersVisible)
        {
            ContentGrid.ColumnDefinitions[0].Width = new GridLength(220);
            ChaptersView.IsVisible = true;
        }
        else
        {
            ContentGrid.ColumnDefinitions[0].Width = new GridLength(0);
            ChaptersView.IsVisible = false;
        }
    }

    // -------------------------------
    // SAVE / LOAD PROGRESS (JSON)
    // -------------------------------
    private void SaveProgressInternal()
    {
        try
        {
            string json = JsonSerializer.Serialize(_progress);
            string path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "progress.json"
            );
            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Save error: " + ex.Message);
        }
    }

    private ReaderProgress LoadProgressInternal()
    {
        try
        {
            string path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "progress.json"
            );

            if (!File.Exists(path))
                return new ReaderProgress();

            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<ReaderProgress>(json) ?? new ReaderProgress();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Load error: " + ex.Message);
            return new ReaderProgress();
        }
    }
}