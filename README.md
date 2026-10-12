# Imparare .NET sviluppando

> **Da C# ad ASP.NET Core e Blazor, costruendo passo dopo passo un'applicazione web completa**

Questo repository contiene tutti gli esempi di codice e le esercitazioni svolte del libro **Imparare .NET sviluppando**.

Il libro parte dalle basi di .NET e C# e, capitolo dopo capitolo, fa evolvere una semplice applicazione di ticketing fino ad arrivare a una Web App completa realizzata con ASP.NET Core e Blazor.

L'obiettivo di questo repository è permettere di:

- recuperare il codice funzionante associato a ogni capitolo;
- confrontare il proprio codice con quello sviluppato nel libro;
- partire dall'esempio di un capitolo senza dover ricostruire tutti i passaggi precedenti;
- verificare la soluzione degli esercizi proposti.

Il repository può essere consultato anche senza il libro, ma gli esempi sono pensati come supporto al percorso descritto nei capitoli: il codice mostra il risultato, mentre nel libro vengono spiegati i concetti, le scelte e il motivo per cui l'applicazione evolve in quel modo.

## Il libro

**Imparare .NET sviluppando** è pensato sia per chi vuole iniziare a programmare con .NET e C#, sia per chi conosce già C# e vuole avvicinarsi ad ASP.NET Core e Blazor.

Il percorso non vuole essere un manuale esaustivo di ogni tecnologia. L'idea è introdurre gli argomenti quando diventano utili per risolvere un problema concreto e costruire, un passo alla volta, un'applicazione completa.

👉 **Acquista il libro su Amazon:** `[LINK AMAZON - DA INSERIRE]`

## Come è organizzato il repository

Per ogni capitolo che contiene codice è presente una cartella dedicata.

La struttura è normalmente questa:

- `src`: il codice compilabile mostrato o costruito nel capitolo;
- `exercise`: il codice funzionante che include anche la soluzione dell'esercizio proposto a fine capitolo.

In questo modo puoi leggere il capitolo lavorando sul progetto presente in `src` e, una volta completato l'esercizio, confrontare il risultato con la versione presente in `exercise`.

Il codice e il naming sono in inglese. I progetti usano `net11.0`, nullable reference types e implicit usings.

## Versioni degli esempi

Il testo del libro usa la versione finale .NET 11. Il codice allegato mantiene
la combinazione RC usata durante la preparazione degli esempi:

| Componente | Versione |
|---|---|
| SDK .NET | `11.0.100-rc.1.26425.128` |
| Package Microsoft ASP.NET Core, EF Core e Dependency Injection | `11.0.0-rc.1.26425.128` |
| `dotnet-ef` | `11.0.0-rc.1.26425.128` |
| Microsoft.Data.SqlClient | `6.1.3` |
| Dapper | `2.1.72` |
| Scalar.AspNetCore | `2.1.7` |
| Microsoft.NET.Test.Sdk | `17.14.1` |
| xUnit / runner Visual Studio | `2.9.3` / `3.1.5` |

`global.json` seleziona l'SDK e usa `latestPatch`: una feature band successiva
non soddisfa automaticamente questo vincolo. `Directory.Packages.props`
centralizza le versioni NuGet: i `PackageReference` dei progetti non hanno
quindi un attributo `Version`. Il manifest `.config/dotnet-tools.json` fissa
la versione locale di `dotnet-ef`. Dalla radice del repository esegui:

```powershell
dotnet --version
dotnet tool restore
dotnet ef --version
```

Conserva questi tre file nelle directory superiori ai progetti anche se copi
un solo checkpoint. Se distribuisci separatamente la cartella `samples`,
copia al suo interno `global.json`, `Directory.Packages.props` e la cartella
`.config` della radice. Per aggiornare alla versione finale occorre aggiornare
insieme SDK, versioni centralizzate e manifest dello strumento, poi verificare
nuovamente build e test.

## Capitoli disponibili

| Cartella | Contenuto principale |
|---|---|
| `chapter-02-first-program` | File-based app `FirstDotnetApp/hello.cs`, primo progetto console e ticketing console base. |
| `chapter-03-types-variables` | Tipi, condizioni, cicli, categoria e ore stimate. |
| `chapter-04-methods` | Estrazione in metodi e decisione `RequiresEvaluation`. |
| `chapter-05-debugging` | Bug logico da osservare con debugger e correzione. |
| `chapter-06-classes` | `Ticket`, incapsulamento, snapshot e assegnazione operatore. |
| `chapter-07-inheritance` | Gerarchia di ticket e formatter polimorfici. |
| `chapter-08-generics` | `TicketRegistry<TTicket>`, collezioni, queue, stack e set. |
| `chapter-09-lambda-linq` | Eventi, lambda, extension method e query LINQ. |
| `chapter-10-async` | Eccezioni, `Task`, `await`, cancellazione e gateway simulato. |
| `chapter-11-organization` | `Ticketing.Application` e `Ticketing.ConsoleApp`. |
| `chapter-12-dependency-injection` | Dependency Injection e lifetime dei servizi. |
| `chapter-13-ado-net` | Repository ADO.NET e script `database/create-database.sql`. |
| `chapter-14-dapper` | `ITicketRepository`, Dapper e implementazioni confrontabili. |
| `chapter-15-entity-framework-core` | Entity Framework Core, `TicketingDbContext` e factory. |
| `chapter-17-web-api` | ASP.NET Core Minimal API con endpoint `/api/tickets`. |
| `chapter-18-config-logging` | Configurazione, logging e gestione degli errori. |
| `chapter-19-openapi` | OpenAPI e documentazione delle API. |
| `chapter-20-blazor` | Blazor Interactive Server, lista, tabella e summary. |
| `chapter-21-forms` | `EditForm`, validazione e pagina create/edit. |
| `chapter-22-authentication` | Cookie authentication, policy `ManageTickets` e route protette. |
| `chapter-23-tests` | Test xUnit applicativi e web. |
| `chapter-24-final-application` | Applicazione completa costruita durante il percorso. |

I capitoli 00, 01 e 16 sono principalmente concettuali e non hanno quindi una cartella dedicata. Il capitolo 05 è invece presente perché il sample contiene volutamente un errore logico da individuare e correggere usando il debugger.

## Esercizi aggiuntivi per chi è agli inizi

Nelle cartelle dei capitoli 3, 4, 6 e 7 trovi anche un file `exercises.md` con esercizi aggiuntivi a difficoltà graduale. Sono pensati per aiutarti a trasformare i concetti letti nel libro in pratica: capire un problema, dividerlo in piccoli passi e tradurre il ragionamento in codice. Le consegne usano situazioni semplici e varie, anche indipendenti dall'applicazione di ticketing.

| File | Esercizi | Argomenti |
|---|---|---|
| [Capitolo 3](chapter-03-types-variables/exercises.md) | 40 | Variabili, condizioni, cicli, stringhe e array, fino all'inversione di una stringa e all'ordinamento di un array. |
| [Capitolo 4](chapter-04-methods/exercises.md) | 20 | Metodi, parametri, valori di ritorno e scomposizione di piccoli problemi in operazioni riutilizzabili. |
| [Capitolo 6](chapter-06-classes/exercises.md) | 10 | Classi, oggetti, costruttori, proprietà e incapsulamento. |
| [Capitolo 7](chapter-07-inheritance/exercises.md) | 10 | Ereditarietà, polimorfismo e interfacce. |

Ogni esercizio include una consegna, esempi di risultati attesi e una soluzione completa in una sezione espandibile, inizialmente chiusa nei visualizzatori compatibili. Prova prima a risolverlo da solo, esegui il codice con i dati suggeriti e verifica anche i casi limite. Poi apri la soluzione e confronta il ragionamento: il tuo codice può essere diverso e funzionare altrettanto bene.

All'inizio di ogni file trovi le istruzioni per lavorare in un progetto console separato e provare un esercizio alla volta. Se ti blocchi, segui su carta come cambiano le variabili a ogni passaggio; dopo aver consultato la soluzione, prova a riscriverla senza guardarla e a cambiare i dati di partenza.

## Come usare gli esempi

Puoi clonare il repository e lavorare direttamente con la .NET CLI, Visual Studio Code o Visual Studio.

Compilare un singolo sample:

```text
dotnet build chapter-17-web-api/exercise/Ticketing.Web/Ticketing.Web.csproj
```

Eseguire la file-based app del capitolo 2:

```text
dotnet run --file chapter-02-first-program/src/FirstDotnetApp/hello.cs
```

Eseguire una console application:

```text
dotnet run --project chapter-10-async/exercise/Ticketing.ConsoleApp/Ticketing.ConsoleApp.csproj
```

Avviare un'applicazione web:

```text
dotnet run --project chapter-20-blazor/exercise/Ticketing.Web/Ticketing.Web.csproj
```

Eseguire i test:

```text
dotnet test chapter-23-tests/src/Ticketing.Application.Tests/Ticketing.Application.Tests.csproj
dotnet test chapter-23-tests/src/Ticketing.Web.Tests/Ticketing.Web.Tests.csproj
```

## Database e configurazione

I sample non contengono connection string reali.

L'ambiente di riferimento è quello utilizzato nel libro: un'istanza di SQL Server raggiungibile tramite container Docker, installazione locale oppure, su Windows, SQL Server Express LocalDB. La semplice compilazione dei progetti non richiede SQL Server attivo.

Per i capitoli dal 13 al 17 la connection string viene letta dalla variabile d'ambiente `TICKETING_CONNECTION_STRING`. Ad esempio, in PowerShell:

```powershell
$env:TICKETING_CONNECTION_STRING = `
    "Server=localhost,1433;Database=Ticketing;User Id=sa;" + `
    "Password=<YOUR_PASSWORD>;Encrypt=True;TrustServerCertificate=True"
```

Dal capitolo 15 gli esempi con Entity Framework Core utilizzano il database `TicketingEfCore`, creato tramite le migrazioni.

Dal capitolo 18 i progetti Web leggono invece la chiave `ConnectionStrings:Ticketing`. In `appsettings.json` la chiave è presente con un valore vuoto: il valore reale deve essere impostato nei User Secrets del progetto Web.

Ad esempio:

```text
dotnet user-secrets set "ConnectionStrings:Ticketing" `
    "<TICKETING_CONNECTION_STRING>" `
    --project chapter-18-config-logging/src/Ticketing.Web/Ticketing.Web.csproj
```

In alternativa è possibile utilizzare la variabile d'ambiente `ConnectionStrings__Ticketing`.

Dal capitolo 17 i sample web usano Entity Framework Core come percorso predefinito. Dal capitolo 18 la configurazione `Persistence:Implementation` permette di scegliere tra `AdoNet`, `Dapper` ed `EfCore`, mantenendo disponibili le tre implementazioni sviluppate nei capitoli precedenti.

I progetti di integration test sostituiscono invece il repository con una fake in memoria, così da verificare la pipeline HTTP senza dipendere da SQL Server.

## Autore

Sono **Andrea Dottor**, sviluppatore e consulente .NET, Microsoft MVP Developer Technologies, docente ITS e speaker.

Mi occupo principalmente di .NET, ASP.NET Core e Blazor e pubblico settimanalmente il podcast **.NET in pillole**.

- Sito: https://dottor.net/
- Podcast: https://dottor.net/podcast/
