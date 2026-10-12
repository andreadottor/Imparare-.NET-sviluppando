# Capitolo 3 — Esercizi su tipi, condizioni e cicli

Quaranta esercizi per prendere confidenza con la logica di un programma: variabili, condizioni, cicli, stringhe e array, fino ai primi algoritmi di ordinamento. Le situazioni sono semplici e non richiedono di conoscere l'applicazione del libro. Svolgili in ordine: prima prova a scrivere il codice, poi apri la soluzione e confronta le scelte. Le soluzioni sono chiuse per impostazione predefinita nei visualizzatori Markdown che supportano `<details>`.

## Come lavorare

Crea un progetto console separato con `dotnet new console -n Chapter03Practice`. Per ogni esercizio sostituisci **tutto** il contenuto di `Program.cs` con il tuo codice e avvia il progetto con `dotnet run --project Chapter03Practice`. Anche ogni soluzione è un programma completo da copiare nello stesso file, una alla volta. Usa l'SDK .NET indicato nel repository. Non occorre modificare `src` o `exercise`.

Per gli esercizi interattivi inserisci i valori dalla tastiera. Verifica anche i casi limite proposti, non soltanto il primo esempio funzionante.

Il percorso passa dalle basi (1–10) ai cicli e ai piccoli problemi numerici (11–15), poi alle stringhe (16–23) e agli array (24–40). Gli ultimi esercizi richiedono più ragionamento: aiutati con carta e penna per seguire il valore delle variabili a ogni giro. Usa cicli e condizioni, senza LINQ o metodi pronti che risolvano direttamente il problema. `string.Join` nelle soluzioni serve solo a stampare gli array.

## 1. Quanto costa un intervento?

**Obiettivo:** variabili, `int`, `decimal` e moltiplicazione.

Un intervento richiede 3 ore e costa 25,50 euro all'ora. Memorizza i due dati in variabili e stampa il costo totale con due cifre decimali. Poi cambia le ore a 0 e a 5.

**Verifica:** il totale iniziale è 76,50 euro; con 0 ore è 0; con 5 ore è 127,50. Il separatore decimale visualizzato dipende dalle impostazioni del computer.

<details>
<summary>Mostra la soluzione</summary>

```csharp
int hours = 3;
decimal hourlyRate = 25.50m;
decimal total = hours * hourlyRate;

Console.WriteLine($"Costo totale: {total:F2} euro");
```

Il suffisso `m` indica un valore `decimal`. Le ore sono intere, mentre la tariffa può avere una parte decimale.

</details>

## 2. Pari o dispari?

**Obiettivo:** `if`, `else`, resto della divisione e conversione dell'input.

Chiedi un numero intero. Se il testo non è convertibile, stampa un messaggio; altrimenti indica se il numero è pari o dispari. Basta una sola lettura: in questo esercizio non serve ripetere la domanda.

**Verifica:** 8 è pari, 7 è dispari, 0 e -4 sono pari; `ciao` non è un numero valido.

<details>
<summary>Mostra la soluzione</summary>

```csharp
Console.WriteLine("Inserisci un numero intero:");
string input = Console.ReadLine() ?? "";

if (!int.TryParse(input, out int number))
{
    Console.WriteLine("Il testo non è un numero intero valido.");
}
else if (number % 2 == 0)
{
    Console.WriteLine("Pari");
}
else
{
    Console.WriteLine("Dispari");
}
```

Un numero è pari quando il resto della divisione per 2 è zero.

</details>

## 3. Quale priorità?

**Obiettivo:** `switch` e gestione del caso non previsto.

Chiedi la priorità di un ticket: 1 significa Alta, 2 Media e 3 Bassa. Stampa l'etichetta corrispondente. Distingui un testo non numerico da un numero fuori dall'intervallo.

**Verifica:** 1 → Alta, 2 → Media, 3 → Bassa, 4 → Priorità non valida; `abc` → messaggio di conversione fallita.

<details>
<summary>Mostra la soluzione</summary>

```csharp
Console.WriteLine("Priorità (1 Alta, 2 Media, 3 Bassa):");
string input = Console.ReadLine() ?? "";

if (int.TryParse(input, out int priority))
{
    switch (priority)
    {
        case 1:
            Console.WriteLine("Alta");
            break;
        case 2:
            Console.WriteLine("Media");
            break;
        case 3:
            Console.WriteLine("Bassa");
            break;
        default:
            Console.WriteLine("Priorità non valida");
            break;
    }
}
else
{
    Console.WriteLine("Inserisci un numero intero.");
}
```

`default` gestisce tutti i numeri che non corrispondono ai tre casi.

</details>

## 4. Serve una valutazione?

**Obiettivo:** confronti e operatori logici `||` e `&&`.

Usa due variabili: `priority` e `estimatedHours`. Prima verifica che la priorità sia tra 1 e 3 e le ore siano positive. Per dati validi, un ticket richiede una valutazione se ha priorità 1 **oppure** richiede più di 8 ore. Stampa il risultato.

**Verifica:** (1, 2) e (3, 9) richiedono valutazione; (2, 8) no; (4, 2) e (1, 0) sono dati non validi.

<details>
<summary>Mostra la soluzione</summary>

```csharp
int priority = 2;
int estimatedHours = 8;
bool isValid = priority >= 1 && priority <= 3 && estimatedHours > 0;

if (!isValid)
{
    Console.WriteLine("Dati non validi.");
}
else
{
    bool requiresEvaluation = priority == 1 || estimatedHours > 8;
    Console.WriteLine($"Richiede valutazione: {requiresEvaluation}");
}
```

`&&` richiede che tutte le condizioni siano vere; con `||` ne basta una.

</details>

## 5. La tabellina

**Obiettivo:** contatore e ciclo `for`.

Memorizza il numero 7 e stampa la sua tabellina da 1 a 10, una moltiplicazione per riga. Poi prova con 0 e con -2.

**Verifica:** con 7 la prima riga è `7 x 1 = 7`, l'ultima è `7 x 10 = 70`. Devono esserci esattamente dieci righe.

<details>
<summary>Mostra la soluzione</summary>

```csharp
int number = 7;

for (int multiplier = 1; multiplier <= 10; multiplier++)
{
    Console.WriteLine($"{number} x {multiplier} = {number * multiplier}");
}
```

La condizione include 10. Con `< 10` mancherebbe l'ultima riga.

</details>

## 6. Sommare da 1 a N

**Obiettivo:** accumulatore e ciclo `for`.

Usa una variabile `limit` tra 1 e 100. Somma tutti gli interi da 1 a quel numero usando un ciclo, senza usare una formula matematica. Se il limite è fuori intervallo, stampa un errore.

**Verifica:** 1 → 1; 5 → 15; 100 → 5050; 0 e 101 non sono validi.

<details>
<summary>Mostra la soluzione</summary>

```csharp
int limit = 5;

if (limit < 1 || limit > 100)
{
    Console.WriteLine("Il limite deve essere tra 1 e 100.");
}
else
{
    int total = 0;
    for (int number = 1; number <= limit; number++)
    {
        total += number;
    }

    Console.WriteLine($"Somma: {total}");
}
```

L'accumulatore nasce fuori dal ciclo: il suo valore deve sopravvivere tra le iterazioni.

</details>

## 7. Una priorità davvero valida

**Obiettivo:** ripetere una lettura con `while`.

Continua a chiedere la priorità finché l'utente non inserisce un intero tra 1 e 3. Segnala ogni tentativo non valido. Alla fine stampa la priorità accettata.

**Verifica:** inserendo nell'ordine `abc`, 0, 4 e 2, il programma rifiuta i primi tre valori e termina la lettura con 2.

<details>
<summary>Mostra la soluzione</summary>

```csharp
int priority = 0;
bool isValid = false;

while (!isValid)
{
    Console.WriteLine("Inserisci una priorità tra 1 e 3:");
    string input = Console.ReadLine() ?? "";
    isValid = int.TryParse(input, out priority)
        && priority >= 1 && priority <= 3;

    if (!isValid)
    {
        Console.WriteLine("Priorità non valida, riprova.");
    }
}

Console.WriteLine($"Priorità accettata: {priority}");
```

La condizione cambia quando arriva un valore valido: così il ciclo può terminare.

</details>

## 8. Un titolo obbligatorio

**Obiettivo:** `do-while`, stringhe e spazi.

Chiedi il titolo di un ticket almeno una volta. Ripeti la domanda se il testo è vuoto o contiene solo spazi. Stampa il titolo accettato senza spazi iniziali e finali.

**Verifica:** Invio e tre spazi devono essere rifiutati; `  Stampante bloccata  ` deve diventare `Stampante bloccata`.

<details>
<summary>Mostra la soluzione</summary>

```csharp
string title;

do
{
    Console.WriteLine("Inserisci il titolo:");
    title = Console.ReadLine() ?? "";

    if (string.IsNullOrWhiteSpace(title))
    {
        Console.WriteLine("Il titolo è obbligatorio.");
    }
}
while (string.IsNullOrWhiteSpace(title));

Console.WriteLine($"Titolo: {title.Trim()}");
```

`do-while` esegue il corpo prima di controllare la condizione.

</details>

## 9. Quante ore in totale?

**Obiettivo:** array, `foreach`, somma e contatore.

Parti dall'array di stime `{ 2, 5, 1, 9 }`. Stampa ogni stima, calcola le ore totali e conta quante stime superano 4 ore. Non usare LINQ.

**Verifica:** totale 17 ore e 2 stime sopra 4. Prova anche un array vuoto: entrambi i risultati devono essere 0.

<details>
<summary>Mostra la soluzione</summary>

```csharp
int[] estimates = { 2, 5, 1, 9 };
int totalHours = 0;
int longEstimateCount = 0;

foreach (int hours in estimates)
{
    Console.WriteLine($"Stima: {hours} ore");
    totalHours += hours;

    if (hours > 4)
    {
        longEstimateCount++;
    }
}

Console.WriteLine($"Totale: {totalHours} ore");
Console.WriteLine($"Stime sopra 4 ore: {longEstimateCount}");
```

Somma e conteggio sono due informazioni diverse: servono due variabili.

</details>

## 10. Un piccolo riepilogo interattivo

**Obiettivo:** combinare `while`, `if`, `break` e `continue`.

Chiedi stime intere tra 1 e 24 ore. Il valore 0 termina l'inserimento. Rifiuta testi non numerici e valori fuori intervallo senza contarli. Alla fine stampa quante stime valide sono state inserite e la loro somma.

**Verifica:** con `abc`, -2, 25, 2, 5, 0 il risultato è 2 stime e 7 ore. Con 0 come primo input, entrambi i risultati sono 0.

<details>
<summary>Mostra la soluzione</summary>

```csharp
int count = 0;
int totalHours = 0;

while (true)
{
    Console.WriteLine("Ore da 1 a 24 (0 per terminare):");
    string input = Console.ReadLine() ?? "";

    if (!int.TryParse(input, out int hours))
    {
        Console.WriteLine("Inserisci un numero intero.");
        continue;
    }

    if (hours == 0)
    {
        break;
    }

    if (hours < 1 || hours > 24)
    {
        Console.WriteLine("La stima deve essere tra 1 e 24.");
        continue;
    }

    count++;
    totalHours += hours;
}

Console.WriteLine($"Stime valide: {count}");
Console.WriteLine($"Ore totali: {totalHours}");
```

`continue` passa alla lettura successiva; `break` esce dal ciclo. Lo 0 viene controllato prima dell'intervallo perché ha un significato speciale.

</details>

## 11. Conto alla rovescia

**Obiettivo:** `while` e aggiornamento del contatore.

Parti da 5, stampa tutti i numeri fino a 1 e infine `Via!`. Usa un ciclo `while`. Prova anche a partire da 0: in quel caso stampa soltanto `Via!`.

**Verifica:** Con 5 compaiono 5, 4, 3, 2, 1 e poi `Via!`.

<details>
<summary>Mostra la soluzione</summary>

```csharp
int remaining = 5;
while (remaining > 0)
{
    Console.WriteLine(remaining);
    remaining--;
}
Console.WriteLine("Via!");
```

Il contatore diminuisce a ogni giro, avvicinandosi alla condizione di uscita.

</details>

## 12. Multipli di tre o di cinque

**Obiettivo:** resto della divisione e condizioni ordinate.

Per i numeri da 1 a 30 stampa `Tre` per i multipli di 3, `Cinque` per quelli di 5 e `Tre e cinque` per quelli di entrambi. Negli altri casi stampa il numero.

**Verifica:** 3 → Tre; 5 → Cinque; 15 e 30 → Tre e cinque; 7 → 7.

<details>
<summary>Mostra la soluzione</summary>

```csharp
for (int number = 1; number <= 30; number++)
{
    if (number % 3 == 0 && number % 5 == 0)
    {
        Console.WriteLine("Tre e cinque");
    }
    else if (number % 3 == 0)
    {
        Console.WriteLine("Tre");
    }
    else if (number % 5 == 0)
    {
        Console.WriteLine("Cinque");
    }
    else
    {
        Console.WriteLine(number);
    }
}
```

Controlla prima il caso più specifico: un multiplo di entrambi è anche un multiplo di 3.

</details>

## 13. Moltiplicare da 1 a N

**Obiettivo:** accumulatore moltiplicativo.

Calcola il prodotto degli interi da 1 a `number`, usando un ciclo. Accetta numeri da 0 a 10; per 0 il risultato deve essere 1. Questo prodotto si chiama fattoriale.

**Verifica:** 5 → 120; 1 → 1; 0 → 1; -1 e 11 → errore.

<details>
<summary>Mostra la soluzione</summary>

```csharp
int number = 5;
if (number < 0 || number > 10)
{
    Console.WriteLine("Scegli un numero da 0 a 10.");
}
else
{
    int product = 1;
    for (int factor = 1; factor <= number; factor++)
    {
        product *= factor;
    }
    Console.WriteLine(product);
}
```

Un prodotto parte da 1: partendo da 0 ogni moltiplicazione darebbe 0. Il limite mantiene il risultato nel tipo `int`.

</details>

## 14. Il numero è primo?

**Obiettivo:** ricerca e interruzione anticipata.

Usa un intero tra -100 e 100. Un numero è primo se è maggiore di 1 ed è divisibile soltanto per 1 e per se stesso. Prova tutti i possibili divisori da 2 fino al numero escluso e fermati appena ne trovi uno.

**Verifica:** 2, 7 e 13 sono primi; -3, 0, 1, 9 e 12 non lo sono.

<details>
<summary>Mostra la soluzione</summary>

```csharp
int number = 13;
bool isPrime = number > 1;
for (int divisor = 2; divisor < number && isPrime; divisor++)
{
    if (number % divisor == 0)
    {
        isPrime = false;
        break;
    }
}
Console.WriteLine($"Primo: {isPrime}");
```

Basta trovare un divisore intermedio per escludere che il numero sia primo.

</details>

## 15. Un rettangolo di asterischi

**Obiettivo:** cicli annidati e differenza tra `Write` e `WriteLine`.

Con `rows = 3` e `columns = 4`, stampa un rettangolo di asterischi: tre righe, ciascuna di quattro caratteri. Usa due cicli. Le dimensioni possono essere tra 0 e 10.

**Verifica:** Il risultato è tre righe di `****`. Con una dimensione uguale a 0 non stampare il rettangolo.

<details>
<summary>Mostra la soluzione</summary>

```csharp
int rows = 3;
int columns = 4;
if (rows > 0 && columns > 0)
{
    for (int row = 0; row < rows; row++)
    {
        for (int column = 0; column < columns; column++)
        {
            Console.Write("*");
        }
        Console.WriteLine();
    }
}
```

Il ciclo esterno sceglie la riga; quello interno scrive i caratteri di quella riga.

</details>

## 16. Ripulire un nome

**Obiettivo:** `Trim`, `Length` e testo vuoto.

Parti da `"  Anna Maria  "`. Rimuovi soltanto gli spazi iniziali e finali, stampa il nome tra parentesi quadre e la sua lunghezza. Se dopo la pulizia non rimane testo, stampa `Nome mancante`.

**Verifica:** Il nome diventa `[Anna Maria]` e contiene 10 caratteri, incluso lo spazio interno. `"   "` → Nome mancante.

<details>
<summary>Mostra la soluzione</summary>

```csharp
string name = "  Anna Maria  ";
string cleanedName = name.Trim();
if (cleanedName.Length == 0)
{
    Console.WriteLine("Nome mancante");
}
else
{
    Console.WriteLine($"[{cleanedName}]");
    Console.WriteLine($"Caratteri: {cleanedName.Length}");
}
```

Lo spazio tra Anna e Maria fa parte del nome e non viene rimosso da `Trim()`.

</details>

## 17. Un carattere per riga

**Obiettivo:** indice di una stringa e ciclo `for`.

Stampa ogni carattere di `"ciao"` su una riga separata, preceduto dal suo indice. Non usare `Split`. Prova anche una stringa vuota.

**Verifica:** Le righe sono `0: c`, `1: i`, `2: a`, `3: o`. Con testo vuoto non compaiono righe.

<details>
<summary>Mostra la soluzione</summary>

```csharp
string text = "ciao";
for (int index = 0; index < text.Length; index++)
{
    Console.WriteLine($"{index}: {text[index]}");
}
```

Gli indici partono da 0 e arrivano a `Length - 1`. La condizione deve quindi usare `<`, non `<=`.

</details>

## 18. Contare le vocali

**Obiettivo:** `foreach` su una stringa e confronti tra caratteri.

Conta quante vocali ci sono in un testo, ignorando maiuscole e minuscole. Considera soltanto a, e, i, o, u senza accenti. Spazi, cifre e punteggiatura non contano.

**Verifica:** `"Ciao MONDO!"` → 5; `"xyz"` e `""` → 0.

<details>
<summary>Mostra la soluzione</summary>

```csharp
string text = "Ciao MONDO!";
int vowelCount = 0;
foreach (char character in text.ToLowerInvariant())
{
    if (character == 'a' || character == 'e' || character == 'i'
        || character == 'o' || character == 'u')
    {
        vowelCount++;
    }
}
Console.WriteLine(vowelCount);
```

Il ciclo visita i caratteri. Convertire il testo in minuscolo evita di duplicare tutti i confronti.

</details>

## 19. Quante volte compare una lettera?

**Obiettivo:** contare una corrispondenza.

Conta quante volte compare il carattere `a` in `"Banana"`, ignorando maiuscole e minuscole. Memorizza il carattere cercato in una variabile. Non usare metodi di conteggio pronti.

**Verifica:** Banana → 3; cercando z → 0; un testo vuoto → 0.

<details>
<summary>Mostra la soluzione</summary>

```csharp
string text = "Banana";
char target = 'a';
int count = 0;
foreach (char character in text)
{
    if (char.ToLowerInvariant(character) == char.ToLowerInvariant(target))
    {
        count++;
    }
}
Console.WriteLine(count);
```

Il contatore aumenta solo quando il carattere corrente coincide con quello cercato.

</details>

## 20. Scrivere una stringa al contrario

**Obiettivo:** scorrimento dall'ultimo indice al primo.

Costruisci una nuova stringa con i caratteri di `"Roma"` in ordine inverso, senza usare `Reverse`. Conserva maiuscole, spazi e punteggiatura. Per questo esercizio usa testo semplice, senza emoji o caratteri Unicode composti.

**Verifica:** Roma → amoR; `"ab cd"` → `"dc ba"`; `""` → `""`.

<details>
<summary>Mostra la soluzione</summary>

```csharp
string text = "Roma";
string reversed = "";
for (int index = text.Length - 1; index >= 0; index--)
{
    reversed += text[index];
}
Console.WriteLine(reversed);
```

Il primo carattere letto è l'ultimo del testo. La concatenazione è sufficiente per questi testi brevi.

</details>

## 21. Una parola palindroma

**Obiettivo:** confrontare coppie di caratteri.

Una parola è palindroma se si legge allo stesso modo nei due sensi. Confronta il primo carattere con l'ultimo, il secondo con il penultimo e così via. Ignora maiuscole e minuscole, ma non rimuovere spazi o punteggiatura. Considera il testo vuoto palindromo.

**Verifica:** Anna, radar, a e testo vuoto → true; casa → false.

<details>
<summary>Mostra la soluzione</summary>

```csharp
string word = "Anna";
string normalized = word.ToLowerInvariant();
bool isPalindrome = true;
for (int index = 0; index < normalized.Length / 2; index++)
{
    if (normalized[index] != normalized[normalized.Length - 1 - index])
    {
        isPalindrome = false;
        break;
    }
}
Console.WriteLine(isPalindrome);
```

Basta visitare metà della parola: ogni confronto controlla contemporaneamente due posizioni.

</details>

## 22. Sostituire gli spazi senza `Replace`

**Obiettivo:** costruire un testo con un ciclo.

Trasforma ogni spazio normale di `"pane latte uova"` in un trattino, costruendo una nuova stringa. Non usare `Replace`. Lascia invariati tutti gli altri caratteri; due spazi diventano due trattini.

**Verifica:** `"pane latte uova"` → `"pane-latte-uova"`; `" a  b "` → `"-a--b-"`.

<details>
<summary>Mostra la soluzione</summary>

```csharp
string text = "pane latte uova";
string result = "";
foreach (char character in text)
{
    if (character == ' ')
    {
        result += '-';
    }
    else
    {
        result += character;
    }
}
Console.WriteLine(result);
```

A ogni giro decidi quale singolo carattere aggiungere al risultato.

</details>

## 23. Contare le parole

**Obiettivo:** riconoscere il passaggio da uno spazio a una parola.

Conta le parole senza usare `Split`: una parola è una sequenza di caratteri non bianchi, separata da spazi, tabulazioni o ritorni a capo. Usa `char.IsWhiteSpace`. La punteggiatura resta parte della parola.

**Verifica:** `"  Ciao   a tutti  "` → 3; `"ciao"` → 1; `"   "` e testo vuoto → 0.

<details>
<summary>Mostra la soluzione</summary>

```csharp
string text = "  Ciao   a tutti  ";
int wordCount = 0;
bool insideWord = false;
foreach (char character in text)
{
    if (char.IsWhiteSpace(character))
    {
        insideWord = false;
    }
    else if (!insideWord)
    {
        wordCount++;
        insideWord = true;
    }
}
Console.WriteLine(wordCount);
```

Conta l'inizio di ogni parola, non gli spazi: diversi spazi consecutivi non devono creare parole in più.

</details>

## 24. Numerare gli elementi di un array

**Obiettivo:** `Length`, indice e accesso agli elementi.

Usa l'array di frutti `{ "mela", "pera", "banana" }` e stampa una lista numerata a partire da 1, con un ciclo `for`. Prova anche un array vuoto.

**Verifica:** `1. mela`, `2. pera`, `3. banana`. Con array vuoto non ci sono righe.

<details>
<summary>Mostra la soluzione</summary>

```csharp
string[] fruits = { "mela", "pera", "banana" };
for (int index = 0; index < fruits.Length; index++)
{
    Console.WriteLine($"{index + 1}. {fruits[index]}");
}
```

L'indice interno parte da 0; il numero mostrato alla persona può partire da 1.

</details>

## 25. Solo i numeri pari

**Obiettivo:** filtrare un array con una condizione.

Stampa soltanto i numeri pari dell'array `{ 3, 0, -2, 7, 8 }` nell'ordine originale e conta quanti sono. Non usare LINQ.

**Verifica:** Compaiono 0, -2, 8; il conteggio è 3. Con `{ 1, 3 }` il conteggio è 0.

<details>
<summary>Mostra la soluzione</summary>

```csharp
int[] numbers = { 3, 0, -2, 7, 8 };
int count = 0;
foreach (int number in numbers)
{
    if (number % 2 == 0)
    {
        Console.WriteLine(number);
        count++;
    }
}
Console.WriteLine($"Numeri pari: {count}");
```

Il controllo di parità funziona anche per zero e per i numeri negativi.

</details>

## 26. Il minimo e il massimo

**Obiettivo:** scegliere un valore iniziale corretto.

Trova minimo e massimo di `{ -5, -2, -9, -1 }` senza usare `Min`, `Max` o ordinamenti. Se l'array è vuoto, stampa `Nessun dato`.

**Verifica:** Minimo -9, massimo -1; con `{ 4 }` entrambi valgono 4; con array vuoto stampa il messaggio.

<details>
<summary>Mostra la soluzione</summary>

```csharp
int[] numbers = { -5, -2, -9, -1 };
if (numbers.Length == 0)
{
    Console.WriteLine("Nessun dato");
}
else
{
    int minimum = numbers[0];
    int maximum = numbers[0];
    for (int index = 1; index < numbers.Length; index++)
    {
        if (numbers[index] < minimum)
        {
            minimum = numbers[index];
        }
        if (numbers[index] > maximum)
        {
            maximum = numbers[index];
        }
    }
    Console.WriteLine($"Minimo: {minimum}; massimo: {maximum}");
}
```

Partire da 0 darebbe un massimo errato per un array di soli numeri negativi. Parti dal primo dato reale.

</details>

## 27. La media dei voti

**Obiettivo:** somma, divisione decimale e array vuoto.

Calcola la media di `{ 6, 7, 8, 8 }` usando un ciclo. I voti sono interi da 0 a 10. Stampa due decimali; se l'array è vuoto, segnala che non puoi calcolare la media.

**Verifica:** Media 7,25; con `{ 6, 7 }` → 6,50; un array vuoto non deve provocare una divisione per zero.

<details>
<summary>Mostra la soluzione</summary>

```csharp
int[] grades = { 6, 7, 8, 8 };
if (grades.Length == 0)
{
    Console.WriteLine("Nessun voto");
}
else
{
    decimal total = 0m;
    foreach (int grade in grades)
    {
        total += grade;
    }
    decimal average = total / grades.Length;
    Console.WriteLine($"Media: {average:F2}");
}
```

Una somma `decimal` permette una divisione con parte decimale, anche se i voti sono interi.

</details>

## 28. Cercare la prima posizione

**Obiettivo:** ricerca lineare e valore sentinella.

Cerca il numero 7 nell'array `{ 4, 7, 2, 7 }`. Stampa l'indice della prima occorrenza, oppure -1 se non esiste. Non usare `Array.IndexOf`.

**Verifica:** 7 → indice 1; 4 → 0; 9 → -1; array vuoto → -1.

<details>
<summary>Mostra la soluzione</summary>

```csharp
int[] numbers = { 4, 7, 2, 7 };
int target = 7;
int foundIndex = -1;
for (int index = 0; index < numbers.Length; index++)
{
    if (numbers[index] == target)
    {
        foundIndex = index;
        break;
    }
}
Console.WriteLine(foundIndex);
```

-1 non è un indice valido e può rappresentare l'assenza del dato. `break` conserva la prima corrispondenza.

</details>

## 29. Contare tutte le occorrenze

**Obiettivo:** distinguere ricerca e conteggio.

Conta quante volte il numero 2 compare in `{ 2, 1, 2, 3, 2 }`. Controlla tutti gli elementi, senza fermarti alla prima corrispondenza.

**Verifica:** 2 → 3 volte; 8 → 0; array vuoto → 0.

<details>
<summary>Mostra la soluzione</summary>

```csharp
int[] numbers = { 2, 1, 2, 3, 2 };
int target = 2;
int count = 0;
foreach (int number in numbers)
{
    if (number == target)
    {
        count++;
    }
}
Console.WriteLine(count);
```

Qui non usiamo `break`: ogni corrispondenza contribuisce al risultato.

</details>

## 30. Un array al contrario

**Obiettivo:** calcolare l'indice speculare.

Da `{ 10, 20, 30, 40 }` crea un nuovo array con gli elementi in ordine inverso. Non modificare l'originale e non usare `Array.Reverse`. Stampa i due array.

**Verifica:** Originale: 10, 20, 30, 40; risultato: 40, 30, 20, 10. Un array vuoto produce un array vuoto.

<details>
<summary>Mostra la soluzione</summary>

```csharp
int[] original = { 10, 20, 30, 40 };
int[] reversed = new int[original.Length];
for (int index = 0; index < original.Length; index++)
{
    reversed[index] = original[original.Length - 1 - index];
}
Console.WriteLine($"Originale: {string.Join(", ", original)}");
Console.WriteLine($"Invertito: {string.Join(", ", reversed)}");
```

All'indice 0 del risultato va l'ultimo elemento dell'originale, all'indice 1 il penultimo e così via.

</details>

## 31. Una copia davvero indipendente

**Obiettivo:** riferimenti agli array e copia degli elementi.

Copia `{ 1, 2, 3 }` in un nuovo array con un ciclo, senza usare `Clone` o `Array.Copy`. Poi, se la copia ha almeno un elemento, cambia il suo primo valore in 99. Stampa entrambi.

**Verifica:** Originale: 1, 2, 3; copia: 99, 2, 3. Con un array vuoto non accedere alla posizione 0.

<details>
<summary>Mostra la soluzione</summary>

```csharp
int[] original = { 1, 2, 3 };
int[] copy = new int[original.Length];
for (int index = 0; index < original.Length; index++)
{
    copy[index] = original[index];
}
if (copy.Length > 0)
{
    copy[0] = 99;
}
Console.WriteLine($"Originale: {string.Join(", ", original)}");
Console.WriteLine($"Copia: {string.Join(", ", copy)}");
```

Scrivere soltanto `int[] copy = original` condividerebbe lo stesso array. Qui `new` crea un contenitore distinto.

</details>

## 32. Unire due array

**Obiettivo:** dimensione del risultato e offset.

Unisci `{ 1, 2 }` e `{ 3, 4, 5 }` in un nuovo array, mantenendo l'ordine. Usa due cicli, senza `Concat` o metodi di copia.

**Verifica:** Risultato: 1, 2, 3, 4, 5. Se uno dei due array è vuoto, il risultato contiene gli elementi dell'altro.

<details>
<summary>Mostra la soluzione</summary>

```csharp
int[] first = { 1, 2 };
int[] second = { 3, 4, 5 };
int[] combined = new int[first.Length + second.Length];
for (int index = 0; index < first.Length; index++)
{
    combined[index] = first[index];
}
for (int index = 0; index < second.Length; index++)
{
    combined[first.Length + index] = second[index];
}
Console.WriteLine(string.Join(", ", combined));
```

Gli elementi del secondo array iniziano subito dopo quelli del primo: l'offset è `first.Length`.

</details>

## 33. Stampare i valori senza ripetizioni

**Obiettivo:** cicli annidati e ricerca nei dati già visitati.

Stampa i valori distinti di `{ 3, 1, 3, 2, 1 }` nell'ordine della prima comparsa. Per ogni posizione, controlla se lo stesso valore era già presente nelle posizioni precedenti. Non usare `Distinct` o collezioni nuove.

**Verifica:** Risultato: 3, 1, 2; `{ 5, 5, 5 }` → solo 5; array vuoto → nessuna riga.

<details>
<summary>Mostra la soluzione</summary>

```csharp
int[] numbers = { 3, 1, 3, 2, 1 };
for (int index = 0; index < numbers.Length; index++)
{
    bool alreadySeen = false;
    for (int previous = 0; previous < index; previous++)
    {
        if (numbers[previous] == numbers[index])
        {
            alreadySeen = true;
            break;
        }
    }
    if (!alreadySeen)
    {
        Console.WriteLine(numbers[index]);
    }
}
```

Il ciclo interno visita soltanto le posizioni precedenti: il primo esemplare di ogni valore viene sempre stampato.

</details>

## 34. Il secondo valore più grande

**Obiettivo:** ricerche successive e valori distinti.

Trova il secondo valore più grande **distinto** in `{ 8, 3, 8, 5 }`, senza ordinare. Prima trova il massimo; poi trova il maggiore tra i valori più piccoli del massimo. Se non ci sono due valori distinti, stampa un messaggio.

**Verifica:** Risultato 5; `{ -1, -4, -2 }` → -2; `{ 7, 7 }`, `{ 7 }` e array vuoto → nessun secondo valore.

<details>
<summary>Mostra la soluzione</summary>

```csharp
int[] numbers = { 8, 3, 8, 5 };
if (numbers.Length == 0)
{
    Console.WriteLine("Nessun secondo valore");
}
else
{
    int maximum = numbers[0];
    foreach (int number in numbers)
    {
        if (number > maximum)
        {
            maximum = number;
        }
    }

    int secondMaximum = 0;
    bool found = false;
    foreach (int number in numbers)
    {
        if (number < maximum && (!found || number > secondMaximum))
        {
            secondMaximum = number;
            found = true;
        }
    }
    if (found)
    {
        Console.WriteLine(secondMaximum);
    }
    else
    {
        Console.WriteLine("Nessun secondo valore");
    }
}
```

Il booleano `found` distingue un dato trovato da un valore iniziale provvisorio, anche quando tutti i numeri sono negativi.

</details>

## 35. Spostare tutto di una posizione

**Obiettivo:** conservare un dato prima di sovrascriverlo.

Ruota `{ 1, 2, 3, 4 }` di una posizione verso destra nello stesso array. L'ultimo elemento diventa il primo. Salva prima l'ultimo valore, poi sposta gli altri partendo da destra.

**Verifica:** Risultato: 4, 1, 2, 3; un array di un elemento resta uguale; un array vuoto resta vuoto.

<details>
<summary>Mostra la soluzione</summary>

```csharp
int[] numbers = { 1, 2, 3, 4 };
if (numbers.Length > 1)
{
    int last = numbers[numbers.Length - 1];
    for (int index = numbers.Length - 1; index > 0; index--)
    {
        numbers[index] = numbers[index - 1];
    }
    numbers[0] = last;
}
Console.WriteLine(string.Join(", ", numbers));
```

Partire da destra evita di sovrascrivere un valore che ti servirà al giro successivo.

</details>

## 36. L'array è già ordinato?

**Obiettivo:** confrontare elementi vicini.

Controlla se un array è in ordine crescente non decrescente: ogni elemento deve essere minore o uguale al successivo. Sono ammessi valori uguali. Considera ordinati anche gli array vuoti e quelli di un solo elemento.

**Verifica:** `{ 1, 2, 2, 5 }` → true; `{ 1, 4, 3 }` → false.

<details>
<summary>Mostra la soluzione</summary>

```csharp
int[] numbers = { 1, 2, 2, 5 };
bool isSorted = true;
for (int index = 1; index < numbers.Length; index++)
{
    if (numbers[index - 1] > numbers[index])
    {
        isSorted = false;
        break;
    }
}
Console.WriteLine(isSorted);
```

Una sola coppia fuori ordine basta per dare una risposta negativa.

</details>

## 37. Ordinare scegliendo il minimo

**Obiettivo:** selection sort e scambio di due elementi.

Ordina `{ 5, 2, 4, 1, 2 }` in senso crescente, senza `Array.Sort` o LINQ. Per ogni posizione cerca il minimo nella parte ancora da ordinare e scambialo con l'elemento di quella posizione. Usa una variabile temporanea per lo scambio.

**Verifica:** Risultato: 1, 2, 2, 4, 5. Prova anche numeri negativi, un array già ordinato e un array vuoto.

<details>
<summary>Mostra la soluzione</summary>

```csharp
int[] numbers = { 5, 2, 4, 1, 2 };
for (int position = 0; position < numbers.Length - 1; position++)
{
    int minimumIndex = position;
    for (int index = position + 1; index < numbers.Length; index++)
    {
        if (numbers[index] < numbers[minimumIndex])
        {
            minimumIndex = index;
        }
    }

    int temporary = numbers[position];
    numbers[position] = numbers[minimumIndex];
    numbers[minimumIndex] = temporary;
}
Console.WriteLine(string.Join(", ", numbers));
```

Dopo ogni giro esterno, un'altra posizione contiene il suo valore definitivo. Prova a seguire su carta gli indici e gli scambi.

</details>

## 38. Ordinare confrontando i vicini

**Obiettivo:** bubble sort e passaggi ripetuti.

Ordina `{ 4, 1, 3, 2 }` confrontando coppie adiacenti: se il valore a sinistra è maggiore, scambia i due. Ripeti i passaggi; dopo ogni passaggio l'elemento più grande della parte visitata è in fondo. Non usare ordinamenti pronti.

**Verifica:** Risultato: 1, 2, 3, 4. Anche duplicati, array vuoti e array di un solo elemento devono funzionare.

<details>
<summary>Mostra la soluzione</summary>

```csharp
int[] numbers = { 4, 1, 3, 2 };
for (int end = numbers.Length - 1; end > 0; end--)
{
    bool swapped = false;
    for (int index = 0; index < end; index++)
    {
        if (numbers[index] > numbers[index + 1])
        {
            int temporary = numbers[index];
            numbers[index] = numbers[index + 1];
            numbers[index + 1] = temporary;
            swapped = true;
        }
    }
    if (!swapped)
    {
        break;
    }
}
Console.WriteLine(string.Join(", ", numbers));
```

Se un intero passaggio non richiede scambi, l'array è già ordinato e puoi terminare.

</details>

## 39. Fondere due array già ordinati

**Obiettivo:** gestire due indici indipendenti.

Unisci `{ 1, 3, 5 }` e `{ 2, 3, 4, 6 }`, già ordinati, in un nuovo array ordinato. Confronta i due elementi correnti e copia il minore, avanzando solo il suo indice. Quando uno dei due array finisce, copia gli elementi rimasti dell'altro. Conserva i duplicati e non riordinare il risultato.

**Verifica:** Risultato: 1, 2, 3, 3, 4, 5, 6. Deve funzionare anche se uno o entrambi gli array sono vuoti.

<details>
<summary>Mostra la soluzione</summary>

```csharp
int[] first = { 1, 3, 5 };
int[] second = { 2, 3, 4, 6 };
int[] merged = new int[first.Length + second.Length];
int firstIndex = 0;
int secondIndex = 0;
int resultIndex = 0;

while (firstIndex < first.Length && secondIndex < second.Length)
{
    if (first[firstIndex] <= second[secondIndex])
    {
        merged[resultIndex] = first[firstIndex];
        firstIndex++;
    }
    else
    {
        merged[resultIndex] = second[secondIndex];
        secondIndex++;
    }
    resultIndex++;
}
while (firstIndex < first.Length)
{
    merged[resultIndex] = first[firstIndex];
    firstIndex++;
    resultIndex++;
}
while (secondIndex < second.Length)
{
    merged[resultIndex] = second[secondIndex];
    secondIndex++;
    resultIndex++;
}
Console.WriteLine(string.Join(", ", merged));
```

Ogni indice indica il prossimo elemento da usare nel proprio array. Le code residue sono già ordinate.

</details>

## 40. Un piccolo istogramma

**Obiettivo:** array di contatori e visualizzazione con cicli.

Parti dai voti `{ 0, 2, 2, 5, 3, 5, 5 }`, tutti tra 0 e 5. Usa un array di sei contatori per contare ogni voto. Poi stampa sei righe: il voto e un asterisco per ogni occorrenza. Non usare LINQ.

**Verifica:** La riga 0 ha un asterisco, la 2 ne ha due, la 3 uno e la 5 tre; 1 e 4 non ne hanno. Con dati vuoti tutte le righe sono senza asterischi.

<details>
<summary>Mostra la soluzione</summary>

```csharp
int[] grades = { 0, 2, 2, 5, 3, 5, 5 };
int[] counts = new int[6];
foreach (int grade in grades)
{
    counts[grade]++;
}
for (int grade = 0; grade < counts.Length; grade++)
{
    Console.Write($"{grade}: ");
    for (int star = 0; star < counts[grade]; star++)
    {
        Console.Write("*");
    }
    Console.WriteLine();
}
```

Il valore del voto è anche l'indice del suo contatore. Questa scelta funziona perché la consegna limita i voti a 0–5.

</details>
