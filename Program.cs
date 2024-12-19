using Microsoft.EntityFrameworkCore;

// 1. Skapa model som skall bilda en tabell i databasen
public class Todo
{
    public int Id { get; set; }

    public string Title { get; set; }
    public bool Completed { get; set; }
}

// 2. Skapa en DbContext som representerar kopplingen till databasen.
// DbContext har koll på vilka tabelelr som skall skapas och
// innehåller funktioner på saker som kan göras - CRUD.
public class AppContext : DbContext
{
    // 3. EF representation av en tabell. Denna rad bestämmer att det skall finnas "todos" tabell.
    public DbSet<Todo> Todos { get; set; }

    // 4. Konfigurera EF att använda PostgreSQL specifikt (genom att ange connection string)
    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.UseNpgsql("Host=localhost;Database=todos;Username=postgres;Password=password");
    }
}

class Program
{
    static void Main(string[] args)
    {
        // 5. Hjälp text i början av program
        Console.WriteLine("Welcome to the todo application.");
        Console.WriteLine("create-todo - Create and save todos to database.");
        Console.WriteLine("delete-todo - Remove todos from database.");
        Console.WriteLine("list-todos  - View all todos.");

        // 6. Implementera kommandosystem
        string command = Console.ReadLine()!;
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

    // 7. Denna funktion bestämmer vad som skall hända när vi kör "create-todo" kommandot.
    static void CreateTodo()
    {
        Console.Write("Enter a title: ");
        string title = Console.ReadLine()!;

        var todo = new Todo
        {
            Title = title,
            Completed = false,
        };

        // 8. Skapa 'AppContext' för att kunna använda databasen
        // Den kommer att ansluta till databasen genom connection string.
        using var context = new AppContext();

        // 9. Spara en todo till databasen (den gör en INSERT i bakgrunden)
        // (Se rad 84)
        context.Todos.Add(todo);

        // 10. I EF måste vi anropa "SaveChanges" för att faktiskt få ändringar att ske.
        // (.Add av sig självt gör ingenting)
        // context.Todos.Add(todo); + context.SaveChanges(); är som en commit + push (i git termer.)
        // Add = commit
        // Push = SaveChanges
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

        // 14. Radera rader (todos) baserat på villkor. Denna exekveras direkt och kräver ingen "SaveChanges".
        context.Todos.Where(todo => todo.Id == id).ExecuteDelete();

        Console.WriteLine("Removed todo from database.");
    }

    // 15. Denna funktion bestämmer vad som skall hända när vi kör "list-todos" kommandot.
    static void ListTodos()
    {
        // 16. Skapa 'AppContext' för att kunna använda databasen
        // Den kommer att ansluta till databasen genom connection string.
        using var context = new AppContext();

        // 17. Hämta alla rader (todos) med .ToList (gör 'SELECT * FROM todos' i bakgrunden) och hämta som lista
        var todos = context.Todos.ToList();

        foreach (var todo in todos)
        {
            Console.WriteLine($"- {todo.Id} {todo.Title}");
            Console.WriteLine("  Completed: " + (todo.Completed ? "Yes" : "No"));
        }
    }
}