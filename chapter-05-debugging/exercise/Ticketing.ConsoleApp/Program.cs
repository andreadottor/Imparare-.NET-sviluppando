class Program
{
    static void Main()
    {
        bool createAnotherTicket;

        do
        {
            Console.WriteLine("=== Nuovo ticket ===");

            string title = ReadRequiredText("Inserisci il titolo:");

            Console.WriteLine("Inserisci la descrizione:");
            string description = Console.ReadLine() ?? "";

            int priority = ReadIntInRange(
                "Inserisci la priorità da 1 a 3:",
                minimumValue: 1,
                maximumValue: 3);

            int category = ReadIntInRange(
                "Inserisci la categoria (1 Hardware, 2 Software, 3 Accesso):",
                minimumValue: 1,
                maximumValue: 3);

            int estimatedHours = ReadPositiveInt(
                "Inserisci le ore stimate per la risoluzione:");

            bool isUrgent = priority == 1;
            DateTimeOffset createdAt = DateTimeOffset.UtcNow;
            string priorityLabel = GetPriorityLabel(priority);
            string categoryLabel = GetCategoryLabel(category);
            bool requiresEvaluation = RequiresEvaluation(priority, estimatedHours);

            PrintTicketSummary(
                title,
                description,
                priorityLabel,
                categoryLabel,
                estimatedHours,
                createdAt,
                isUrgent,
                requiresEvaluation);

            createAnotherTicket = ReadConfirmation(
                "Vuoi inserire un altro ticket? S/N");

            Console.WriteLine();
        }
        while (createAnotherTicket);

        Console.WriteLine("Applicazione terminata.");
    }

    /// <summary>
    /// Legge dalla console un testo obbligatorio.
    /// Continua a chiedere il valore finché l'utente non inserisce almeno
    /// un carattere significativo.
    /// </summary>
    /// <param name="prompt">Messaggio mostrato prima della lettura.</param>
    /// <returns>Il testo inserito senza spazi iniziali e finali.</returns>
    static string ReadRequiredText(string prompt)
    {
        string value;

        do
        {
            Console.WriteLine(prompt);
            value = Console.ReadLine() ?? "";

            if (string.IsNullOrWhiteSpace(value))
            {
                Console.WriteLine("Il valore non può essere vuoto.");
            }
        }
        while (string.IsNullOrWhiteSpace(value));

        return value.Trim();
    }

    /// <summary>
    /// Legge un numero intero compreso nell'intervallo specificato.
    /// Continua a chiedere il valore finché l'utente non inserisce un input valido.
    /// </summary>
    /// <param name="prompt">Messaggio mostrato prima della lettura.</param>
    /// <param name="minimumValue">Valore minimo accettato, incluso.</param>
    /// <param name="maximumValue">Valore massimo accettato, incluso.</param>
    /// <returns>Il numero intero valido inserito dall'utente.</returns>
    static int ReadIntInRange(string prompt, int minimumValue, int maximumValue)
    {
        while (true)
        {
            Console.WriteLine(prompt);
            string input = Console.ReadLine() ?? "";

            if (!int.TryParse(input, out int value))
            {
                Console.WriteLine("Il valore deve essere un numero intero.");
                continue;
            }

            if (value < minimumValue || value > maximumValue)
            {
                Console.WriteLine(
                    "Il valore deve essere compreso tra " +
                    $"{minimumValue} e {maximumValue}.");
                continue;
            }

            return value;
        }
    }

    /// <summary>
    /// Restituisce l'etichetta associata a una priorità numerica.
    /// </summary>
    /// <param name="priority">Priorità numerica del ticket.</param>
    /// <returns>
    /// L'etichetta della priorità oppure "Sconosciuta" se il valore non è gestito.
    /// </returns>
    static string GetPriorityLabel(int priority)
    {
        switch (priority)
        {
            case 1:
                return "Alta";
            case 2:
                return "Media";
            case 3:
                return "Bassa";
            default:
                return "Sconosciuta";
        }
    }

    /// <summary>
    /// Restituisce l'etichetta associata a una categoria numerica.
    /// </summary>
    /// <param name="category">Categoria numerica del ticket.</param>
    /// <returns>
    /// L'etichetta della categoria oppure "Sconosciuta" se il valore non è gestito.
    /// </returns>
    static string GetCategoryLabel(int category)
    {
        switch (category)
        {
            case 1:
                return "Hardware";
            case 2:
                return "Software";
            case 3:
                return "Accesso";
            default:
                return "Sconosciuta";
        }
    }

    /// <summary>
    /// Legge un numero intero positivo dalla console.
    /// Continua a chiedere il valore finché l'utente non inserisce
    /// un numero maggiore di zero.
    /// </summary>
    /// <param name="prompt">Messaggio mostrato prima della lettura.</param>
    /// <returns>Il numero intero positivo inserito dall'utente.</returns>
    static int ReadPositiveInt(string prompt)
    {
        while (true)
        {
            Console.WriteLine(prompt);
            string input = Console.ReadLine() ?? "";

            if (!int.TryParse(input, out int value))
            {
                Console.WriteLine("Il valore deve essere un numero intero.");
                continue;
            }

            if (value <= 0)
            {
                Console.WriteLine("Il valore deve essere maggiore di zero.");
                continue;
            }

            return value;
        }
    }

    /// <summary>
    /// Determina se il ticket deve essere valutato da un operatore.
    /// </summary>
    /// <param name="priority">Priorità numerica del ticket.</param>
    /// <param name="estimatedHours">Ore stimate per la risoluzione.</param>
    /// <returns>
    /// <see langword="true"/> se il ticket è urgente o richiede più di otto ore;
    /// altrimenti <see langword="false"/>.
    /// </returns>
    static bool RequiresEvaluation(int priority, int estimatedHours)
    {
        return priority == 1 || estimatedHours > 8;
    }

    /// <summary>
    /// Mostra il riepilogo del ticket inserito.
    /// </summary>
    /// <param name="title">Titolo del ticket.</param>
    /// <param name="description">Descrizione del problema.</param>
    /// <param name="priorityLabel">Etichetta leggibile della priorità.</param>
    /// <param name="categoryLabel">Etichetta leggibile della categoria.</param>
    /// <param name="estimatedHours">Ore stimate per la risoluzione.</param>
    /// <param name="createdAt">Momento di creazione del ticket.</param>
    /// <param name="isUrgent">
    /// Indica se il ticket richiede una gestione urgente.
    /// </param>
    /// <param name="requiresEvaluation">
    /// Indica se il ticket deve essere valutato da un operatore.
    /// </param>
    static void PrintTicketSummary(
        string title,
        string description,
        string priorityLabel,
        string categoryLabel,
        int estimatedHours,
        DateTimeOffset createdAt,
        bool isUrgent,
        bool requiresEvaluation)
    {
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
            Console.WriteLine(
                "ATTENZIONE: il ticket richiede una gestione urgente.");
        }

        if (requiresEvaluation)
        {
            Console.WriteLine(
                "È necessaria una valutazione da parte di un operatore.");
        }
    }

    /// <summary>
    /// Legge una conferma dalla console.
    /// </summary>
    /// <param name="prompt">Domanda mostrata all'utente.</param>
    /// <returns>
    /// <see langword="true"/> se la risposta è S, ignorando maiuscole,
    /// minuscole e spazi; altrimenti <see langword="false"/>.
    /// </returns>
    static bool ReadConfirmation(string prompt)
    {
        Console.WriteLine();
        Console.WriteLine(prompt);

        string answer = Console.ReadLine() ?? "";

        return string.Equals(
            answer.Trim(),
            "S",
            StringComparison.OrdinalIgnoreCase);
    }
}