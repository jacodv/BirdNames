using Microsoft.VisualBasic.FileIO;

namespace BirdNames.Core.Helpers;
public static class FileHelpers
{
  public static int ProcessSimpleCsvLines(
    Stream inputStream,
    Action<IList<string>> rowProcessed,
    CancellationToken cancellationToken=default,
    bool closeStream=true,
    bool hasTextQualifier=false,
    bool ignoreFirstLine=false,
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
}
