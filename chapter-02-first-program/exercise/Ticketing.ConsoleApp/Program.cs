Console.WriteLine("=== Ticketing App ===");

Console.WriteLine("Inserisci il titolo del ticket:");
var title = Console.ReadLine();

Console.WriteLine("Descrivi il problema:");
var description = Console.ReadLine();

Console.WriteLine();
Console.WriteLine("Ticket inserito:");
Console.WriteLine(title);
Console.WriteLine(description);

Console.WriteLine();
Console.WriteLine("Premi un tasto per terminare.");
Console.ReadKey();
