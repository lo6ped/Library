using System;
using System.Collections.Generic;
using System.Text;

public class BookStats
{
    public List<KeyValuePair<string, int>> AuthorStats(List<Book> books)
    {
        var stats = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (Book book in books)
        {
            if (stats.ContainsKey(book.Author))
            {
                stats[book.Author]++;
            }
            else
            {
                stats[book.Author] = 1;
            }
        }
        var result = stats.OrderByDescending(pair => pair.Value)
                    .ThenBy(pair => pair.Key).ToList() ;
        return result;
    }
}
