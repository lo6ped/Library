using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
public interface IBookStorage
{
    bool Save(List<Book> books);
    List<Book> Load();
}
class FailingStorage :IBookStorage
{
    public bool Save(List<Book> books)
    {
        Console.WriteLine("Имитация сбоя сохранения");
        return false;
    }
    public List<Book> Load()
    {
        return new List<Book>();
    }
}
class InMemoryStorage : IBookStorage
{
    private List<Book> _memoryBooks = new();
    public bool Save(List<Book> books)                
    {
        _memoryBooks = new List<Book>(books);
        return true;
    }
    public List<Book> Load()
    {
        return new List<Book>(_memoryBooks);
    }
}
public class BookStorage : IBookStorage
{
    private readonly JsonSerializerOptions _options;
    private readonly string _filePath;
    public BookStorage(string filePath)
    {
        _filePath = filePath;
        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
    }
    private string BackupPath => _filePath + ".bak";
    private void CopyBrokenFile()
    {
        try
        {
            string stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            string brokenPath = $"{_filePath}.{stamp}.broken";
            File.Copy(_filePath, brokenPath);
            Console.WriteLine($"Копия поврежденного файла сохранена в {brokenPath}");
        }
        catch(IOException ex)
        {
            ReadError(ex.Message);
        }
        catch(UnauthorizedAccessException ex)
        {
            ReadError(ex.Message);
        }
    }

    private void ReadError(string message)
    {
        Console.WriteLine($"Ошибка работы с файлом: {message}");
    }
    public bool Save(List<Book> books)
    {
        try
        {
            if (File.Exists(_filePath))
            {
                File.Copy(_filePath, BackupPath, overwrite: true);
            }
            string json = JsonSerializer.Serialize(books, _options);
            File.WriteAllText(_filePath, json);
            Console.WriteLine($"Книг сохранено: {books.Count}");
            return true;
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Ошибка записи файла: {ex.Message}");
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Ошибка доступа: {ex.Message}");
            return false;
        }
    }
    public List<Book> Load()
    {
        return LoadFrom(_filePath);
    }
    public List<Book> LoadBackup()
    {
        if (File.Exists(BackupPath))
        {
            return LoadFrom(BackupPath);
        }
        throw new FileNotFoundException("Файл с бэкапом не найден");
    }
    private List<Book> LoadFrom(string path)
    {
        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                List<BookData>? raw = JsonSerializer.Deserialize<List<BookData>>(json);
                List<Book> result = new List<Book>();
                int skipped = 0;
                foreach (BookData item in raw ?? new List<BookData>())
                {
                    try
                    {
                        result.Add(new Book(item.Title ?? "", item.Author ?? "", item.Pages));
                    }
                    catch (ArgumentException)
                    {
                        skipped++;
                    }
                }
                if (skipped > 0)
                {
                    Console.WriteLine($"Пропущено повреждённых записей: {skipped}");
                    CopyBrokenFile();
                }
                return result;
            }
            catch (JsonException ex)
            {
                ReadError(ex.Message);
                CopyBrokenFile();
            }
            catch (IOException ex)
            {
                ReadError(ex.Message);
                throw;
            }
            catch (UnauthorizedAccessException ex)
            {
                ReadError(ex.Message);
                throw;
            }
        }
        else
        {
            Console.WriteLine("Файла нет, начинаем с пустого списка");
        }
        return new List<Book>();
    }
}

