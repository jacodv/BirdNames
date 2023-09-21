using System.Text.Json.Serialization;

namespace BirdNames.Core.Models;

public class CodeAndName
{
  public CodeAndName(string code, string name)
  {
    ArgumentException.ThrowIfNullOrEmpty(code);
    ArgumentException.ThrowIfNullOrEmpty(name);

    Code = code;
    Name = name;
  }

  [JsonPropertyName("code")]
  public string Code { get; }
  [JsonPropertyName("name")]
  public string Name { get; }

  public override string ToString()
  {
    return $"{Code}|{Name}";
  }
  public override bool Equals(object? obj)
  {
    if (obj is not CodeAndName other)
      return false;

    return Code == other.Code && Name == other.Name;
  }
  public override int GetHashCode()
  {
    return HashCode.Combine(Code, Name);
  }
}