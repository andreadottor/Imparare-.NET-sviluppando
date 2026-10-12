const int MinimumPriority = 1;
const int MaximumPriority = 3;
const int MinimumCategory = 1;
const int MaximumCategory = 3;

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

    int category = 0;
    bool isValidCategory = false;

    while (!isValidCategory)
    {
        Console.WriteLine(
            $"Inserisci la categoria da {MinimumCategory} a {MaximumCategory}:");
        string categoryInput = Console.ReadLine() ?? "";

        if (!int.TryParse(categoryInput, out category))
        {
            Console.WriteLine("La categoria deve essere un numero intero.");
        }
        else if (category < MinimumCategory || category > MaximumCategory)
        {
            Console.WriteLine("La categoria indicata non è valida.");
        }
        else
        {
            isValidCategory = true;
        }
    }

    int estimatedHours = 0;
    bool isValidEstimate = false;

    while (!isValidEstimate)
    {
        Console.WriteLine("Inserisci le ore stimate:");
        string estimatedHoursInput = Console.ReadLine() ?? "";

        if (!int.TryParse(estimatedHoursInput, out estimatedHours))
        {
            Console.WriteLine("Le ore stimate devono essere un numero intero.");
        }
        else if (estimatedHours <= 0)
        {
            Console.WriteLine("Le ore stimate devono essere maggiori di zero.");
        }
        else
        {
            isValidEstimate = true;
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

    string categoryLabel;

    switch (category)
    {
        case 1:
            categoryLabel = "Hardware";
            break;
        case 2:
            categoryLabel = "Software";
            break;
        case 3:
            categoryLabel = "Accesso";
            break;
        default:
            categoryLabel = "Sconosciuta";
            break;
    }

    bool requiresEvaluation = isUrgent || estimatedHours > 8;

    Console.WriteLine();
    Console.WriteLine("Ticket inserito:");
    Console.WriteLine($"Titolo: {title}");
    Console.WriteLine($"Descrizione: {description}");
    Console.WriteLine($"Priorità: {priorityLabel}");
    Console.WriteLine($"Categoria: {categoryLabel}");
    Console.WriteLine($"Ore stimate: {estimatedHours}");
    Console.WriteLine($"Creato il: {createdAt:dd/MM/yyyy HH:mm:ss zzz}");

    if (isUrgent)
    {
        Console.WriteLine("ATTENZIONE: il ticket richiede una gestione urgente.");
    }

    if (requiresEvaluation)
    {
        Console.WriteLine("Il ticket richiede una valutazione.");
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
