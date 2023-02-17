using Bogus;

DataRow ToDataRow(string rawValue)
{
  var values = rawValue.Split('.');

  return new DataRow
  {
    Number = int.Parse(values[0]),
    Text = values[1]
  };
}

async Task<string> SortFileAsync(string filePath)
{
  var lines = await File.ReadAllLinesAsync(filePath);

  var dataRows = lines.Select(ToDataRow).ToArray();

  Array.Sort(dataRows);

  var fileName = $"{Path.GetFileNameWithoutExtension(filePath)}.sorted";
  await File.WriteAllLinesAsync(fileName, dataRows.Select(x => x.ToString()));
  File.Delete(filePath);
  return fileName;
}

async Task<List<string>> SortFilesAsync(List<string> fileNames)
{
  var sortedFileNames = new List<string>();
  foreach (var fileName in fileNames)
  {
    var sortedFileName = await SortFileAsync(fileName);

    sortedFileNames.Add(sortedFileName);
  }

  return sortedFileNames;
}

async Task MergeFilesAsync(List<string> fileNames)
{
  var output = File.OpenWrite("result.txt");
  var outputS = new StreamWriter(output);

  var readers = new List<DataRowReader>();

  foreach (var fileName in fileNames)
  {
    var reader = new StreamReader(fileName);
    var line = await reader.ReadLineAsync();
    readers.Add(
      new DataRowReader
      {
        Reader = reader,
        DataRow = ToDataRow(line)
      });
  }

  do
  {
    readers.Sort((x1, x2) => x1.DataRow.CompareTo(x2.DataRow));

    var minValue = readers.First();
    var value = minValue.DataRow.ToString();

    await outputS.WriteLineAsync(value);

    if (!minValue.Reader.EndOfStream)
    {
      var line = await minValue.Reader.ReadLineAsync();
      minValue.DataRow = ToDataRow(line);
      continue;
    }

    minValue.Reader.Dispose();
    readers.Remove(minValue);
  }
  while (readers.Count > 0);
}

async Task<List<string>> SplitFileAsync()
{
  const int chunkSize = 10_000;
  var fileNames = new List<string>();

  await using var file = File.OpenRead("test.txt");

  var bytes = new List<byte>();
  var readByteCount = 0L;
  var currentChunk = 0;

  do
  {
    var value = (byte)file.ReadByte();
    bytes.Add(value);
    readByteCount++;

    if (bytes.Count < chunkSize || value != '\n')
    {
      continue;
    }

    var fileName = $"chunk{currentChunk}.tosort";
    fileNames.Add(fileName);

    await File.WriteAllBytesAsync(fileName, bytes.ToArray());

    bytes.Clear();
    currentChunk++;
  }
  while (readByteCount < file.Length);

  return fileNames;
}

async Task GenerateFileAsync()
{
  var dataRows = new Faker<DataRow>()
    .RuleFor(x => x.Number, x => x.Random.Number(0, 1_000))
    .RuleFor(
      x => x.Text,
      x => $"{x.Name.FirstName()} {x.Name.LastName()} {x.Name.JobArea()} {x.Internet.Email()} {x.Address.City()} {x.UniqueIndex}")
    .Generate(1_000);

  await File.AppendAllLinesAsync("test.txt", dataRows.Select(x => $"{x.Number}. {x.Text}"));
}

Console.WriteLine("Hello, World!");

//await GenerateFileAsync();
var files = await SplitFileAsync();
var sortedFiles = await SortFilesAsync(files);
await MergeFilesAsync(sortedFiles);

Console.ReadKey();
