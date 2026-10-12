# Capitolo 6 — Esercizi su classi, oggetti e incapsulamento

Dieci esercizi per passare dalle variabili agli oggetti. Le prime attività introducono un concetto alla volta; le ultime riprendono la differenza tra riferimenti e valori. Prova a risolvere ogni consegna prima di aprire la soluzione, chiusa per impostazione predefinita nei visualizzatori che supportano `<details>`.

## Come lavorare

Crea un progetto console separato con `dotnet new console -n Chapter06Practice` e avvialo con `dotnet run --project Chapter06Practice`, usando l'SDK .NET indicato nel repository. Ogni soluzione è indipendente: sostituisci **tutto** il contenuto di `Program.cs` per provarla. Le istruzioni eseguibili vanno prima delle dichiarazioni di classe, struct o record.

Le classi qui sono versioni ridotte per esercitarsi: non devi aggiungerle ai sample in `src` o `exercise`. Dove una consegna impone dati validi, rispettali; la gestione completa delle eccezioni sarà approfondita più avanti nel libro.

## 1. Due operatori distinti

**Obiettivo:** dichiarare una classe e creare istanze con `new`.

Crea una classe `Operator` con le proprietà pubbliche `Name` e `Team`, entrambe stringhe modificabili. Crea Anna del team Supporto e Luca del team Infrastruttura. Stampa i dati di entrambi.

**Verifica:** ciascun oggetto conserva il proprio nome e il proprio team.

<details>
<summary>Mostra la soluzione</summary>

```csharp
var first = new Operator { Name = "Anna", Team = "Supporto" };
var second = new Operator { Name = "Luca", Team = "Infrastruttura" };
Console.WriteLine($"{first.Name}: {first.Team}");
Console.WriteLine($"{second.Name}: {second.Team}");

class Operator
{
    public string Name { get; set; } = "";
    public string Team { get; set; } = "";
}
```

Ogni `new` crea un oggetto distinto. Le stringhe vuote danno alle proprietà un valore iniziale non nullo.

</details>

## 2. Un titolo dal costruttore

**Obiettivo:** costruttore e proprietà con solo `get`.

Crea `Ticket` con una proprietà `Title` leggibile dall'esterno. Il costruttore riceve un titolo, lo ripulisce con `Trim()` e rifiuta titoli vuoti o composti da spazi con `ArgumentException`, come nel capitolo. Crea e stampa un ticket valido.

**Verifica:** `"  Stampante bloccata  "` diventa `"Stampante bloccata"`. Provando separatamente `"   "`, la costruzione deve generare un'eccezione e terminare il programma: qui non è richiesto intercettarla.

<details>
<summary>Mostra la soluzione</summary>

```csharp
var ticket = new Ticket("  Stampante bloccata  ");
Console.WriteLine(ticket.Title);

class Ticket
{
    public string Title { get; }

    public Ticket(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Il titolo è obbligatorio.", nameof(title));
        }

        Title = title.Trim();
    }
}
```

Il costruttore stabilisce lo stato iniziale. Dopo la creazione il chiamante non può assegnare un nuovo titolo.

</details>

## 3. Chiudere e riaprire

**Obiettivo:** metodi di istanza e `private set`.

Crea `Ticket` con `IsClosed`, leggibile da tutti ma modificabile solo dalla classe. Aggiungi `Close()` e `Reopen()`. Un ticket nuovo è aperto. Stampa lo stato iniziale, quello dopo la chiusura e quello dopo la riapertura.

**Verifica:** la sequenza è `false`, `true`, `false`. Chiamare `Close()` due volte deve lasciare il ticket chiuso.

<details>
<summary>Mostra la soluzione</summary>

```csharp
var ticket = new Ticket();
Console.WriteLine(ticket.IsClosed);
ticket.Close();
ticket.Close();
Console.WriteLine(ticket.IsClosed);
ticket.Reopen();
Console.WriteLine(ticket.IsClosed);

class Ticket
{
    public bool IsClosed { get; private set; }

    public void Close()
    {
        IsClosed = true;
    }

    public void Reopen()
    {
        IsClosed = false;
    }
}
```

Il codice esterno chiede un'operazione all'oggetto anziché modificare direttamente lo stato.

</details>

## 4. Cambiare priorità senza rovinare lo stato

**Obiettivo:** validazione in un metodo e proprietà calcolata.

Crea un ticket con priorità iniziale 3. `ChangePriority(int newPriority)` accetta soltanto valori da 1 a 3 e restituisce l'esito come `bool`. Un valore non valido lascia invariata la priorità. Aggiungi `IsUrgent`, calcolata come `Priority == 1`.

**Verifica:** dopo aver assegnato 1, il ticket è urgente; tentando 0 e 4, l'esito è `false` e la priorità resta 1. Con 2 non è più urgente.

<details>
<summary>Mostra la soluzione</summary>

```csharp
var ticket = new Ticket();
Console.WriteLine(ticket.ChangePriority(1));
Console.WriteLine($"{ticket.Priority}, urgente: {ticket.IsUrgent}");
Console.WriteLine(ticket.ChangePriority(0));
Console.WriteLine(ticket.ChangePriority(4));
Console.WriteLine($"{ticket.Priority}, urgente: {ticket.IsUrgent}");
ticket.ChangePriority(2);
Console.WriteLine($"{ticket.Priority}, urgente: {ticket.IsUrgent}");

class Ticket
{
    public int Priority { get; private set; } = 3;
    public bool IsUrgent => Priority == 1;

    public bool ChangePriority(int newPriority)
    {
        if (newPriority < 1 || newPriority > 3)
        {
            return false;
        }

        Priority = newPriority;
        return true;
    }
}
```

`IsUrgent` viene ricalcolata quando la leggi: non occorre mantenerla sincronizzata a mano.

</details>

## 5. Assegnare un operatore

**Obiettivo:** stato opzionale e stringa nullable.

Crea un ticket con `AssignedOperator` di tipo `string?`, inizialmente `null`, e con `private set`. `AssignTo(string operatorName)` rifiuta nomi vuoti o di soli spazi restituendo `false`; altrimenti memorizza il nome ripulito e restituisce `true`. Una richiesta non valida deve conservare l'assegnazione precedente.

**Verifica:** inizialmente stampa `Non assegnato`; assegnando `"  Anna  "` ottieni `Anna`; assegnando `"   "` ottieni `false` e il nome rimane `Anna`.

<details>
<summary>Mostra la soluzione</summary>

```csharp
var ticket = new Ticket();
Console.WriteLine(ticket.AssignedOperator ?? "Non assegnato");
Console.WriteLine(ticket.AssignTo("  Anna  "));
Console.WriteLine(ticket.AssignedOperator);
Console.WriteLine(ticket.AssignTo("   "));
Console.WriteLine(ticket.AssignedOperator);

class Ticket
{
    public string? AssignedOperator { get; private set; }

    public bool AssignTo(string operatorName)
    {
        if (string.IsNullOrWhiteSpace(operatorName))
        {
            return false;
        }

        AssignedOperator = operatorName.Trim();
        return true;
    }
}
```

`null` rappresenta l'assenza di un'assegnazione; `??` sceglie un testo da mostrare in quel caso.

</details>

## 6. Un contatore con un campo privato

**Obiettivo:** distinguere campo, proprietà e metodo.

Crea `WorkLog` con un campo privato `_totalHours`, una proprietà di sola lettura `TotalHours` e un metodo `AddHours(int hours)`. Accetta valori da 1 a 24, sommali al totale e restituisci `true`. Per gli altri valori restituisci `false` senza cambiare il totale.

**Verifica:** aggiungendo 2 e 3 arrivi a 5; aggiungendo 0, -1 o 25 il totale rimane 5.

<details>
<summary>Mostra la soluzione</summary>

```csharp
var log = new WorkLog();
log.AddHours(2);
log.AddHours(3);
Console.WriteLine(log.TotalHours);
Console.WriteLine(log.AddHours(0));
Console.WriteLine(log.AddHours(-1));
Console.WriteLine(log.AddHours(25));
Console.WriteLine(log.TotalHours);

class WorkLog
{
    private int _totalHours;
    public int TotalHours => _totalHours;

    public bool AddHours(int hours)
    {
        if (hours < 1 || hours > 24)
        {
            return false;
        }

        _totalHours += hours;
        return true;
    }
}
```

Il campo conserva il dato; la proprietà lo espone; il metodo controlla come può cambiare.

</details>

## 7. Un numero diverso per ogni ticket

**Obiettivo:** membri `static` e membri di istanza.

Crea `Ticket` con una proprietà `Id` di sola lettura e un contatore privato `static`. Nel costruttore incrementa il contatore e assegna il nuovo valore all'ID. Crea tre ticket nello stesso processo.

**Verifica:** gli ID sono 1, 2 e 3. Il primo ticket conserva ID 1 anche dopo aver creato gli altri. Riavviando il programma, il contatore riparte da zero: non è un archivio persistente.

<details>
<summary>Mostra la soluzione</summary>

```csharp
var first = new Ticket();
var second = new Ticket();
var third = new Ticket();
Console.WriteLine(first.Id);
Console.WriteLine(second.Id);
Console.WriteLine(third.Id);

class Ticket
{
    private static int _lastId;
    public int Id { get; }

    public Ticket()
    {
        _lastId++;
        Id = _lastId;
    }
}
```

Il contatore appartiene al tipo, mentre ogni ID appartiene a un oggetto. Questo esempio è pensato per il semplice programma console sequenziale.

</details>

## 8. Due variabili, un solo oggetto

**Obiettivo:** capire l'assegnazione di un reference type.

Crea `Ticket` con `Title` modificabile. Assegna un ticket a `first`, poi scrivi `Ticket second = first`. Cambia il titolo tramite `second` e stampa entrambi i titoli. Infine assegna a `second` un nuovo ticket e stampa di nuovo.

**Verifica:** dopo la modifica entrambi mostrano `Titolo aggiornato`; dopo il nuovo `new`, `first` mantiene quel titolo e `second` mostra `Altro ticket`.

<details>
<summary>Mostra la soluzione</summary>

```csharp
var first = new Ticket { Title = "Titolo iniziale" };
Ticket second = first;
second.Title = "Titolo aggiornato";
Console.WriteLine(first.Title);
Console.WriteLine(second.Title);

second = new Ticket { Title = "Altro ticket" };
Console.WriteLine(first.Title);
Console.WriteLine(second.Title);

class Ticket
{
    public string Title { get; set; } = "";
}
```

L'assegnazione copia il riferimento. Soltanto il secondo `new` crea un altro oggetto. Qui il setter pubblico serve a osservare la semantica dei riferimenti.

</details>

## 9. Una stima come valore

**Obiettivo:** `readonly struct` e copia di un valore.

Crea `WorkEstimate` come `readonly struct`, con `Hours` di sola lettura, un costruttore e la proprietà calcolata `IsLong` (`Hours > 8`). Per questo esercizio usa ore positive. Crea una stima di 4 ore, copiala in un'altra variabile, poi assegna alla seconda una nuova stima di 10 ore.

**Verifica:** la prima stima resta di 4 ore e non è lunga; la seconda è di 10 ore ed è lunga.

<details>
<summary>Mostra la soluzione</summary>

```csharp
var first = new WorkEstimate(4);
WorkEstimate second = first;
second = new WorkEstimate(10);
Console.WriteLine($"Prima: {first.Hours}, lunga: {first.IsLong}");
Console.WriteLine($"Seconda: {second.Hours}, lunga: {second.IsLong}");

readonly struct WorkEstimate
{
    public int Hours { get; }
    public bool IsLong => Hours > 8;

    public WorkEstimate(int hours)
    {
        Hours = hours;
    }
}
```

Una struct viene copiata per valore. Essendo immutabile, per rappresentare una stima diversa creiamo un nuovo valore. Anche questa struct ammette il valore predefinito con 0 ore: il solo costruttore non lo impedisce.

</details>

## 10. Una fotografia del ticket

**Obiettivo:** record e snapshot indipendente dallo stato successivo.

Crea `Ticket` con titolo ricevuto dal costruttore, `IsClosed` con `private set`, `Close()` e `CreateSnapshot()`. Quest'ultimo restituisce un `TicketSnapshot`, record con titolo e stato di chiusura. Usa un titolo non vuoto. Crea uno snapshot prima di chiudere il ticket e un altro dopo.

**Verifica:** dopo la chiusura il ticket è chiuso, il primo snapshot riporta ancora `false` e il secondo `true`.

<details>
<summary>Mostra la soluzione</summary>

```csharp
var ticket = new Ticket("Stampante bloccata");
TicketSnapshot before = ticket.CreateSnapshot();
ticket.Close();
TicketSnapshot after = ticket.CreateSnapshot();
Console.WriteLine($"Ticket: {ticket.IsClosed}");
Console.WriteLine($"Prima: {before.IsClosed}");
Console.WriteLine($"Dopo: {after.IsClosed}");

class Ticket
{
    public string Title { get; }
    public bool IsClosed { get; private set; }

    public Ticket(string title)
    {
        Title = title;
    }

    public void Close()
    {
        IsClosed = true;
    }

    public TicketSnapshot CreateSnapshot()
    {
        return new TicketSnapshot(Title, IsClosed);
    }
}

record TicketSnapshot(string Title, bool IsClosed);
```

Lo snapshot cattura i dati al momento della creazione. Qui contiene una stringa immutabile e un booleano, quindi le successive modifiche al ticket non lo alterano.

</details>
