# SystemZarzadzaniaSchroniskiem

**SystemZarzadzaniaSchroniskiem** to aplikacja webowa wspierająca pracę schronisk dla zwierząt. Umożliwia obsługę procesów związanych z rejestracją, adopcjami, ewidencją zwierząt oraz zarządzaniem personelem i wolontariuszami. System został stworzony w C# z użyciem ASP.NET Core MVC oraz Entity Framework.

## Technologie

- [![C#](https://img.shields.io/badge/Language-C%23-blue)](https://learn.microsoft.com/en-us/dotnet/csharp/)
- [![.NET 8.0](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [![ASP.NET Core MVC](https://img.shields.io/badge/ASP.NET%20Core%20MVC-Framework-lightblue)](https://learn.microsoft.com/en-us/aspnet/core/mvc/)
- [![EntityFramework Core](https://img.shields.io/badge/EntityFramework%20Core-ORM-green)](https://learn.microsoft.com/en-us/ef/core/)
- [![Microsoft SQL Server](https://img.shields.io/badge/Microsoft%20SQL%20Server-Database-blue)](https://www.microsoft.com/en-us/sql-server)
- [![Microsoft SQLite](https://img.shields.io/badge/Microsoft%20SQLite-Database-lightgrey)](https://www.sqlite.org/)
- [![Visual Studio](https://img.shields.io/badge/Visual%20Studio-IDE-blueviolet)](https://visualstudio.microsoft.com/)
- [![JSON](https://img.shields.io/badge/JSON-Data%20Format-yellow)](https://www.json.org/json-en.html)
- [![HTML](https://img.shields.io/badge/HTML-Marked%20Language-orange)](https://developer.mozilla.org/en-US/docs/Web/HTML)
- [![CSS](https://img.shields.io/badge/CSS-Style%20Sheet-blue)](https://developer.mozilla.org/en-US/docs/Web/CSS)
- [![JavaScript](https://img.shields.io/badge/JavaScript-Scripting%20Language-yellowgreen)](https://developer.mozilla.org/en-US/docs/Web/JavaScript)
- [![Bootstrap](https://img.shields.io/badge/Bootstrap-UI%20Framework-pink)](https://getbootstrap.com/)

## Instalacja

Aby zainstalować projekt na swoim lokalnym komputerze musisz posiadać Microsoft SQL Server oraz Visual Studio 2022 :

1. **Sklonuj repozytorium:**

   ```bash
   git clone https://github.com/chomiczo/SystemZarzadzaniaSchroniskiem.git

2. **Uruchomienie**

   ```bash
   add-migration nazwa_migracji
   Update-Database
W przypadku, gdy po sklonowaniu repozytorium w folderze Migrations znajdują się jakieś pliki należy pierw je usunąć a następnie wykonać czynności opisane w punkcie 2.

## Dodatkowe informacje
W razie napotkanych problemów można usunąć migracje wraz ze snapshotem bazy, a następnie samą bazę.
W kolejnym kroku ponownie wykonać ponownie wyżej wymienione komendy w punkcie 2.

## Autor
Aleksander Chomicz
