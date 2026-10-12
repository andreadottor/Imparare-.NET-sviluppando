# Capitolo 4 — Esercizi sui metodi

Venti esercizi per dare un nome alle operazioni, separare le responsabilità e riutilizzare il codice. Le situazioni spaziano da saluti e prezzi a temperature, parole e array: non occorre conoscere l'applicazione del libro. Procedi in ordine e prova la tua soluzione prima di aprire quella proposta. Ogni sezione `<details>` è chiusa per impostazione predefinita nei visualizzatori compatibili.

## Come lavorare

Crea un progetto console separato con `dotnet new console -n Chapter04Practice` e avvialo con `dotnet run --project Chapter04Practice`. Usa l'SDK .NET indicato nel repository. Per ogni esercizio sostituisci **tutto** `Program.cs`: le soluzioni sono indipendenti e complete. Non occorre modificare i sample in `src` o `exercise`.

Le soluzioni usano istruzioni di primo livello e funzioni locali `static`, come introdotto nel capitolo. Scrivi le chiamate prima delle dichiarazioni dei metodi. L'esercizio sugli overload usa invece una classe `Program` con `Main`, perché le funzioni locali non supportano gli overload. Per i metodi di calcolo usa dati fissi: così puoi controllare la logica senza dover leggere ogni volta dalla console.

Gli esercizi 1–10 introducono i diversi modi di chiamare un metodo e restituire risultati; gli esercizi 11–20 li applicano a piccoli problemi, fino a un programma con menu. Gli array e le stringhe passati ai metodi sono sempre non nulli, ma possono essere vuoti. Prima di scrivere un metodo, decidi quali dati riceve, cosa restituisce e se modifica i dati del chiamante.

## 1. Un saluto riutilizzabile

**Obiettivo:** metodo `void` e parametro.

Scrivi `PrintWelcome(string name)`, che stampa `Benvenuto, <nome>!`. Chiamalo per Anna e Luca. Il metodo deve stampare il messaggio, senza restituire un valore.

**Verifica:** compaiono due righe, una per ciascun nome.

<details>
<summary>Mostra la soluzione</summary>

```csharp
PrintWelcome("Anna");
PrintWelcome("Luca");

static void PrintWelcome(string name)
{
    Console.WriteLine($"Benvenuto, {name}!");
}
```

Il parametro permette di riutilizzare la stessa operazione con dati diversi.

</details>

## 2. Calcolare senza stampare

**Obiettivo:** valore di ritorno e separazione tra calcolo e output.

Scrivi `CalculateCost(int hours, decimal hourlyRate)`. Gli argomenti sono non negativi. Il metodo restituisce il prodotto, mentre il programma principale lo stampa con due decimali.

**Verifica:** (3, 25,50) → 76,50; (0, 25,50) → 0; (2, 0) → 0. La virgola è qui il separatore decimale italiano: nel codice scrivi `25.50m`.

<details>
<summary>Mostra la soluzione</summary>

```csharp
decimal total = CalculateCost(3, 25.50m);
Console.WriteLine($"Totale: {total:F2} euro");
Console.WriteLine(CalculateCost(0, 25.50m));
Console.WriteLine(CalculateCost(2, 0m));

static decimal CalculateCost(int hours, decimal hourlyRate)
{
    return hours * hourlyRate;
}
```

Restituire il risultato permette di usarlo anche in altri calcoli.

</details>

## 3. Una regola con un nome

**Obiettivo:** metodo che restituisce `bool`.

Scrivi `RequiresEvaluation(int priority, int estimatedHours)`: restituisce `true` se la priorità è 1 oppure le ore superano 8. In questo esercizio gli argomenti sono già validi: priorità da 1 a 3 e ore positive.

**Verifica:** (1, 2) → `true`; (2, 8) → `false`; (3, 9) → `true`.

<details>
<summary>Mostra la soluzione</summary>

```csharp
Console.WriteLine(RequiresEvaluation(1, 2));
Console.WriteLine(RequiresEvaluation(2, 8));
Console.WriteLine(RequiresEvaluation(3, 9));

static bool RequiresEvaluation(int priority, int estimatedHours)
{
    return priority == 1 || estimatedHours > 8;
}
```

Il nome descrive la decisione. Il chiamante non deve ripetere la condizione.

</details>

## 4. La somma diventa un metodo

**Obiettivo:** ciclo dentro un metodo e variabili locali.

Scrivi `SumUpTo(int limit)`, che somma gli interi da 1 al limite usando `for`. Considera limiti da 0 a 100; per 0 restituisci 0. Chiama il metodo due volte e verifica che la seconda somma non includa la prima.

**Verifica:** 5 → 15; 3 → 6; 0 → 0.

<details>
<summary>Mostra la soluzione</summary>

```csharp
Console.WriteLine(SumUpTo(5));
Console.WriteLine(SumUpTo(3));
Console.WriteLine(SumUpTo(0));

static int SumUpTo(int limit)
{
    int total = 0;
    for (int number = 1; number <= limit; number++)
    {
        total += number;
    }

    return total;
}
```

Ogni chiamata ha una nuova variabile `total`, inizializzata a zero.

</details>

## 5. Leggere un valore nell'intervallo

**Obiettivo:** riutilizzare un metodo di lettura e validazione.

Scrivi `ReadIntInRange(string prompt, int minimum, int maximum)`. Continua a leggere finché ricevi un intero nell'intervallo inclusivo. Usalo per una priorità da 1 a 3 e per una stima da 1 a 24. Il minimo sarà sempre minore o uguale al massimo.

**Verifica:** per la priorità prova `abc`, 0 e 3; per le ore prova 25 e 24. I valori accettati devono essere 3 e 24.

<details>
<summary>Mostra la soluzione</summary>

```csharp
int priority = ReadIntInRange("Priorità:", 1, 3);
int hours = ReadIntInRange("Ore stimate:", 1, 24);
Console.WriteLine($"Priorità: {priority}; ore: {hours}");

static int ReadIntInRange(string prompt, int minimum, int maximum)
{
    while (true)
    {
        Console.WriteLine($"{prompt} Da {minimum} a {maximum}.");
        string input = Console.ReadLine() ?? "";
        if (int.TryParse(input, out int value)
            && value >= minimum && value <= maximum)
        {
            return value;
        }

        Console.WriteLine("Valore non valido, riprova.");
    }
}
```

`return` termina il metodo appena arriva un valore valido.

</details>

## 6. Uno sconto opzionale

**Obiettivo:** parametro opzionale e argomenti denominati.

Scrivi `ApplyDiscount(decimal amount, int percentage = 10)`. Restituisci l'importo dopo lo sconto. Per questo esercizio l'importo è non negativo e la percentuale è tra 0 e 100. Provalo omettendo la percentuale e poi indicandola con il suo nome.

**Verifica:** 100 con sconto predefinito → 90; 100 con 20% → 80; 100 con 0% → 100; 100 con 100% → 0.

<details>
<summary>Mostra la soluzione</summary>

```csharp
Console.WriteLine(ApplyDiscount(100m));
Console.WriteLine(ApplyDiscount(amount: 100m, percentage: 20));
Console.WriteLine(ApplyDiscount(100m, 0));
Console.WriteLine(ApplyDiscount(100m, 100));

static decimal ApplyDiscount(decimal amount, int percentage = 10)
{
    return amount - amount * percentage / 100m;
}
```

Il valore predefinito si usa quando il chiamante omette l'argomento.

</details>

## 7. Due modi di creare un riepilogo

**Obiettivo:** overload.

Scrivi due metodi `CreateSummary`: il primo riceve solo il titolo, il secondo titolo e priorità. Il primo restituisce `Ticket: <titolo>`, il secondo aggiunge ` - priorità <numero>`. Non usare parametri opzionali.

**Verifica:** una chiamata con un argomento produce il riepilogo semplice, una con due produce quello con priorità.

<details>
<summary>Mostra la soluzione</summary>

```csharp
class Program
{
    static void Main()
    {
        Console.WriteLine(CreateSummary("Stampante bloccata"));
        Console.WriteLine(CreateSummary("Stampante bloccata", 2));
    }

    static string CreateSummary(string title)
    {
        return $"Ticket: {title}";
    }

    static string CreateSummary(string title, int priority)
    {
        return $"{CreateSummary(title)} - priorità {priority}";
    }
}
```

Gli overload hanno lo stesso nome ma parametri diversi. Il secondo riutilizza il primo.

</details>

## 8. Validare con `out`

**Obiettivo:** restituire esito e valore convertito.

Scrivi `TryReadPriority(string input, out int priority)`. Restituisce `true` per interi tra 1 e 3; negli altri casi restituisce `false` e assegna 0 al parametro `out`. Non leggere dalla console dentro il metodo.

**Verifica:** `"2"` → `true`, 2; `"9"` e `"abc"` → `false`, 0.

<details>
<summary>Mostra la soluzione</summary>

```csharp
string[] inputs = { "2", "9", "abc" };
foreach (string input in inputs)
{
    bool success = TryReadPriority(input, out int priority);
    Console.WriteLine($"{input}: {success}, {priority}");
}

static bool TryReadPriority(string input, out int priority)
{
    priority = 0;
    if (!int.TryParse(input, out int value) || value < 1 || value > 3)
    {
        return false;
    }

    priority = value;
    return true;
}
```

Il parametro `out` deve ricevere un valore in tutti i percorsi del metodo.

</details>

## 9. Modificare un contatore con `ref`

**Obiettivo:** osservare la differenza tra passaggio per valore e per riferimento.

Scrivi `IncrementCopy(int count)` e `IncrementOriginal(ref int count)`, entrambi `void`, che incrementano il parametro di uno. Parti da un contatore uguale a 0, chiama i metodi in questo ordine e stampa il contatore dopo ciascuna chiamata.

**Verifica:** dopo il primo metodo il contatore è ancora 0; dopo il secondo è 1.

<details>
<summary>Mostra la soluzione</summary>

```csharp
int count = 0;
IncrementCopy(count);
Console.WriteLine(count);
IncrementOriginal(ref count);
Console.WriteLine(count);

static void IncrementCopy(int count)
{
    count++;
}

static void IncrementOriginal(ref int count)
{
    count++;
}
```

Nel primo metodo cambia una copia. Nel secondo cambia la variabile del chiamante; `ref` compare anche nella chiamata.

</details>

## 10. Sommare un numero variabile di stime

**Obiettivo:** `params`, array e contratto del metodo.

Scrivi `SumHours(params int[] estimates)`. Somma le stime, che per questo esercizio sono non negative. Il metodo deve funzionare anche senza argomenti. Aggiungi un commento XML che descriva cosa restituisce quando non riceve stime.

**Verifica:** (2, 3, 4) → 9; (5) → 5; nessun argomento → 0.

<details>
<summary>Mostra la soluzione</summary>

```csharp
Console.WriteLine(SumHours(2, 3, 4));
Console.WriteLine(SumHours(5));
Console.WriteLine(SumHours());

/// <summary>Somma le stime non negative ricevute.</summary>
/// <param name="estimates">Ore stimate per gli interventi.</param>
/// <returns>Il totale delle ore, oppure 0 se non ci sono stime.</returns>
static int SumHours(params int[] estimates)
{
    int total = 0;
    foreach (int hours in estimates)
    {
        total += hours;
    }

    return total;
}
```

`params` raccoglie gli argomenti in un array. Senza argomenti l'array è vuoto.

</details>

## 11. Convertire una temperatura

**Obiettivo:** metodo di calcolo e tipo `decimal`.

Scrivi `ToFahrenheit(decimal celsius)`, che converte una temperatura con la formula `celsius * 9 / 5 + 32`. Il metodo restituisce il risultato senza stampare. Provalo con tre temperature.

**Verifica:** 0 °C → 32 °F; 100 °C → 212 °F; -40 °C → -40 °F.

<details>
<summary>Mostra la soluzione</summary>

```csharp
Console.WriteLine(ToFahrenheit(0m));
Console.WriteLine(ToFahrenheit(100m));
Console.WriteLine(ToFahrenheit(-40m));

static decimal ToFahrenheit(decimal celsius)
{
    return celsius * 9m / 5m + 32m;
}
```

La conversione è un'operazione indipendente: il chiamante decide come mostrarne il risultato.

</details>

## 12. Trovare il massimo, anche senza dati

**Obiettivo:** contratto di un metodo e parametro `out`.

Scrivi `TryFindMaximum(int[] numbers, out int maximum)`. Se l'array contiene dati, restituisce `true` e assegna il massimo; se è vuoto, restituisce `false` e assegna 0. Non usare `Max`.

**Verifica:** `{ -5, -2, -9 }` → true e -2; array vuoto → false e 0. Lo 0 in caso di fallimento non è un massimo trovato.

<details>
<summary>Mostra la soluzione</summary>

```csharp
int[] numbers = { -5, -2, -9 };
if (TryFindMaximum(numbers, out int maximum))
{
    Console.WriteLine($"Massimo: {maximum}");
}
else
{
    Console.WriteLine("Nessun dato");
}
Console.WriteLine(TryFindMaximum(Array.Empty<int>(), out int emptyMaximum));
Console.WriteLine(emptyMaximum);

static bool TryFindMaximum(int[] numbers, out int maximum)
{
    maximum = 0;
    if (numbers.Length == 0)
    {
        return false;
    }
    maximum = numbers[0];
    for (int index = 1; index < numbers.Length; index++)
    {
        if (numbers[index] > maximum)
        {
            maximum = numbers[index];
        }
    }
    return true;
}
```

L'esito permette di distinguere un array vuoto da uno il cui massimo vale davvero 0.

</details>

## 13. Contare le vocali con due metodi

**Obiettivo:** scomporre una regola e riutilizzarla.

Scrivi `IsVowel(char character)`, che riconosce a, e, i, o, u ignorando maiuscole e minuscole, e `CountVowels(string text)`, che usa il primo metodo. Non considerare vocali accentate. Usa stringhe non nulle.

**Verifica:** `"Ciao MONDO"` → 5; `"xyz"` e `""` → 0; `IsVowel('A')` → true.

<details>
<summary>Mostra la soluzione</summary>

```csharp
Console.WriteLine(CountVowels("Ciao MONDO"));
Console.WriteLine(CountVowels("xyz"));
Console.WriteLine(CountVowels(""));
Console.WriteLine(IsVowel('A'));

static bool IsVowel(char character)
{
    char lower = char.ToLowerInvariant(character);
    return lower == 'a' || lower == 'e' || lower == 'i'
        || lower == 'o' || lower == 'u';
}

static int CountVowels(string text)
{
    int count = 0;
    foreach (char character in text)
    {
        if (IsVowel(character))
        {
            count++;
        }
    }
    return count;
}
```

Un metodo riconosce una vocale; l'altro conta. Ogni metodo ha una responsabilità facilmente verificabile.

</details>

## 14. Restituire una stringa al contrario

**Obiettivo:** metodo che produce un nuovo risultato.

Scrivi `ReverseText(string text)` senza usare `Reverse`. Il metodo restituisce un nuovo testo e non stampa. Usa stringhe non nulle di caratteri semplici, senza emoji o caratteri Unicode composti.

**Verifica:** Roma → amoR; a → a; testo vuoto → testo vuoto. La stringa originale deve rimanere uguale.

<details>
<summary>Mostra la soluzione</summary>

```csharp
string original = "Roma";
string reversed = ReverseText(original);
Console.WriteLine(original);
Console.WriteLine(reversed);
Console.WriteLine($"[{ReverseText("")}]");

static string ReverseText(string text)
{
    string result = "";
    for (int index = text.Length - 1; index >= 0; index--)
    {
        result += text[index];
    }
    return result;
}
```

Il risultato nasce dentro il metodo e viene consegnato al chiamante con `return`.

</details>

## 15. Verificare un palindromo riutilizzando il codice

**Obiettivo:** comporre metodi.

Scrivi `IsPalindrome(string text)` usando `ReverseText` dell'esercizio precedente. Ignora maiuscole e minuscole, ma conserva spazi e punteggiatura. Considera palindromi il testo vuoto e quello di un solo carattere. Usa stringhe non nulle.

**Verifica:** Anna → true; casa → false; a e testo vuoto → true.

<details>
<summary>Mostra la soluzione</summary>

```csharp
Console.WriteLine(IsPalindrome("Anna"));
Console.WriteLine(IsPalindrome("casa"));
Console.WriteLine(IsPalindrome("a"));
Console.WriteLine(IsPalindrome(""));

static bool IsPalindrome(string text)
{
    return string.Equals(
        text,
        ReverseText(text),
        StringComparison.OrdinalIgnoreCase);
}

static string ReverseText(string text)
{
    string result = "";
    for (int index = text.Length - 1; index >= 0; index--)
    {
        result += text[index];
    }
    return result;
}
```

Un metodo può risolvere un problema affidando parte del lavoro a un altro metodo.

</details>

## 16. La media con un esito esplicito

**Obiettivo:** gestire l'assenza di dati senza dividere per zero.

Scrivi `TryCalculateAverage(int[] values, out decimal average)`. Con un array non vuoto restituisce `true` e la media; con un array vuoto restituisce `false` e 0. Usa un accumulatore `decimal`.

**Verifica:** `{ 6, 7 }` → true e 6,50; `{ 0 }` → true e 0; array vuoto → false e 0.

<details>
<summary>Mostra la soluzione</summary>

```csharp
int[] values = { 6, 7 };
if (TryCalculateAverage(values, out decimal average))
{
    Console.WriteLine($"Media: {average:F2}");
}
Console.WriteLine(TryCalculateAverage(new int[] { 0 }, out decimal zeroAverage));
Console.WriteLine(zeroAverage);
Console.WriteLine(TryCalculateAverage(Array.Empty<int>(), out decimal emptyAverage));
Console.WriteLine(emptyAverage);

static bool TryCalculateAverage(int[] values, out decimal average)
{
    average = 0m;
    if (values.Length == 0)
    {
        return false;
    }
    decimal total = 0m;
    foreach (int value in values)
    {
        total += value;
    }
    average = total / values.Length;
    return true;
}
```

Il booleano descrive se il calcolo è possibile. Un risultato pari a zero può essere una media valida.

</details>

## 17. Cercare un numero con un metodo

**Obiettivo:** parametri array e valore sentinella.

Scrivi `FindFirstIndex(int[] numbers, int target)`. Restituisci l'indice della prima corrispondenza, oppure -1. Non modificare l'array e non usare `Array.IndexOf`.

**Verifica:** Nell'array `{ 4, 7, 2, 7 }`, cercando 7 ottieni 1, cercando 9 ottieni -1; un array vuoto restituisce -1.

<details>
<summary>Mostra la soluzione</summary>

```csharp
int[] numbers = { 4, 7, 2, 7 };
Console.WriteLine(FindFirstIndex(numbers, 7));
Console.WriteLine(FindFirstIndex(numbers, 9));
Console.WriteLine(FindFirstIndex(Array.Empty<int>(), 7));

static int FindFirstIndex(int[] numbers, int target)
{
    for (int index = 0; index < numbers.Length; index++)
    {
        if (numbers[index] == target)
        {
            return index;
        }
    }
    return -1;
}
```

Il primo `return` conclude la ricerca appena trova il dato. Quello finale gestisce l'assenza.

</details>

## 18. Un metodo può modificare gli elementi di un array?

**Obiettivo:** passaggio per valore di un riferimento.

Scrivi `DoubleValues(int[] numbers)`, che raddoppia ogni elemento, e `ReplaceLocalArray(int[] numbers)`, che assegna al parametro un nuovo array `{ 99 }`. Parti da `{ 1, 2, 3 }` e stampa l'array dopo ciascuna chiamata. Per evitare overflow usa valori piccoli.

**Verifica:** Dopo il raddoppio: 2, 4, 6; dopo la sostituzione locale: ancora 2, 4, 6. Non usare `ref`.

<details>
<summary>Mostra la soluzione</summary>

```csharp
int[] numbers = { 1, 2, 3 };
DoubleValues(numbers);
Console.WriteLine(string.Join(", ", numbers));
ReplaceLocalArray(numbers);
Console.WriteLine(string.Join(", ", numbers));

static void DoubleValues(int[] numbers)
{
    for (int index = 0; index < numbers.Length; index++)
    {
        numbers[index] *= 2;
    }
}

static void ReplaceLocalArray(int[] numbers)
{
    numbers = new int[] { 99 };
}
```

Il parametro riceve una copia del riferimento allo stesso array, quindi può modificarne gli elementi. Assegnargli un altro array cambia soltanto il riferimento locale.

</details>

## 19. Restituire una copia ordinata

**Obiettivo:** separare copia e ordinamento in metodi.

Scrivi `CopyNumbers(int[] numbers)`, che copia gli elementi in un nuovo array, e `SortCopy(int[] numbers)`, che usa quella copia e la ordina con cicli e scambi. L'originale deve restare invariato. Non usare `Array.Sort`, `Clone` o LINQ.

**Verifica:** Originale `{ 3, 1, 2, 1 }` invariato; risultato 1, 1, 2, 3. Con array vuoto restituisci un nuovo array vuoto.

<details>
<summary>Mostra la soluzione</summary>

```csharp
int[] original = { 3, 1, 2, 1 };
int[] sorted = SortCopy(original);
Console.WriteLine($"Originale: {string.Join(", ", original)}");
Console.WriteLine($"Ordinato: {string.Join(", ", sorted)}");

static int[] CopyNumbers(int[] numbers)
{
    int[] copy = new int[numbers.Length];
    for (int index = 0; index < numbers.Length; index++)
    {
        copy[index] = numbers[index];
    }
    return copy;
}

static int[] SortCopy(int[] numbers)
{
    int[] result = CopyNumbers(numbers);
    for (int position = 0; position < result.Length - 1; position++)
    {
        int minimumIndex = position;
        for (int index = position + 1; index < result.Length; index++)
        {
            if (result[index] < result[minimumIndex])
            {
                minimumIndex = index;
            }
        }
        int temporary = result[position];
        result[position] = result[minimumIndex];
        result[minimumIndex] = temporary;
    }
    return result;
}
```

Il nome `SortCopy` comunica che il metodo restituisce un altro array. Il ciclo di ordinamento lavora sulla copia.

</details>

## 20. Un piccolo menu per lavorare con il testo

**Obiettivo:** coordinare metodi con responsabilità distinte.

Scrivi un programma che chiede un testo, poi propone un menu: 1 conta le vocali, 2 inverte il testo, 0 termina. Il menu si ripete fino a 0; altre scelte producono un messaggio. Usa `ReadChoice()`, `CountVowels(string text)` e `ReverseText(string text)`. Il programma principale decide quale metodo chiamare. Conta a, e, i, o, u senza accenti; usa testo semplice senza emoji.

**Verifica:** Inserisci Roma, poi 1 → 2 vocali; 2 → amoR; 9 → scelta non valida; 0 → uscita. Il testo vuoto è ammesso: conta 0 vocali e resta vuoto se invertito.

<details>
<summary>Mostra la soluzione</summary>

```csharp
Console.WriteLine("Inserisci un testo:");
string text = Console.ReadLine() ?? "";
bool running = true;
while (running)
{
    string choice = ReadChoice();
    switch (choice)
    {
        case "1":
            Console.WriteLine($"Vocali: {CountVowels(text)}");
            break;
        case "2":
            Console.WriteLine(ReverseText(text));
            break;
        case "0":
            running = false;
            break;
        default:
            Console.WriteLine("Scelta non valida.");
            break;
    }
}

static string ReadChoice()
{
    Console.WriteLine("1 Conta vocali; 2 Inverti testo; 0 Esci");
    return (Console.ReadLine() ?? "").Trim();
}

static int CountVowels(string text)
{
    int count = 0;
    foreach (char character in text.ToLowerInvariant())
    {
        if (character == 'a' || character == 'e' || character == 'i'
            || character == 'o' || character == 'u')
        {
            count++;
        }
    }
    return count;
}

static string ReverseText(string text)
{
    string result = "";
    for (int index = text.Length - 1; index >= 0; index--)
    {
        result += text[index];
    }
    return result;
}
```

La lettura del menu, il conteggio e l'inversione sono operazioni separate. Il ciclo principale le collega e gestisce l'uscita.

</details>
