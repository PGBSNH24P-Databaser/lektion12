using Microsoft.EntityFrameworkCore;

// 1. Skapa model som skall bilda en tabell i databasen
public class Todo
{
    public int Id { get; set; }

    public string Title { get; set; }
    public bool Completed { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow; // PostgresSQL kräver UTC tidszon

    // 20. Vi vill att användare skall ha todos
    //  och då kan man med EF lägga in en egenskap User (detta bildar en Many-to-One)
    public User User { get; set; }
}

// 18. Skapa model som skall bilda en tabell i databasen
public class User
{
    public int Id { get; set; }

    public string Name { get; set; }
    public string Password { get; set; }

    // 19. Vi vill att användare skall ha todos
    //  och då kan man med EF lägga in en egenskap List<Todo> (detta bildar en One-to-Many)

    public List<Todo> Todos { get; set; }
}

// 2. Skapa en DbContext som representerar kopplingen till databasen.
// DbContext har koll på vilka tabelelr som skall skapas och
// innehåller funktioner på saker som kan göras - CRUD.
public class AppContext : DbContext
{
    // 3. EF representation av en tabell. Denna rad bestämmer att det skall finnas en "todos" tabell.
    public DbSet<Todo> Todos { get; set; }

    // 21. EF representation av en tabell. Denna rad bestämmer att det skall finnas en "users" tabell.
    public DbSet<User> Users { get; set; }

    // 4. Konfigurera EF att använda PostgreSQL specifikt (genom att ange connection string)
    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.UseNpgsql("Host=localhost;Database=todos;Username=postgres;Password=password");
    }

    /*
    Om man vill konfigurera modellerna lite extra: fler constraints exempelvis, då kan man använda 'OnModelCreating'.
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Todo>()
            .Property(todo => todo.Title)
            .IsRequired();
    }
    */
}

class Program
{
    // Denna håller koll på vem som är inloggad.
    // Om den är -1 betyder det att man inte har loggat in än.
    public static int loggedInUserId = -1;

    static void Main(string[] args)
    {
        // 22. Hjälp text i början av program
        Console.WriteLine("Welcome to the todo application.");
        Console.WriteLine("register - Create and save a user account.");
        Console.WriteLine("login    - Login to the application.");

        // 22.5. Implementera kommandosystem kopplat till användare (login & register)
        string command = Console.ReadLine()!;
        while (!command.Equals("exit"))
        {
            if (command.Equals("register"))
            {
                RegisterUser();
            }
            else if (command.Equals("login"))
            {
                if (Login())
                {
                    break;
                }
            }
            else
            {
                Console.WriteLine("No such command exists, please try again.");
            }

            command = Console.ReadLine()!;
        }

        // 5. Hjälp text i början av program
        Console.WriteLine("create-todo - Create and save todos to database.");
        Console.WriteLine("delete-todo - Remove todos from database.");
        Console.WriteLine("list-todos  - View all todos.");

        // 6. Implementera kommandosystem
        command = Console.ReadLine()!;
        while (!command.Equals("exit"))
        {
            if (command.Equals("create-todo"))
            {
                CreateTodo();
            }
            else if (command.Equals("delete-todo"))
            {
                DeleteTodo();
            }
            else if (command.Equals("list-todos"))
            {
                ListTodos();
            }
            else
            {
                Console.WriteLine("No such command exists, please try again.");
            }

            command = Console.ReadLine()!;
        }
    }

    // 23. Denna funktion bestämmer vad som skall hända när vi kör "register" kommandot.
    static void RegisterUser()
    {
        Console.Write("Enter a username: ");
        string username = Console.ReadLine()!;

        Console.Write("Enter a password: ");
        string password = Console.ReadLine()!;

        // Vi skippar Id så att den genereras automatiskt av EF
        var user = new User
        {
            Name = username,
            Password = password,
            Todos = new List<Todo>(),
        };

        using var context = new AppContext();

        context.Users.Add(user);
        context.SaveChanges();

        Console.WriteLine("Created user. Try logging in.");
    }

    // 24. Denna funktion bestämmer vad som skall hända när vi kör "register" kommandot.
    static bool Login()
    {
        Console.Write("Enter a username: ");
        string username = Console.ReadLine()!;

        Console.Write("Enter a password: ");
        string password = Console.ReadLine()!;

        using var context = new AppContext();

        // 25. Vi vill hämta (en) användare men matchande användarnamn och läsenord
        //  för att "logga in".
        // Om ingen användare matchar så kommer denna rad att kasta en exception och krascha programmet.
        var user = context.Users.Where(user => user.Name.Equals(username) && user.Password.Equals(password)).First();

        // Markera användare som inloggad
        loggedInUserId = user.Id;

        Console.WriteLine("You have logged in!");

        // Return true för att indikera att vi har lyckats logga in.
        return true;
    }

    // 7. Denna funktion bestämmer vad som skall hända när vi kör "create-todo" kommandot.
    static void CreateTodo()
    {
        Console.Write("Enter a title: ");
        string title = Console.ReadLine()!;

        // 8. Skapa 'AppContext' för att kunna använda databasen
        // Den kommer att ansluta till databasen genom connection string.
        // using var context = new AppContext();
        using var context = new AppContext();

        // 26. Anslut till databas och koppla på en användare
        //  vi börjar genom att hämta användaren som är inloggat från databasen
        //  context.Users.Find(loggedInUserId) hämtar hela raden (användaren) som är inloggad (baserat på id).
        var user = context.Users.Find(loggedInUserId);
        if (user == null)
        {
            Console.Write("You are not logged in.");
            return;
        }

        var todo = new Todo
        {
            Title = title,
            Completed = false,
            // 27. Koppla på användaren så att en relation bildas
            User = user,
        };

        // 9. Spara en todo till databasen (den gör en INSERT i bakgrunden)
        // (Se rad 84)
        context.Todos.Add(todo);

        // 27. Man kan, men måste inte, lägga in todo i användarens lista också.
        user.Todos.Add(todo);

        // 10. I EF måste vi anropa "SaveChanges" för att faktiskt få ändringar att ske.
        // (.Add av sig självt gör ingenting)
        // context.Todos.Add(todo); + context.SaveChanges(); är som en commit + push (i git termer.)
        // Add = commit
        // Push = SaveChanges
        // 28. Denna rad kommer också att spara ändringen i User modellen (den som är inloggad) - på grund av user.Todos.Add(todo);
        context.SaveChanges();

        Console.WriteLine("Saved todo to database.");
    }

    // 11. Denna funktion bestämmer vad som skall hända när vi kör "delete-todo" kommandot.
    static void DeleteTodo()
    {
        // 12. Ta in ett id på en todo
        Console.Write("Enter an id: ");
        string idString = Console.ReadLine()!;
        int id = int.Parse(idString);

        // 13. Skapa 'AppContext' för att kunna använda databasen
        // Den kommer att ansluta till databasen genom connection string.
        using var context = new AppContext();

        // Alternativ: context.Todos.Remove();

        // 14. Radera rader (todos) baserat på villkor. Denna exekveras direkt och kräver ingen "SaveChanges".
        // 29. Lägg till ett villkor så att vi bara kan radera användarens todos, men ingen annans todos.
        //  kolla även så att det finns en todo att radera.
        if (context.Todos.Where(todo => todo.Id == id && todo.User.Id == loggedInUserId).ExecuteDelete() == 0)
        {
            Console.WriteLine("Could not remove todo.");
            return;
        }

        Console.WriteLine("Removed todo from database.");
    }

    // 15. Denna funktion bestämmer vad som skall hända när vi kör "list-todos" kommandot.
    static void ListTodos()
    {
        // 16. Skapa 'AppContext' för att kunna använda databasen
        // Den kommer att ansluta till databasen genom connection string.
        using var context = new AppContext();

        // 17. Hämta alla rader (todos) med .ToList (gör 'SELECT * FROM todos' i bakgrunden) och hämta som lista
        var todos = context.Todos
            .Include(todo => todo.User) // 30. Inkludera användardata i SELECT, vilket gör en JOIN i bakgrunden. Detta så att vi får tillgång till användarnamnet på rad 266
            .Where(todo => todo.User.Id == loggedInUserId); // 31. Hämta bara alla todos tillhörande inloggad användare

        // var todos = context.Todos.Where(todo => todo.Completed == true);

        foreach (var todo in todos)
        {
            Console.WriteLine($"- {todo.Id} {todo.Title}");
            Console.WriteLine("  Completed: " + (todo.Completed ? "Yes" : "No"));
            Console.WriteLine("  Username: " + todo.User.Name);
        }
    }
}