namespace BirdNames.Core.Models;

public abstract class KeywordListItem<T> : CodeAndName
{
  protected KeywordListItem(string code, string name) : base(code, name)
  {
  }

  public List<T> Items { get; set; } = new();
  public bool Ignore { get; set; }

  public abstract Task GetFileContent(StreamWriter writer, string lineTabs);

  protected static async Task _writeLineAsync(StreamWriter writer, string line)
  {
    await writer.WriteLineAsync(line.Replace(",",string.Empty));
  }
}