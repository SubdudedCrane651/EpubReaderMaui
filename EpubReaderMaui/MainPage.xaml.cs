using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using VersOne.Epub;

namespace EpubReaderMaui;

public class ReaderProgress
{
    public string BookHash { get; set; } = "";
    public string PageHash { get; set; } = "";
}

public partial class MainPage : ContentPage
{
    private readonly List<string> _chapterTitles = new();
    private readonly List<List<string>> _chapterPages = new();
    private readonly List<List<string>> _chapterPageHashes = new();

    private Dictionary<string, byte[]> _images = new();

    private int _currentChapter = 0;
    private int _currentPage = 0;

    private ReaderProgress _progress = new();
    private bool _chaptersVisible = true;
    private bool _suppressSelection = false;

    private const int ParagraphsPerPage = 6;

    public MainPage()
    {
        InitializeComponent();
        DeviceDisplay.KeepScreenOn = true;
    }

    private string ComputeHash(string text)
    {
        using var sha = SHA256.Create();
        byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(bytes);
    }

    private string ComputeFileHash(Stream stream)
    {
        using var sha = SHA256.Create();
        byte[] bytes = sha.ComputeHash(stream);
        return Convert.ToHexString(bytes);
    }

    // ⭐ Embed EPUB images into HTML as Base64
    private string EmbedImages(string html)
    {
        foreach (var kv in _images)
        {
            string key = kv.Key;
            string filename = key.Split('/').Last();
            string base64 = Convert.ToBase64String(kv.Value);
            string dataUrl = $"data:image/jpeg;base64,{base64}";

            html = html.Replace($"src=\"{key}\"", $"src=\"{dataUrl}\"");
            html = html.Replace($"src=\"{filename}\"", $"src=\"{dataUrl}\"");
        }

        return html;
    }

    private async void OpenButton_Clicked(object sender, EventArgs e)
    {
        var result = await FilePicker.PickAsync();
        if (result == null)
            return;

        using var stream = await result.OpenReadAsync();

        string bookHash = ComputeFileHash(stream);
        stream.Position = 0;

        var book = await EpubReader.ReadBookAsync(stream);

        _chapterTitles.Clear();
        _chapterPages.Clear();
        _chapterPageHashes.Clear();
        _images.Clear();
        ChaptersView.ItemsSource = null;

        // Replace this:
        // foreach (var img in book.Content.Images)
        // {
        //     _images[img.Key] = img.Value.Content;
        // }

        // With this:
        foreach (var img in book.Content.Images.Local)
        {
            _images[img.Key] = img.Content;
        }

        // ⭐ COVER PAGE
        if (book.CoverImage != null)
        {
            string base64 = Convert.ToBase64String(book.CoverImage);
            string html = $@"
<div style='text-align:center; padding:20px;'>
    <img src='data:image/jpeg;base64,{base64}' style='max-width:100%; height:auto;' />
</div>";

            _chapterTitles.Add("COVER");
            _chapterPages.Add(new List<string> { html });
            _chapterPageHashes.Add(new List<string> { ComputeHash(html) });
        }

        // ⭐ CHAPTERS WITH IMAGES
        int chapterNumber = 1;
        foreach (var chapter in book.ReadingOrder)
        {
            string html = chapter.Content;
            if (string.IsNullOrWhiteSpace(html))
                continue;

            html = EmbedImages(html);

            var pages = PaginateChapter(html);
            var hashes = pages.Select(p => ComputeHash(p)).ToList();

            _chapterTitles.Add($"CHAPTER {chapterNumber}");
            _chapterPages.Add(pages);
            _chapterPageHashes.Add(hashes);

            chapterNumber++;
        }

        ChaptersView.ItemsSource = _chapterTitles;

        // ⭐ RESTORE PROGRESS
        _progress = LoadProgressInternal();

        if (_progress.BookHash == bookHash)
        {
            bool found = false;

            for (int c = 0; c < _chapterPageHashes.Count; c++)
            {
                int p = _chapterPageHashes[c].FindIndex(h => h == _progress.PageHash);
                if (p >= 0)
                {
                    _currentChapter = c;
                    _currentPage = p;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                _currentChapter = 0;
                _currentPage = 0;
            }
        }
        else
        {
            _currentChapter = 0;
            _currentPage = 0;
            _progress = new ReaderProgress { BookHash = bookHash };
        }

        _suppressSelection = true;

        ChaptersView.SelectedItem = _chapterTitles[_currentChapter];
        DisplayPage(_currentChapter, _currentPage);

        _suppressSelection = false;
    }

    // ⭐ Paginate by paragraphs (keeps images intact)
    private List<string> PaginateChapter(string html)
    {
        var pages = new List<string>();

        var parts = Regex.Split(html, "(</p>)", RegexOptions.IgnoreCase);
        var paragraphs = new List<string>();

        for (int i = 0; i < parts.Length - 1; i += 2)
            paragraphs.Add(parts[i] + parts[i + 1]);

        if (paragraphs.Count == 0)
        {
            pages.Add(html);
            return pages;
        }

        var current = new List<string>();
        int count = 0;

        foreach (var p in paragraphs)
        {
            current.Add(p);
            count++;

            if (count >= ParagraphsPerPage)
            {
                pages.Add(string.Join("\n", current));
                current.Clear();
                count = 0;
            }
        }

        if (current.Count > 0)
            pages.Add(string.Join("\n", current));

        return pages;
    }

    private void DisplayPage(int chapterIndex, int pageIndex)
    {
        var pages = _chapterPages[chapterIndex];
        string bodyHtml = pages[pageIndex];

        string pageNumberHtml =
            $"<div style='margin-top:40px; text-align:center; opacity:0.6;'>Page {pageIndex + 1} / {pages.Count}</div>";

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
    height: auto;
}}
</style>
</head>
<body>
{bodyHtml}
{pageNumberHtml}
</body>
</html>";

        HtmlView.Source = new HtmlWebViewSource { Html = finalHtml };

        PageLabel.Text =
            $"{_chapterTitles[chapterIndex]} – Page {pageIndex + 1} / {pages.Count}";

        _currentChapter = chapterIndex;
        _currentPage = pageIndex;

        SaveCurrentProgress();
    }

    private void NextButton_Clicked(object sender, EventArgs e)
    {
        var pages = _chapterPages[_currentChapter];

        if (_currentPage < pages.Count - 1)
        {
            DisplayPage(_currentChapter, _currentPage + 1);
        }
        else if (_currentChapter < _chapterPages.Count - 1)
        {
            DisplayPage(_currentChapter + 1, 0);
            ChaptersView.SelectedItem = _chapterTitles[_currentChapter];
        }
    }

    private void PrevButton_Clicked(object sender, EventArgs e)
    {
        if (_currentPage > 0)
        {
            DisplayPage(_currentChapter, _currentPage - 1);
        }
        else if (_currentChapter > 0)
        {
            int prev = _currentChapter - 1;
            int lastPage = _chapterPages[prev].Count - 1;
            DisplayPage(prev, lastPage);
            ChaptersView.SelectedItem = _chapterTitles[_currentChapter];
        }
    }

    private void ChaptersView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressSelection)
            return;

        if (e.CurrentSelection.FirstOrDefault() is string selected)
        {
            int index = _chapterTitles.IndexOf(selected);
            DisplayPage(index, 0);
        }
    }

    private void SaveCurrentProgress()
    {
        _progress.PageHash = _chapterPageHashes[_currentChapter][_currentPage];
        SaveProgressInternal();
    }

    private void ToggleChapters_Clicked(object sender, EventArgs e)
    {
        _chaptersVisible = !_chaptersVisible;

        ContentGrid.ColumnDefinitions[0].Width =
            _chaptersVisible ? new GridLength(220) : new GridLength(0);

        ChaptersView.IsVisible = _chaptersVisible;
    }

    private void SaveProgressInternal()
    {
        string json = JsonSerializer.Serialize(_progress);
        string path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "progress_pages.json"
        );
        File.WriteAllText(path, json);
    }

    private ReaderProgress LoadProgressInternal()
    {
        string path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "progress_pages.json"
        );

        if (!File.Exists(path))
            return new ReaderProgress();

        string json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<ReaderProgress>(json) ?? new ReaderProgress();
    }
}