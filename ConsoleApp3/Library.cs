using System;
using System.Collections.Generic;
using System.Text;

public class Library
{
    private readonly List<Book> _books;
    public Library(List<Book> initial)
    {
        _books = new List<Book>(initial);
    }
    public bool Remove(Book book)
    { 
        return _books.Remove(book);
    }
    public int Count => _books.Count;
    public bool Add(Book book)
    {
        if(Exists(book.Title, book.Author))
        {
            return false;
        }
        _books.Add(book);
        return true;
    }
    public bool Exists(string title, string author)
    {
        return _books.Any(book => book.Title.ToLower() == title.ToLower()&&  book.Author.ToLower()==author.ToLower());
    }
    public List<Book> GetAll()
    {
        return new List<Book>(_books);
    }
    public List<Book> FindByGenre(BookGenre genre)
    {      
        foreach(BookGenre bookGenre in Enum.GetValues<BookGenre>())
        {
            if (bookGenre == genre)
            {
                var result = _books.Where(book => book.Genre == genre).ToList();
                return result.Count > 0 ? result : new List<Book>();
            }
        }
        Console.WriteLine($"{genre} не найден");
        return new List<Book>();
    }
}
