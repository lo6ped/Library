using System.Diagnostics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text.Encodings.Web;
using System.Text.Json;
Console.OutputEncoding = System.Text.Encoding.UTF8;
var storage = new BookStorage("books.json");
Library library;
while(true)
{
    try
    {
        library = new Library(storage.Load());
        break;
    }
    catch(Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
    {
        int choice = ReadIntInRange("Выберите действие:\n1 - повторить загрузку\n2 - начать с пустого списка\n3 - загрузить последнее сохранение\n", 1, 3);
        if (choice == 1)
        {
            continue;
        }
        if (choice == 2)
        {
            if(AskYesNo("При сохранении старый файл с данными будет перезаписан.\nВы уверены что хотите начать с пустого списка?"))
            {
                library = new Library(new List<Book>());
                break;
            }
            else
            {
                continue;
            }
        }
        if (choice == 3)
        {
            try
            {
                library = new Library(storage.LoadBackup());
                break;
            }
            catch(IOException)
            {
                Console.WriteLine("Не удалось загрузить бэкап");
                continue;
            }
            catch(UnauthorizedAccessException)
            {
                Console.WriteLine("Не удалось загрузить бэкап, проблема с доступом к файлу");
                continue;
            }
        }
    }
}
bool running = true;
bool changed = false;
while (running)
{
    Console.WriteLine("1 — показать все книги от самой толстой к самой тонкой");
    Console.WriteLine("2 — поиск по автору");
    Console.WriteLine("3 — поиск по названию");
    Console.WriteLine("4 — добавить книгу");
    Console.WriteLine("5 — удалить книгу");
    Console.WriteLine("6 — сохранить в файл");
    Console.WriteLine("7 — статистика по авторам");
    Console.WriteLine("0 — выход");
    string choice = Console.ReadLine() ?? "";
    choice = choice.Trim();
    switch (choice)
    {
        case "1":
            ShowAll(library.GetAll());
            break;
        case "2":
            ShowByAuthor(library.GetAll());
            break;
        case "3":
            ShowByTitle(library.GetAll());
            break;
        case "4":
            if (AddBook(library))
            { changed = true; }
            break;
        case "5":
            if (RemoveBook(library))
            { changed = true; }
            break;
        case "6":
            if (storage.Save(library.GetAll()))
            { changed = false; }
            break;
        case "7":
            ShowAuthorStats(library.GetAll());
            break;
        case "0":
            if (changed == false)
            {
                running = false;
                break;
            }
            if (changed == true)
            {
                if (AskYesNo("Сохранить изменения? (да / нет) : "))
                {
                    if(storage.Save(library.GetAll()))
                    {
                        running = false;
                    }
                    else
                    {
                        Console.WriteLine("Файл не сохранен");
                    }
                }
                else { running = false; }
            }
            break;
        default:
            Console.WriteLine("Неизвестная команда");
            break;
    }
}
List<Book> FindByAuthor(List<Book> books, string author)
{
    List<Book> result = new List<Book>();

    foreach (Book book in books)
    {
        if (book.Author.ToLower() == author.ToLower())
        {
            result.Add(book);
        }
    }

    return result;
}
List<Book> FindByTitle(List<Book> books, string title)
{
    List<Book> result = new List<Book>();

    foreach (Book book in books)
    {
        if (book.Title.ToLower().Contains(title.ToLower()))
        {
            result.Add(book);
        }
    }

    return result;
}
bool AskYesNo(string question)
{
    Console.Write(question);
    string acception = Console.ReadLine() ?? "";
    acception = acception.Trim().ToLower();
    while (acception != "да" && acception != "нет")
    {
        Console.WriteLine("Введите да или нет");
        acception = Console.ReadLine() ?? "";
        acception = acception.Trim().ToLower();
    }
    if (acception == "да")
    {
        return true;
    }
    if (acception == "нет")
    {
        return false;
    }
    return false;
}
void ShowAll(List<Book> books)                  // 1
{
    var totalPages = books.Sum(book => book.Pages);
    Console.WriteLine($"Всего страниц: {totalPages}");
    List<Book> sortedPages = books.OrderByDescending(book => book.Pages).ToList();
    foreach (Book book in sortedPages) { Console.WriteLine(book); }
    if (books.Count == 0) Console.WriteLine("Список пуст");
    Console.WriteLine();
}
void ShowByAuthor(List<Book> books)             // 2
{
    string author = ReadNonEmpty("Введите автора которого хотите почитать: ");
    List<Book> authorSort = FindByAuthor(books, author);
    if (authorSort.Count == 0)
    {
        Console.WriteLine("Книг этого автора не найдено");
    }
    else
    {
        foreach (Book book in authorSort)
        {
            Console.WriteLine(book);
        }
    }
    Console.WriteLine();
}
void ShowByTitle(List<Book> books)              // 3
{
    string title = ReadNonEmpty("Введите название книги которую хотите прочесть: ");
    List<Book> titleFounded = FindByTitle(books, title);
    if (titleFounded.Count == 0)
    {
        Console.WriteLine("Книга не найдена");
    }
    else
    {
        foreach (Book book in titleFounded)
        {
            Console.WriteLine(book);
        }
    }
    Console.WriteLine();
}
void ShowAuthorStats(List<Book> books)
{
    if (books.Count == 0)
    {
        Console.WriteLine("Список пуст");
        return;
    }
    var bookStats = new BookStats();
    var stats = bookStats.AuthorStats(books);
    foreach (var pair in stats)
    {
        Console.WriteLine($"{pair.Key}: {pair.Value}");
    }
}
bool AddBook(Library library)                  // 4
{
    Console.WriteLine("Введите название и автора");
    string title = ReadNonEmpty("Введите название: ");
    string author = ReadNonEmpty("Введите автора: ");
    if (library.Exists(title, author))
    {
        Console.WriteLine("Такая книга уже есть.");
        return false;
    }
    Console.Write("Введите количество страниц: ");
    int pages;
    while (!int.TryParse(Console.ReadLine(), out pages) || pages <= 0)
    {
        Console.WriteLine("Введите корректное значение");
    }
    var book = new Book(title, author, pages);
    if(!library.Add(book))
    {
        Console.WriteLine("Не удалось добавить книгу");
        return false;
    }
    Console.WriteLine($"Книга {title} добавлена");
    return true;
}
bool RemoveBook(Library library)               // 5
{
    var books = library.GetAll();
    string title = ReadNonEmpty("Введите название книги которую хотите удалить: ");
    List<Book> foundBooks = books.Where(book => book.Title.ToLower() == title.ToLower()).ToList();
    if (foundBooks.Count == 0)
    {
        Console.WriteLine("Книга не найдена");
        return false;
    }
    Book selected;
    if (foundBooks.Count == 1)
    {
        selected = foundBooks[0];
    }
    else
    {
        Console.WriteLine("Найдено несколько книг с таким названием:");
        for (int i = 0; i < foundBooks.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {foundBooks[i]}");
        }
        int choice = ReadIntInRange($"Выберите номер книги для удаления (1-{foundBooks.Count}): ", 1, foundBooks.Count);
        selected = foundBooks[choice - 1];
    }
    var removed = library.Remove(selected);
    if (removed)
    {
        Console.WriteLine($"Книга удалена: {selected}");
        return true;
    }
    else
    {
        Console.WriteLine("Не удалось удалить книгу");
        return false;
    }
}
int ReadIntInRange(string prompt, int min, int max)
{
    int result;
    while(true)
    {
        Console.Write(prompt);
        string input = Console.ReadLine()??"";
        if (int.TryParse(input, out result) && result >= min && result <= max)
        {
            return result;
        }
        Console.WriteLine($"Ошибка! Введите целое число от {min} до {max}.");
    }
}
string ReadNonEmpty(string prompt)
{
    Console.Write(prompt);
    while (true)
    {
        string input = Console.ReadLine() ?? "";
        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("Ошибка: введено пустое поле");
            Console.Write(prompt);
            continue;  
        }
        return input.Trim();
    }
}
/*
bool SaveBooks(List<Book> books)                // 6
{
    try
    {
        if (File.Exists("books.json"))
        {
            File.Copy("books.json", "books.json.bak", overwrite: true);
        }
        string json = JsonSerializer.Serialize(books, options);
        File.WriteAllText("books.json", json);
        Console.WriteLine($"Книг сохранено: {books.Count}");
        return true;
    }
    catch(IOException ex)
    {
        Console.WriteLine($"Ошибка записи файла: {ex.Message}");
        return false;
    }
    catch(UnauthorizedAccessException ex)
    {
        Console.WriteLine($"Ошибка доступа: {ex.Message}");
        return false;
    }
}

List<Book> LoadBooks()
{
    if (File.Exists("books.json"))
    {
        try
        {
            string json = File.ReadAllText("books.json");
            List<Book>? loaded = JsonSerializer.Deserialize<List<Book>>(json) ?? new List<Book> { };
            return loaded;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Ошибка чтения файла, файл поврежден: {ex.Message}");
            File.Copy("books.json", "books.broken.json", overwrite: true);
            Console.WriteLine("Копия поврежденного файла сохранена в books.broken.json");
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Ошибка чтения файла: {ex.Message}");
        }
    }
    else
    {
        Console.WriteLine("Файла нет, начинаем с пустого списка");
    }
    return new List<Book>();
}
*/






/*
if (File.Exists("books.txt"))
{
    string[] lines = File.ReadAllLines("books.txt");
    foreach (string line in lines)
    {
        string[] parts = line.Split(';');
        if (parts.Length != 3) continue;
        string title = parts[0].Trim();
        string author = parts[1].Trim();
        if (!int.TryParse(parts[2].Trim(), out int pages) || pages <= 0 || title == "" || author == "")
        {
            continue;
        }
        loaded.Add(new Book(title, author, pages));
    }
}
else { Console.WriteLine("Файл с книгами отсутствует"); }
return loaded;


/*
int minPages;
Console.WriteLine("Введите минимальное количество страниц: ");
while (!int.TryParse(Console.ReadLine(), out minPages))
{
    Console.WriteLine("Введите число");
    Console.WriteLine("Введите минимальное количество страниц: ");
}
List<Book> minPageBooks = FindByMinPages(books, minPages);
if (minPageBooks.Count == 0)
{
    Console.WriteLine("Ничего не найдено");
}
else
{
    foreach (Book book in minPageBooks)
    {
        book.PrintInfo();
    }
}



/*
public record Product(string Name, decimal Price);

class Program
{
    static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };
    static void Main()
    {

        var products = new List<Product>
        {
            new Product("Банан", 60m),
            new Product("Вишня", 100m),
            new Product("Клубника", 150m),
            new Product("Iphone 18 pro", 120000m),
            new Product("Кокос", 59.99m)
        };
        string path = "products.json";
        //SaveProducts(path, products);
        LoadProducts(path);
    }
    static void SaveProducts(string path, List<Product> products)
    {
        string json = JsonSerializer.Serialize(products, options);
        File.WriteAllText(path, json);
    }
    static List<Product> LoadProducts(string path)
    {
        try
        {
            string loadProducts = File.ReadAllText(path);
            List<Product>? loaded = JsonSerializer.Deserialize<List<Product>>(loadProducts) ?? new List<Product>();
            Console.WriteLine(loaded.Count);
            return loaded;
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("Файла не существет");
            return new List<Product>();
        }
        catch (JsonException ex)
        {
            Console.WriteLine(ex.Message);
            return new List<Product>();
        }
    }
}




/*
class Program
{
    static async Task Main()
    {
        static async Task<string> MakeCoffeeAsync()
        {
            await Task.Delay(2000);
            throw new InvalidOperationException("Кофемашина сломана");
        }
        static async Task<string> MakeToastAsync()
        {
            await Task.Delay(1500);
            return "Тосты готовы";
        }
        
        var time1 = Stopwatch.StartNew();
        var coffee1 = await MakeCoffeeAsync();
        var toast1 = await MakeToastAsync();
        Console.WriteLine($"Прошло: {time1.ElapsedMilliseconds} ms");
        
        var time2 = Stopwatch.StartNew();
        var coffee2 = MakeCoffeeAsync();
        var toast2 = MakeToastAsync();
        await Task.WhenAll(coffee2, toast2);
        //второй вариант быстрее потомучто здесь обе задачи выполняются одновременно, так сказать один повар отвечает за кофе другой 
        Console.WriteLine($"Прошло: {time2.ElapsedMilliseconds} ms");
        
        var coffee3 = MakeCoffeeAsync();
        var toast3 = MakeToastAsync();
        try
        {
            await Task.WhenAll(coffee3, toast3);
            Console.WriteLine($"Готово {coffee3}, {toast3}");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(ex.Message);
        }
        if (toast3.IsCompletedSuccessfully)
        {
            Console.WriteLine("Тост готов");
        }
    }
}
*/




/*
public record Product(string Name, decimal Price);
class ProductClass
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public ProductClass(string name,decimal price)
    {
        Name = name;
        Price = price;
    }
}
class Program
{
    static void Main()
    {
        
        var products = new List<Product>
        {
            new Product("Банан", 60m),
            new Product("Вишня", 100m),
            new Product("Клубника", 150m),
            new Product("Iphone 18 pro", 120000m)
        };
        File.WriteAllLines("products1.txt", products.Select(product=>$"{product.Name};{product.Price}"));
        string[] lines;
        try
        {
            lines = File.ReadAllLines("products2.txt");
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("Файл products.txt не найден!");
            return;
        }
        var productsTxt = new List<Product>();
        int skipped = 0;
        using var writer = new StreamWriter("log.txt", append: true);
        foreach(var line in lines)
        {
            string[] part = line.Split(';');
            if (part.Length != 2)
            {
                skipped++;
                writer.WriteLine($"{line} состоит не из 2 частей");
                break;
            }
            if (!decimal.TryParse(part[1],out decimal price))
            {
                skipped++;
                writer.WriteLine("Значение цены отсутствует");
                break;
            }
            productsTxt.Add(new Product(part[0], price));
        }
        Console.WriteLine($"Загружено: {productsTxt.Count}, пропущено: {skipped}");
    }
}
/*
        var orange = new Product("Апельсин", 50m);
        var orangE = new Product("Апельсин", 50m);
        Console.WriteLine(orange == orangE); // тут сравниваются два компонента конструктора потому выводится тру        var apple = new ProductClass("Яблоко", 30);
        var applE = new ProductClass("Яблоко", 30m);
        var apple = new ProductClass("Яблоко", 30m);
        Console.WriteLine(apple == applE); // а тут сравниваются apple и applE а не то что в них лежит
        Console.WriteLine(orange);
        var orangeSale = orange with { Price = orange.Price * 0.9m };
        Console.WriteLine($"{orange.Price:F2}");
        Console.WriteLine($"{orangeSale.Price:F2}");
        */
/*
        var totalPrice = products.Sum(product=>product.Price);
        Console.WriteLine($"{totalPrice:F2}");
        var mostExpensive = products.MaxBy(product => product.Price);
        Console.WriteLine(mostExpensive);
        var maxPrice = 120m;
        var lowPrice = products
            .Where(product => product.Price < maxPrice)
            .Select(product=> product.Name);
        foreach (var name  in lowPrice)
        {
            Console.WriteLine(name);
        }
        */




/*
enum OrderStatus
{
    New,
    Paid,
    Shipped,
    Delivered,
    Canceled
}
class Program
{
    static void Main()
    {
        Console.WriteLine(GetStatusText(OrderStatus.New));
        Console.WriteLine(GetStatusText(OrderStatus.Paid));
        Console.WriteLine(GetStatusText(OrderStatus.Shipped));
        Console.WriteLine(GetStatusText(OrderStatus.Delivered));
        Console.WriteLine(GetStatusText(OrderStatus.Canceled));
        Console.WriteLine(GetAgeGroup(-1));
        Console.WriteLine(GetAgeGroup(5));
        Console.WriteLine(GetAgeGroup(12));
        Console.WriteLine(GetAgeGroup(18));
        Console.WriteLine(GetAgeGroup(65));
        foreach (var status in Enum.GetValues<OrderStatus>())
        {
            Console.WriteLine(GetStatusText(status));
        }
    }
    static string GetStatusText(OrderStatus status) => status switch
    {
        OrderStatus.New => "Новый",
        OrderStatus.Paid => "Оплачен",
        OrderStatus.Shipped => "Погружен",
        OrderStatus.Delivered => "Доставлен",
        OrderStatus.Canceled => "Отменен",
        _ => throw new ArgumentException("Неизвестная единица", nameof(status))
    };
    static string GetAgeGroup(int age) => age switch
    {
        < 0 => "Ошибка",
        < 12 => "ребенок",
        < 18 => "Подросток",
        < 65 => "Взрослый",
        _ => "Пожилой"
    };
}















/*
class Programm
{
    static void Main()
    {
        string text = "to be or not to be";
        string[] words = text.Split(' ');
        var stock = new Dictionary<string, int>();
        foreach (var word in words)
        {
            if (stock.TryGetValue(word, out int value))
            {
                stock[word] = value + 1;
            }    
            else
            {
                stock[word] = 1;
            }
        }
        foreach (var (name,count) in stock)
        {
            Console.WriteLine($"{name}: {count}");
        }
        var biggest = stock.MaxBy(pair => pair.Value);
        Console.WriteLine(biggest);
    }
}










/*
class Programm
{
    static int ParseAge(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("Строка не должна быть пустой");
        }
        if (!int.TryParse(input, out int age))
        {
            throw new FormatException("Строка не является числом");
        }
        if (age < 0 || age > 150)
        {
            throw new ArgumentOutOfRangeException("age", "Возраст вне диапазона");
        }
        return age;
    }
    static void Main()
    {
        string?[] ages = { "25", "abc", "", "-5", "200", null };
        foreach (var age in ages)
        {
            try
            {
                int agE = ParseAge(age);
                Console.WriteLine($"Хорошо, {age} = {agE}");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine($"{age} вне диапазона: {ex.Message}");
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"{age} {ex.Message}");
            }
            catch(ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}








/*
class Customer
{
    public string Name { get; set; }
    public string? Email { get; set; }
    public Customer(string name, string? email = null)
    {
        Name = name;
        Email = email;
    }
    public string GetContact() => Email ?? "нет Email";
}
class Program
{
    static void Main()
    {
        var clients = new List<Customer>
        {
            new Customer("Anna"),
            new Customer("Anna","anna228@mail.ru"),
            new Customer("Oleg","oleg1456@gmail.com"),
            new Customer("Vanya")
        }; 
        foreach (var client in clients)
        {
            Console.WriteLine($"{client.Name}: {client.GetContact()}");
        }
        var emailCount = clients.Count(email => email.Email is not null);
        Console.WriteLine(emailCount);
        var longestEmail = clients.Max(email => email.Email?.Length);
        Console.WriteLine(longestEmail);
    }
}









/*
interface IPayable
{
    decimal GetPayment();
}
class Employee : IPayable
{
    public decimal Salary { get; protected set; }
    public Employee(decimal salary)
    {
        Salary = salary;
    }
    public decimal GetPayment() => Salary;
}
class Invoice : IPayable
{
    public decimal Amount { get; protected set; }
    public Invoice(decimal amount)
    {
        Amount = amount;
    }
    public decimal GetPayment() => Amount;
}
class Program
{
    static void Main()
    {
        var toPay = new List<IPayable>
        {
            new Employee(20000m),
            new Employee(15000m),
            new Invoice(30000m)
        };
        foreach (var payment in toPay) { Console.WriteLine(payment.GetPayment()); }
        var sumPay = toPay.Sum(e => e.GetPayment());
        Console.WriteLine(sumPay);
    }
}




/*
class Employee
{
    public string Name { get; }
    public double BaseSalary { get; protected set; }
    public Employee(string name, double baseSalary)
    {
        Name = name;
        BaseSalary = baseSalary;
    }
    public virtual double CalculateSalary()
    { return BaseSalary; }
}
class Manager : Employee
{
    public double Bonus { get; protected set; }
    public Manager(string name, double baseSalary, double bonus) : base(name, baseSalary) 
    {
        Bonus = bonus;
    }
    public override double CalculateSalary() => base.CalculateSalary() + Bonus;
}

class Program
{
    static void Main()
    {
        var Employees = new List<Employee>
        {
            new Manager("Anna", 15500.5, 5000),
            new Manager("Andrew", 20000.5, 5000.5),
            new Employee("David", 15500)
        };
        foreach (var employee in Employees)
        {
            Console.WriteLine(employee.Name);
            Console.WriteLine(employee.CalculateSalary());
        }
        double totalSalary = Employees.Sum(baseSalary => baseSalary.CalculateSalary());
        Console.WriteLine(totalSalary);
    }
}











/*
class Student
{
    public string Name { get; set; }
    public int Age { get; init; }
    public Student(string name, int age)
    {
        Name = name;
        Age = age;
    }
    public bool IsAdult() { return Age >= 18; }
}
class Program
{
    static void Main()
    {
        var anna = new Student("Anna", 18);
        var oleg = new Student("Oleg", 17);
        var nikita = new Student("Nikita", 19);
        var olga = new Student("Olga", 16);
        var students = new List<Student> { anna, oleg, nikita, olga };
        var namesAdult = students.Where(a => a.IsAdult()).Select(s => s.Name);
        foreach (var s in namesAdult)
        {
            Console.WriteLine(s);
        }
    }
}







//var ages = new List<int> { 15, 18 ,22 ,7,30,44 };



/*
List<int> adult = ages.Where(a => a >= 18).ToList();
foreach (int age in adult)
{
    Console.WriteLine(age);
}
Console.WriteLine(ages.Count(IsAdult));

/*
int adult = 0;
foreach(var age in ages)
{
    if(IsAdult(age))
    {
        adult++;
    }
}
Console.WriteLine($"Совершеннолетних: {adult}");
*/

//if (IsAdult(age)) { Console.WriteLine("adult"); } else { Console.WriteLine("Not"); }
//Console.WriteLine(IsAdult(15) + " " + IsAdult(18) + " " + IsAdult(34));