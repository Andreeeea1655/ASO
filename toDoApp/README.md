# 📋 TODO App — Circular Buffer

Aplicație TODO completă cu **ASP.NET Core 8** backend, **SQLite** bază de date, și **Circular Buffer** pentru managementul task-urilor.

---

## 🏗 Structura proiectului

```
TodoApp/
├── TodoApp.sln
├── .vscode/
│   ├── launch.json       ← debug config
│   ├── tasks.json        ← build tasks
│   └── extensions.json   ← extensii recomandate
└── TodoApi/
    ├── TodoApi.csproj
    ├── Program.cs            ← entry point, DI, middleware
    ├── appsettings.json
    ├── Controllers/
    │   └── TodosController.cs   ← REST endpoints
    ├── Models/
    │   └── TodoItem.cs          ← entitate + DTO-uri
    ├── Data/
    │   └── TodoDbContext.cs     ← EF Core + SQLite
    ├── Services/
    │   └── TodoCircularBufferService.cs  ← logica buffer-ului
    └── wwwroot/
        └── index.html           ← frontend single-page
```

---

## Cum rulezi

### Cerințe
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- VS Code + extensia **C# Dev Kit** (`ms-dotnettools.csdevkit`)

### Pași

```bash
# 1. Clonează / extrage arhiva
cd TodoApp

# 2. Restore pachete
dotnet restore TodoApi/TodoApi.csproj

# 3. Rulează
dotnet run --project TodoApi/TodoApi.csproj
```

Sau în **VS Code**: apasă `F5` → selectează *"🚀 Run TodoApi"*

### Accesează aplicația
| URL | Descriere |
|-----|-----------|
| http://localhost:5000 | Frontend |
| http://localhost:5000/swagger | Swagger UI (testare API) |

---

## 🔄 Circular Buffer — Cum funcționează

Capacitatea maximă este **10 TODO-uri**.

Când adaugi al 11-lea, sistemul aplică **strategia de evicție**:

```
1. Sunt todos COMPLETATE?
   → DA: evictează cel mai vechi todo completat
   → NU: continuă

2. Evictează todo-ul cu PRIORITATEA CEA MAI MICĂ
   + cel mai VECHI dintre cele cu aceeași prioritate
```

Aceasta este o implementare tip **Priority Queue** pentru evicție:
- `Priority 1 (LOW)` → primii la eliminat
- `Priority 3 (HIGH)` → ultimii la eliminat

---

## 📡 REST API Endpoints

| Method | URL | Descriere |
|--------|-----|-----------|
| `GET` | `/api/todos` | Toate todo-urile |
| `GET` | `/api/todos/{id}` | Un todo specific |
| `GET` | `/api/todos/stats` | Statistici buffer |
| `POST` | `/api/todos` | Adaugă todo (cu evicție dacă e plin) |
| `PUT` | `/api/todos/{id}` | Editează todo |
| `DELETE` | `/api/todos/{id}` | Șterge todo |

### Exemplu POST body
```json
{
  "title": "Finalizez tema ASP.NET",
  "description": "Deadline vineri",
  "priority": 3
}
```

### Exemplu răspuns GET /api/todos/stats
```json
{
  "currentCount": 8,
  "maxCapacity": 10,
  "completedCount": 2,
  "pendingCount": 6,
  "isAtCapacity": false,
  "fillPercent": 80.0
}
```

---

## 🗄 Baza de date

SQLite — fișierul `todos.db` este creat automat la prima rulare în directorul `TodoApi/`.

Schema tabelului `Todos`:
```sql
CREATE TABLE Todos (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    Title       TEXT    NOT NULL,
    Description TEXT,
    IsCompleted INTEGER NOT NULL DEFAULT 0,
    Priority    INTEGER NOT NULL DEFAULT 1,
    CreatedAt   TEXT    NOT NULL DEFAULT (datetime('now')),
    CompletedAt TEXT
);
```

---

## 🎨 Frontend

Single-page app servit direct de ASP.NET Core din `wwwroot/index.html`.
- Vanilla JS, fără framework
- Dark theme modern
- Buffer bar în header arată capacitatea în timp real
- Warning automat când buffer-ul e plin
