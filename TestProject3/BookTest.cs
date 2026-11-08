
using System.Diagnostics;

namespace TestProject3
{
    public class BookTest
    {
        [Fact]
        public void Add_LibrarySameBookTwice_ReturnsFalse()
        {
            var library = new Library(new List<Book>());
            library.Add(new Book("Война и мир", "Толстой", 300));

            bool addSame = library.Add(new Book("ВОЙНА и мир", "ТОЛстой", 300));

            Assert.False(addSame);
        }
        [Fact]
        public void Add_LibrarySameBookTitleDifferentAuthor_ReturnsTrue()
        {
            var library = new Library(new List<Book>());
            library.Add(new Book("Война и мир", "Толстой", 300));

            bool addSame = library.Add(new Book("ВОЙНА и мир", "я", 300));

            Assert.True(addSame);
        }
        [Theory]
        [InlineData("", 1)]
        [InlineData("вав",0)]
        [InlineData("вав", -1)]
        [InlineData("вав", -100)]
        public void Book_WithInvalidPagesOrTIitle_Throws(string bookTitle, int bookPages)
        {
            Assert.Throws<ArgumentException>(() => new Book(bookTitle, "я", bookPages));
        }
        [Fact]
        public void Book_WithPositivePages_CreatedCorrectly()
        {
            var book = new Book("ВОЙНА и мир", "я", 1);
            
            Assert.Equal(1, book.Pages);
        }
        [Fact]
        public void Library_CopyListDoesntChangeOriginal_CorrectList()
        {
            var library = new Library(new List<Book>());
            library.Add(new Book("ВОЙНА и мир", "я", 1));

            var copy = library.GetAll();
            copy.Clear();

            Assert.Equal(1, library.Count);
        }
        [Fact]
        public void Library_DescendingAuthorWrite_CorrectList()
        {
            var bookStats = new BookStats();
            var books  = new List<Book>();
            books.Add(new Book("аваф", "Толстой", 123));
            books.Add(new Book("аф", "Толстой", 123));
            books.Add(new Book("авф", "Тамагочи", 123));
            books.Add(new Book("а", "Тамагочи", 123));
            books.Add(new Book("вф", "Аксаков", 123));

            var result = bookStats.AuthorStats(books);

            Assert.Equal("Тамагочи", result[0].Key);
            Assert.Equal(2, result[0].Value);
            Assert.Equal("Толстой", result[1].Key);
            Assert.Equal("Аксаков", result[2].Key);
            Assert.Equal(1, result[2].Value);
        }
        [Fact]
        public void SaveAndLoadBooks_SaveReturnsToLoad()
        {
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
            string backup = path + ".bak";
            try
            {
                var storage = new BookStorage(path);

                Assert.True(storage.Save(new List<Book> { new Book("а", "Тамагочи", 123), new Book("вф", "Аксаков", 123) }));
                var loaded = storage.Load();

                Assert.Equal(2, loaded.Count);
                Assert.Equal("а", loaded[0].Title);
                Assert.Equal("Тамагочи", loaded[0].Author);
                Assert.Equal(123, loaded[0].Pages);
                Assert.Equal("вф", loaded[1].Title);
                Assert.True(storage.Save(loaded));
            }
            finally
            {
                File.Delete(path);
                File.Delete(backup);
            }
        }
        [Fact]
        public void Load_OneBadRecord_SkipsItAndCreatesBrokenCopy()
        {
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
            string folder = Path.GetTempPath();
            string name = Path.GetFileName(path);
            string[] broken = Array.Empty<string>();
            try
            {
                File.WriteAllText(path,
                    "[{\"Title\":\"а\",\"Author\":\"Тамагочи\",\"Pages\":123}," +
                    "{\"Title\":\"вф\",\"Author\":\"Аксаков\",\"Pages\":-5}]");
                var storage = new BookStorage(path);

                var loaded = storage.Load();
                broken = Directory.GetFiles(folder, name + ".*.broken");

                Assert.Single(loaded);
                Assert.Equal("а", loaded[0].Title);
                Assert.Equal("Тамагочи", loaded[0].Author);
                Assert.Equal(123, loaded[0].Pages);
                Assert.Single(broken);
            }
            finally
            {
                File.Delete(path);
                File.Delete(path + ".bak");
                foreach (string file in broken)
                {
                    File.Delete(file);
                }
            }
        }
    }
}
