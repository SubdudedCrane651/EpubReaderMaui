using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using VersOne.Epub;

namespace EpubReaderMaui;

public partial class MainPage : ContentPage
{
    private readonly List<string> _chapters = new();
    private List<string> _pages = new();
    private int _currentChapter = 0;
    private int _currentPage = 0;
    private const int PageSize = 2000; // simple fixed-size pages

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

        if (!result.FileName.EndsWith(".epub", StringComparison.OrdinalIgnoreCase))
        {
            await DisplayAlert("Error", "Please select an EPUB file.", "OK");
            return;
        }

        using var stream = await result.OpenReadAsync();
        var book = await EpubReader.ReadBookAsync(stream);

        _chapters.Clear();
        ChaptersView.ItemsSource = null;

        // Extract chapters as text
        foreach (var chapter in book.ReadingOrder)
        {
            string text = chapter.Content; // VersOne.Epub: text content

            if (!string.IsNullOrWhiteSpace(text))
                _chapters.Add(text);
        }

        if (_chapters.Count == 0)
        {
            await DisplayAlert("Error", "No readable chapters found.", "OK");
            return;
        }

        // Simple chapter titles
        ChaptersView.ItemsSource = _chapters
            .Select((c, i) => $"Chapter {i + 1}")
            .ToList();

        _currentChapter = 0;
        _currentPage = 0;
        LoadChapter(_currentChapter);
    }

    private void LoadChapter(int index)
    {
        if (index < 0 || index >= _chapters.Count)
            return;

        _currentChapter = index;
        _pages = Paginate(_chapters[index]);
        _currentPage = 0;
        DisplayPage();
    }

    private List<string> Paginate(string text)
    {
        var pages = new List<string>();
        for (int i = 0; i < text.Length; i += PageSize)
        {
            int len = Math.Min(PageSize, text.Length - i);
            pages.Add(text.Substring(i, len));
        }

        if (pages.Count == 0)
            pages.Add(string.Empty);

        return pages;
    }

    private void DisplayPage()
    {
        if (_pages.Count == 0)
        {
            ContentLabel.Text = string.Empty;
            PageLabel.Text = string.Empty;
            return;
        }

        var text = _pages[_currentPage];
        ContentLabel.Text = text;

        int total = _pages.Count;
        int current = _currentPage + 1;
        PageLabel.Text = $"Page {current} / {total}";
    }

    private void NextButton_Clicked(object sender, EventArgs e)
    {
        if (_pages.Count == 0)
            return;

        if (_currentPage < _pages.Count - 1)
        {
            _currentPage++;
            DisplayPage();
        }
    }

    private void PrevButton_Clicked(object sender, EventArgs e)
    {
        if (_pages.Count == 0)
            return;

        if (_currentPage > 0)
        {
            _currentPage--;
            DisplayPage();
        }
    }

    private void ChaptersView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is string selected)
        {
            var list = ChaptersView.ItemsSource.Cast<string>().ToList();
            int index = list.IndexOf(selected);
            if (index >= 0)
                LoadChapter(index);
        }
    }
}