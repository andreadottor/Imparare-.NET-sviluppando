const int MinimumPriority = 1;
const int MaximumPriority = 3;

bool createAnotherTicket;

do
{
    Console.WriteLine("=== Nuovo ticket ===");

    string title;

    do
    {
        Console.WriteLine("Inserisci il titolo:");
        title = Console.ReadLine() ?? "";

        if (string.IsNullOrWhiteSpace(title))
        {
            Console.WriteLine("Il titolo non può essere vuoto.");
        }
    }
    while (string.IsNullOrWhiteSpace(title));

    Console.WriteLine("Inserisci la descrizione:");
    string description = Console.ReadLine() ?? "";

    int priority = 0;
    bool isValidPriority = false;

    while (!isValidPriority)
    {
        Console.WriteLine(
            $"Inserisci la priorità da {MinimumPriority} a {MaximumPriority}:");
        string priorityInput = Console.ReadLine() ?? "";

        if (!int.TryParse(priorityInput, out priority))
        {
            Console.WriteLine("La priorità deve essere un numero intero.");
        }
        else if (priority < MinimumPriority || priority > MaximumPriority)
        {
            Console.WriteLine("La priorità indicata non è valida.");
        }
        else
        {
            isValidPriority = true;
        }
    }

    bool isUrgent = priority == 1;
    DateTimeOffset createdAt = DateTimeOffset.UtcNow;

    string priorityLabel;

    switch (priority)
    {
        case 1:
            priorityLabel = "Alta";
            break;
        case 2:
            priorityLabel = "Media";
            break;
        case 3:
            priorityLabel = "Bassa";
            break;
        default:
            priorityLabel = "Sconosciuta";
            break;
    }

    Console.WriteLine();
    Console.WriteLine("Ticket inserito:");
    Console.WriteLine($"Titolo: {title}");
    Console.WriteLine($"Descrizione: {description}");
    Console.WriteLine($"Priorità: {priorityLabel}");
    Console.WriteLine($"Creato il: {createdAt:dd/MM/yyyy HH:mm:ss zzz}");

    if (isUrgent)
    {
        Console.WriteLine("ATTENZIONE: il ticket richiede una gestione urgente.");
    }

    Console.WriteLine();
    Console.WriteLine("Vuoi inserire un altro ticket? S/N");
    string answer = Console.ReadLine() ?? "";
    createAnotherTicket = string.Equals(
        answer.Trim(),
        "S",
        StringComparison.OrdinalIgnoreCase);

    Console.WriteLine();
}
while (createAnotherTicket);

Console.WriteLine("Applicazione terminata.");