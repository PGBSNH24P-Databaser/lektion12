---
author: Lektion 12
date: MMMM dd, YYYY
paging: "%d / %d"
---

# Lektion 12

Hej och välkommen!

## Agenda

1. Frågor och repetition
2. Introduktion till ORMs
3. Projektbygge (todo app)
4. Eget arbete med handledning

---

# Introduktion till ORMs

Object-relational mapping, förkortat ORM, är ett verktyg som utvecklare kan använda för att förenkla databashantering.

- Skapar tabeller (CREATE TABLE)
- Automatiserar queries (SELECT, INSERT, DELETE)
- Förenklar villkor och joins
- Hanterar versioner av tabeller

---

# Entity Framework (EF)

Entity Framework är en ORM för C# som har stöd för det mesta.

- Klasser bildar modeller som bildar tabeller
- Migrations för att uppdatera tabeller
- Funktioner för CRUD
- Stöd för relationer och joins

```sh
# Installera och kom igång med EF i ett projekt
dotnet tool install --global dotnet-ef
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Design
```

<https://learn.microsoft.com/en-us/ef/core/get-started/overview/install>

---

# DbContext

- Ansluter till databas
- Kommunicerar med databas (queries)
- Hanterar relationer och joins automatiskt

---

# Exempel DbContext

```csharp
public class Todo // Model
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
}

public class AppContext : DbContext
{
    public DbSet<Todo> Todos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.UseNpgsql("Host=localhost;Database=todos;Username=postgres;Password=password");
    }
}
```

---

# Migrations

- Beskrivning av tabeller
- Håller koll på historik och versioner
- Skapar och uppdaterar tabeller

```sh
# Skapa en migration och uppdatera databasen
dotnet ef migrations add [namn]
dotnet ef database update
```

---

# Ett enkelt exempel

```csharp
using var db = new AppContext();

var todo = new Todo { Title = "Städa" };

// INSERT: Spara todo
db.Todos.Add(todo);
db.SaveChanges();

// DELETE: Radera todo
db.Todos.Remove(todo);
db.SaveChanges();

// SELECT: Hämta todos
var todos = db.Todos.Where(todo => todo.Title.Contains("a"));
foreach (var todo in todos)
{
    Console.WriteLine(todo.Title);
}
```
