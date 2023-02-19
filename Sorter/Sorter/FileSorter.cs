namespace Sorter;

public class FileSorter
{
  private const string WorkDir = ".\\WorkDir";
  private const int ChunkSize = 10_000_000;

  public static async Task SortFileAsync(string inputFilePath, string outputFilePath)
  {
    var files = await SplitFileAsync(inputFilePath);
    var sortedFiles = await SortFilesAsync(files);
    await MergeFilesAsync(sortedFiles, outputFilePath);
  }

  private static DataRow ToDataRow(string rawValue)
  {
    var values = rawValue.IndexOf('.');

    return new DataRow
    {
      Number = int.Parse(rawValue.Substring(0, values)),
      Text = rawValue.Substring(values + 2)
    };
  }

  private static async Task<string> SortFileAsync(string fileNameToSort)
  {
    var fileToSortPath = Path.Combine(WorkDir, fileNameToSort);
    var lines = await File.ReadAllLinesAsync(fileToSortPath);

    var dataRows = lines.Where(x => !string.IsNullOrWhiteSpace(x)).Select(ToDataRow).ToArray();

    Array.Sort(dataRows);

    var fileName = $"{Path.GetFileNameWithoutExtension(fileNameToSort)}.sorted";
    await File.WriteAllLinesAsync(Path.Combine(WorkDir, fileName), dataRows.Select(x => x.ToString()));
    File.Delete(fileToSortPath);
    return fileName;
  }

  private static async Task<List<string>> SortFilesAsync(List<string> fileNames)
  {
    var sortedFileNames = new List<string>();
    foreach (var fileName in fileNames)
    {
      var sortedFileName = await SortFileAsync(fileName);

      sortedFileNames.Add(sortedFileName);
    }

    return sortedFileNames;
  }

  private static async Task MergeFilesAsync(List<string> fileNames, string outputFilePath)
  {
    await using var output = File.OpenWrite(outputFilePath);
    await using var outputWriter = new StreamWriter(output);

    var readers = new List<DataRowReader>();

    foreach (var fileName in fileNames)
    {
      var reader = new StreamReader(Path.Combine(WorkDir, fileName));
      var line = await reader.ReadLineAsync();

      readers.Add(
        new DataRowReader
        {
          Reader = reader,
          DataRow = ToDataRow(line),
          FileName = fileName
        });
    }

    do
    {
      readers.Sort((x1, x2) => x1.DataRow.CompareTo(x2.DataRow));

      var minValue = readers.First();
      var value = minValue.DataRow.ToString();

      await outputWriter.WriteLineAsync(value);

      if (!minValue.Reader.EndOfStream)
      {
        var line = await minValue.Reader.ReadLineAsync();
        minValue.DataRow = ToDataRow(line);
        continue;
      }

      minValue.Reader.Dispose();
      readers.Remove(minValue);
      File.Delete(Path.Combine(WorkDir, minValue.FileName));
    }
    while (readers.Count > 0);
  }

  private static async Task<List<string>> SplitFileAsync(string inputFilePath)
  {
    var fileNames = new List<string>();

    await using var file = File.OpenRead(inputFilePath);

    var bytes = new List<byte>();
    var readByteCount = 0L;
    var currentChunk = 0;

    var done = false;

    do
    {
      var value = (byte)file.ReadByte();
      bytes.Add(value);
      readByteCount++;

      if (readByteCount == file.Length)
      {
        done = true;
      }
      else if (bytes.Count < ChunkSize || value != '\n')
      {
        continue;
      }

      var fileName = $"chunk{currentChunk}.tosort";
      fileNames.Add(fileName);

      if (!Directory.Exists(WorkDir))
      {
        Directory.CreateDirectory(WorkDir);
      }

      await File.WriteAllBytesAsync(Path.Combine(WorkDir, fileName), bytes.ToArray());

      bytes.Clear();
      currentChunk++;
    }
    while (!done);

    return fileNames;
  }
}
