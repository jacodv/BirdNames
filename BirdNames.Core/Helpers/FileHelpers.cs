using Microsoft.VisualBasic.FileIO;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Writers;

namespace BirdNames.Core.Helpers;
public static class FileHelpers
{
  public static int ProcessSimpleCsvLines(
    Stream inputStream,
    Action<IList<string>> rowProcessed,
    CancellationToken cancellationToken = default,
    bool closeStream = true,
    bool hasTextQualifier = false,
    bool ignoreFirstLine = false,
    params string[] delimiters)

  {
    var rowNumber = 0;
    var recipientsProcessed = 0;
    try
    {
      using var reader = new StreamReader(inputStream);
      using var parser = new TextFieldParser(inputStream);

      parser.SetDelimiters(delimiters);
      parser.HasFieldsEnclosedInQuotes = hasTextQualifier;

      while (!parser.EndOfData)
      {
        rowNumber++;
        var lineValues = parser.ReadFields();
        if (lineValues == null)
          continue;

        if (rowNumber == 1 && ignoreFirstLine)
          continue; // Skip the header row

        if (cancellationToken.IsCancellationRequested)
          break;

        rowProcessed(lineValues);
        recipientsProcessed++;
      }

      return recipientsProcessed;
    }
    catch (Exception e)
    {
      throw new InvalidOperationException($"Csv processing failed:[{rowNumber}].  {e.Message}", e);
    }
    finally
    {
      if (closeStream)
        inputStream?.Close();
    }
  }

  public static byte[] CreateZip(Stream textContent, string downloadFileName)
  {
    using var memoryStream = new MemoryStream();
    using (var archive = ZipArchive.Create())
    {
      archive.AddEntry(downloadFileName, textContent!, textContent.Length, DateTime.Now);
      archive.SaveTo(memoryStream, new WriterOptions(CompressionType.Deflate)
      {
        LeaveStreamOpen = true
      });
    }
    //reset memoryStream to be usable now
    memoryStream.Position = 0;
    return memoryStream.ToArray();
  }

  public static string GetDownloadFileName(string? fileNameFormat = null, bool isZip = false)
  {
    var fileName = string.IsNullOrEmpty(fileNameFormat) ?
      $"BirdNamesKeywords{DateTime.Now:yyy-MM-dd}.txt" :
             string.Format(fileNameFormat, DateTime.Now);
    if (isZip)
    {
      fileName = fileName.EndsWith(".txt") ? 
        fileName.Replace(".txt", ".zip") : 
        $"{fileName}.zip";
    }

    return fileName;
  }
}
