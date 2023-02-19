using System.Diagnostics;
using Sorter;

Console.Write("Enter file size(MB): ");
var sizeText = Console.ReadLine();

var numberOfLines = int.Parse(sizeText) * 10_000;

Console.WriteLine($"File generation started for {numberOfLines} lines.");
var stopwatch = Stopwatch.StartNew();
await FileGenerator.GenerateAsync(numberOfLines, "fileToSort.txt");
stopwatch.Stop();
Console.WriteLine("Generation ended, total time:" + stopwatch.Elapsed);

Console.WriteLine("Press enter to sort file...");
Console.ReadKey();

stopwatch.Restart();
await FileSorter.SortFileAsync("fileToSort.txt", "sortedFile.txt");
stopwatch.Stop();
Console.WriteLine("Sorting ended, total time:" + stopwatch.Elapsed);

Console.ReadKey();
