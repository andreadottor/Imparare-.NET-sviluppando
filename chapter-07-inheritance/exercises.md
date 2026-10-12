# Capitolo 7 — Esercizi su ereditarietà, polimorfismo e interfacce

Dieci esercizi introduttivi, dal riuso di una proprietà fino alla scelta di un comportamento tramite interfaccia. Ogni attività resta piccola e introduce pochi elementi nuovi. Prova prima da solo, poi apri la soluzione: le sezioni `<details>` sono chiuse per impostazione predefinita nei visualizzatori compatibili.

## Come lavorare

Crea un progetto console separato con `dotnet new console -n Chapter07Practice` e avvialo con `dotnet run --project Chapter07Practice`, usando l'SDK .NET indicato nel repository. Ogni soluzione è completa e indipendente: sostituisci **tutto** il contenuto di `Program.cs` per provarla. Le istruzioni eseguibili precedono le dichiarazioni dei tipi.

I tipi `Ticket` di questi esercizi sono versioni ridotte e non vanno aggiunti ai sample in `src` o `exercise`. Usa stringhe non vuote nei costruttori: qui l'obiettivo è capire le relazioni tra tipi, riprendendo la validazione studiata nel capitolo 6 solo quando richiesto. Non servono generics o LINQ.

## 1. Una classe più specifica

**Obiettivo:** dichiarare una classe derivata.

Crea `Ticket` con `Title` modificabile e `IncidentTicket : Ticket` con `AffectedService` modificabile. Crea un *incident* con titolo `Portale non disponibile` e servizio `Customer Portal`, poi stampa entrambe le proprietà.

**Verifica:** sull'oggetto `IncidentTicket` puoi leggere sia la proprietà ereditata sia quella specifica.

<details>
<summary>Mostra la soluzione</summary>

```csharp
var incident = new IncidentTicket
{
    Title = "Portale non disponibile",
    AffectedService = "Customer Portal"
};
Console.WriteLine(incident.Title);
Console.WriteLine(incident.AffectedService);

class Ticket
{
    public string Title { get; set; } = "";
}

class IncidentTicket : Ticket
{
    public string AffectedService { get; set; } = "";
}
```

La classe derivata aggiunge informazioni senza dover dichiarare nuovamente il titolo.

</details>

## 2. Costruire anche la parte comune

**Obiettivo:** costruttori e chiamata `base(...)`.

Crea `Ticket` con `Title` di sola lettura, inizializzato dal costruttore. Crea `IncidentTicket` con `AffectedService` di sola lettura. Il suo costruttore riceve titolo e servizio e passa il titolo al costruttore della base.

**Verifica:** creando un incidente, entrambe le proprietà risultano inizializzate. Non duplicare l'assegnazione del titolo nella classe derivata.

<details>
<summary>Mostra la soluzione</summary>

```csharp
var incident = new IncidentTicket("Portale bloccato", "Customer Portal");
Console.WriteLine($"{incident.Title}: {incident.AffectedService}");

class Ticket
{
    public string Title { get; }

    public Ticket(string title)
    {
        Title = title;
    }
}

class IncidentTicket : Ticket
{
    public string AffectedService { get; }

    public IncidentTicket(string title, string affectedService) : base(title)
    {
        AffectedService = affectedService;
    }
}
```

I costruttori non vengono ereditati. `base(...)` inizializza la parte comune dell'oggetto.

</details>

## 3. Proteggere lo stato della base

**Obiettivo:** membri privati e operazioni ereditate.

Crea `Ticket` con `IsClosed` con `private set` e un metodo pubblico `Close()`. In `IncidentTicket` aggiungi `Resolve()`, che chiama `Close()`. Non assegnare direttamente `IsClosed` dalla derivata.

**Verifica:** prima di `Resolve()` lo stato è `false`, dopo è `true`. Un secondo `Resolve()` lascia il ticket chiuso.

<details>
<summary>Mostra la soluzione</summary>

```csharp
var incident = new IncidentTicket();
Console.WriteLine(incident.IsClosed);
incident.Resolve();
incident.Resolve();
Console.WriteLine(incident.IsClosed);

class Ticket
{
    public bool IsClosed { get; private set; }

    public void Close()
    {
        IsClosed = true;
    }
}

class IncidentTicket : Ticket
{
    public void Resolve()
    {
        Close();
    }
}
```

La derivata usa l'operazione pubblica della base. Il setter privato rimane accessibile soltanto dentro `Ticket`.

</details>

## 4. Un riepilogo specializzato

**Obiettivo:** `virtual`, `override` e riuso con `base`.

Crea `Ticket` con un metodo virtuale `GetSummary()` che restituisce `Ticket: <titolo>`. In `IncidentTicket` aggiungi il servizio e ridefinisci il metodo aggiungendo ` - servizio: <servizio>` al riepilogo della base. Chiama il metodo attraverso una variabile di tipo `Ticket` che contiene un incidente.

**Verifica:** il risultato include anche il servizio, benché la variabile sia dichiarata come `Ticket`.

<details>
<summary>Mostra la soluzione</summary>

```csharp
Ticket ticket = new IncidentTicket("Portale bloccato", "Customer Portal");
Console.WriteLine(ticket.GetSummary());

class Ticket
{
    public string Title { get; }

    public Ticket(string title)
    {
        Title = title;
    }

    public virtual string GetSummary()
    {
        return $"Ticket: {Title}";
    }
}

class IncidentTicket : Ticket
{
    public string AffectedService { get; }

    public IncidentTicket(string title, string affectedService) : base(title)
    {
        AffectedService = affectedService;
    }

    public override string GetSummary()
    {
        return $"{base.GetSummary()} - servizio: {AffectedService}";
    }
}
```

Per un metodo virtuale conta il tipo effettivo dell'oggetto. `base.GetSummary()` evita di duplicare la parte comune.

</details>

## 5. Un comportamento obbligatorio

**Obiettivo:** classe astratta e proprietà astratta.

Crea `Ticket` astratta con `Title` inizializzato da un costruttore `protected` e una proprietà astratta `TargetResolutionHours`. Crea le classi concrete `IncidentTicket` e `ServiceRequestTicket`, che restituiscono rispettivamente 2 e 24 ore. Stampa titolo e ore di entrambe tramite variabili di tipo `Ticket`.

**Verifica:** ogni classe concreta fornisce il proprio valore. `Ticket` rappresenta la base comune e non può essere istanziata direttamente.

<details>
<summary>Mostra la soluzione</summary>

```csharp
Ticket incident = new IncidentTicket("Portale bloccato");
Ticket request = new ServiceRequestTicket("Nuovo account");
Console.WriteLine($"{incident.Title}: {incident.TargetResolutionHours} ore");
Console.WriteLine($"{request.Title}: {request.TargetResolutionHours} ore");

abstract class Ticket
{
    public string Title { get; }
    public abstract int TargetResolutionHours { get; }

    protected Ticket(string title)
    {
        Title = title;
    }
}

class IncidentTicket : Ticket
{
    public override int TargetResolutionHours => 2;

    public IncidentTicket(string title) : base(title) { }
}

class ServiceRequestTicket : Ticket
{
    public override int TargetResolutionHours => 24;

    public ServiceRequestTicket(string title) : base(title) { }
}
```

La base definisce ciò che deve essere disponibile; le classi concrete completano il comportamento.

</details>

## 6. Tipi diversi nello stesso array

**Obiettivo:** upcast e polimorfismo in un ciclo.

Crea una base astratta `Ticket` con `GetSummary()` astratto. Le classi `IncidentTicket` e `ServiceRequestTicket` restituiscono rispettivamente `Incidente` e `Richiesta di servizio`. Inserisci due incidenti e una richiesta in un array `Ticket[]` e stampa i riepiloghi con `foreach`, senza controllare il tipo nel ciclo.

**Verifica:** i tre riepiloghi seguono l'ordine dell'array, usando l'implementazione di ogni oggetto.

<details>
<summary>Mostra la soluzione</summary>

```csharp
Ticket[] tickets =
{
    new IncidentTicket(),
    new ServiceRequestTicket(),
    new IncidentTicket()
};

foreach (Ticket ticket in tickets)
{
    Console.WriteLine(ticket.GetSummary());
}

abstract class Ticket
{
    public abstract string GetSummary();
}

class IncidentTicket : Ticket
{
    public override string GetSummary()
    {
        return "Incidente";
    }
}

class ServiceRequestTicket : Ticket
{
    public override string GetSummary()
    {
        return "Richiesta di servizio";
    }
}
```

L'array conserva riferimenti di tipo base. Ogni oggetto mantiene il proprio tipo concreto.

</details>

## 7. Leggere un dato specifico in sicurezza

**Obiettivo:** pattern matching con `is`.

Crea `Ticket` con `Title` modificabile e una derivata `IncidentTicket` con `AffectedService`. In un array di ticket inserisci un incidente e un ticket comune. Stampa tutti i titoli e, soltanto per gli incidenti, il servizio coinvolto. Evita cast diretti non controllati.

**Verifica:** entrambi i titoli compaiono, ma il servizio viene stampato solo per l'incidente.

<details>
<summary>Mostra la soluzione</summary>

```csharp
Ticket[] tickets =
{
    new IncidentTicket { Title = "Portale bloccato", AffectedService = "Portale" },
    new Ticket { Title = "Informazioni sul servizio" }
};

foreach (Ticket ticket in tickets)
{
    Console.WriteLine(ticket.Title);
    if (ticket is IncidentTicket incident)
    {
        Console.WriteLine($"Servizio: {incident.AffectedService}");
    }
}

class Ticket
{
    public string Title { get; set; } = "";
}

class IncidentTicket : Ticket
{
    public string AffectedService { get; set; } = "";
}
```

Il pattern controlla il tipo e rende disponibile la variabile `incident` soltanto quando il controllo riesce. Serve quando devi leggere un dato specifico; per un comportamento comune preferisci un metodo polimorfico.

</details>

## 8. Un contratto per i riepiloghi

**Obiettivo:** dichiarare e implementare un'interfaccia.

Crea `ISummarizable` con `string GetSummary()`. Implementala in `Ticket`, che ha un titolo ricevuto dal costruttore. Assegna il ticket a una variabile di tipo `ISummarizable` e usa il metodo tramite quel contratto.

**Verifica:** ottieni `Ticket: Stampante bloccata` tramite la variabile di tipo interfaccia.

<details>
<summary>Mostra la soluzione</summary>

```csharp
ISummarizable item = new Ticket("Stampante bloccata");
Console.WriteLine(item.GetSummary());

interface ISummarizable
{
    string GetSummary();
}

class Ticket : ISummarizable
{
    public string Title { get; }

    public Ticket(string title)
    {
        Title = title;
    }

    public string GetSummary()
    {
        return $"Ticket: {Title}";
    }
}
```

L'interfaccia descrive un'operazione disponibile senza imporre una classe base.

</details>

## 9. Due formatter intercambiabili

**Obiettivo:** usare un'interfaccia come parametro e applicare la composizione.

Crea `Ticket` con titolo e priorità ricevuti dal costruttore. Definisci `ITicketFormatter` con `Format(Ticket ticket)`. `CompactTicketFormatter` restituisce solo il titolo; `DetailedTicketFormatter` aggiunge la priorità. Scrivi `PrintTicket(Ticket ticket, ITicketFormatter formatter)` e stampa lo stesso ticket nei due formati.

**Verifica:** il formato compatto mostra `Stampante bloccata`; quello dettagliato mostra `Stampante bloccata - priorità 2`. Non modificare il ticket per cambiare formato.

<details>
<summary>Mostra la soluzione</summary>

```csharp
var ticket = new Ticket("Stampante bloccata", 2);
PrintTicket(ticket, new CompactTicketFormatter());
PrintTicket(ticket, new DetailedTicketFormatter());

static void PrintTicket(Ticket ticket, ITicketFormatter formatter)
{
    Console.WriteLine(formatter.Format(ticket));
}

class Ticket
{
    public string Title { get; }
    public int Priority { get; }

    public Ticket(string title, int priority)
    {
        Title = title;
        Priority = priority;
    }
}

interface ITicketFormatter
{
    string Format(Ticket ticket);
}

sealed class CompactTicketFormatter : ITicketFormatter
{
    public string Format(Ticket ticket)
    {
        return ticket.Title;
    }
}

sealed class DetailedTicketFormatter : ITicketFormatter
{
    public string Format(Ticket ticket)
    {
        return $"{ticket.Title} - priorità {ticket.Priority}";
    }
}
```

Il metodo usa il formatter ricevuto. La formattazione è affidata a un altro oggetto; `sealed` impedisce di derivare ulteriori classi dai formatter.

</details>

## 10. Una nuova specializzazione con una scadenza

**Obiettivo:** combinare base astratta, override e logica comune.

Crea una base astratta `Ticket` con `CreatedAt`, ricevuto dal costruttore, una proprietà astratta `TargetResolutionHours` e `DueAt`, calcolata aggiungendo quelle ore alla creazione. Crea `IncidentTicket` con durata di 2 ore e `MaintenanceTicket` con durata di 48 ore; rendile `sealed`. Stampa le scadenze tramite un array di tipo base senza distinguere i tipi nel ciclo.

**Verifica:** con creazione il 1 giugno 2026 alle 09:00 UTC, l'incidente scade il 1 giugno alle 11:00 e la manutenzione il 3 giugno alle 09:00. Per semplificare, le durate sono fisse e non dipendono dalla priorità.

<details>
<summary>Mostra la soluzione</summary>

```csharp
var createdAt = new DateTimeOffset(2026, 6, 1, 9, 0, 0, TimeSpan.Zero);
Ticket[] tickets =
{
    new IncidentTicket(createdAt),
    new MaintenanceTicket(createdAt)
};

foreach (Ticket ticket in tickets)
{
    Console.WriteLine(
        $"{ticket.TargetResolutionHours} ore: {ticket.DueAt:dd/MM/yyyy HH:mm zzz}");
}

abstract class Ticket
{
    public DateTimeOffset CreatedAt { get; }
    public abstract int TargetResolutionHours { get; }
    public DateTimeOffset DueAt => CreatedAt.AddHours(TargetResolutionHours);

    protected Ticket(DateTimeOffset createdAt)
    {
        CreatedAt = createdAt;
    }
}

sealed class IncidentTicket : Ticket
{
    public override int TargetResolutionHours => 2;

    public IncidentTicket(DateTimeOffset createdAt) : base(createdAt) { }
}

sealed class MaintenanceTicket : Ticket
{
    public override int TargetResolutionHours => 48;

    public MaintenanceTicket(DateTimeOffset createdAt) : base(createdAt) { }
}
```

Il calcolo della scadenza resta nella base. Per aggiungere una specializzazione basta fornire la durata: il ciclo continua a funzionare.

</details>
