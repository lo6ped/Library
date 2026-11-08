using System;
using System.Collections.Generic;
using System.Text;
class BookData
{
    public string? Title {  get; set; }
    public string? Author { get; set; }
    public int Pages { get; set; }
}
public class Book
{
    public string Title { get; init; }
    public string Author { get; init; }
    public int Pages { get; init; }
    public Book(string title, string author, int pages)
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
    }
    public void PrintInfo()
    {
        Console.WriteLine(ToString());
    }
    public override string ToString()
    {
        return $"«{Title}» — {Author}, {Pages} стр.";
    }
}

