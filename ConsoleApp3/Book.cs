using System;
using System.Collections.Generic;
using System.Text;
public enum BookGenre
{
    Unknown,
    Fantastic,
    Fighting,
    Adventure,  
    Romance,
}
class BookData
{
    public string? Title {  get; set; }
    public string? Author { get; set; }
    public int Pages { get; set; }
    public BookGenre Genre { get; set;  }
}
public class Book
{
    public string Title { get; init; }
    public string Author { get; init; }
    public int Pages { get; init; }
    public BookGenre Genre { get; init; }
    public Book(string title, string author, int pages, BookGenre genre = BookGenre.Unknown)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Название не может быть пустым");
        }
        if (string.IsNullOrWhiteSpace(author))
        {
            throw new ArgumentException("Автор книги должен быть указан");
        }
        if (pages <= 0)
        {
            throw new ArgumentException("Некорректное значение страниц");
        }
        Title = title;
        Author = author;
        Pages = pages;
        Genre = genre;
    }
    public static string GenreTranslate(BookGenre genre)    // что такое static?
    {
        switch (genre)
        {
            case BookGenre.Unknown: return "без жанра";
            case BookGenre.Fantastic: return "фантастика";
            case BookGenre.Fighting: return "боевик";
            case BookGenre.Adventure: return "приключения";
            case BookGenre.Romance: return "романтика";
            default: return "неизвестно";
        }
    }
    public void PrintInfo()
    {
        Console.WriteLine(ToString());
    }
    public override string ToString()
    {
        return $"«{Title}» — {Author}, {Pages} стр., {GenreTranslate(Genre)}";
    }
}

