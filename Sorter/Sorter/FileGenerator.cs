using System.Text;
using Bogus;

namespace Sorter;

public static class FileGenerator
{
  private const int SeedSize = 100_000;
  private const int ChunkSize = 100_000;

  private static readonly List<Person> Seed;

  static FileGenerator()
  {
    Seed = new Faker<Person>()
      .RuleFor(x => x.City, x => x.Address.City())
      .RuleFor(x => x.LastName, x => x.Name.LastName())
      .RuleFor(x => x.FirstName, x => x.Name.FirstName())
      .RuleFor(x => x.Email, x => x.Internet.Email())
      .RuleFor(x => x.JobArea, x => x.Name.JobArea())
      .RuleFor(x => x.ProductName, x => x.Commerce.ProductName())
      .Generate(SeedSize);
  }

  public static async Task GenerateAsync(int numberOfLines, string filePath = "fileToSort.txt")
  {
    var runs = numberOfLines / ChunkSize;

    for (var i = 0; i < runs; i++)
    {
      await GenerateInternalAsync(ChunkSize, filePath);
    }
  }

  private static async Task GenerateInternalAsync(int numberOfLines, string filePath)
  {
    var numberToGenerate = (int)(numberOfLines * 0.9);

    var random = new Random(DateTime.Now.Millisecond);
    var text = new StringBuilder();
    var doubleText = new StringBuilder();
    var doubleLineCount = numberOfLines - numberToGenerate;

    for (var i = 0; i < numberToGenerate; i++)
    {
      var indexes = new[]
      {
        random.Next(1, SeedSize),
        random.Next(1, SeedSize),
        random.Next(1, SeedSize),
        random.Next(1, SeedSize),
        random.Next(1, SeedSize),
        random.Next(1, SeedSize)
      };

      var line =
        $"{Seed[indexes[0]].FirstName} {Seed[indexes[1]].LastName} {Seed[indexes[2]].Email} {Seed[indexes[3]].City} " +
        $"{Seed[indexes[4]].JobArea} {Seed[indexes[5]].ProductName} {Guid.NewGuid()}";
      text.Append($"{indexes[3]}. {line}");
      text.AppendLine();

      if (i <= numberToGenerate - doubleLineCount)
      {
        continue;
      }

      doubleText.Append($"{indexes[2]}. {line}");
      doubleText.AppendLine();
    }

    await using var sw = new StreamWriter(filePath, true, Encoding.UTF8, 65536);

    await sw.WriteLineAsync(text);
    await sw.WriteLineAsync(doubleText);
  }
}
