using BirdNames.Dal;

namespace BirdNames.Core.Models;

[BsonCollection(nameof(BirdNamesFamily))]
public class BirdNamesFamily : ModelVersionBase
{
  public string Latin { get; set; }
  public string Name { get; set; }
  public string? Url { get; set; }
  public string? OrderCode { get; set; }

  public BirdNamesFamily(string version, int year, string latin, string name, string? url, string orderCode)
    : base(version, year)
  {
    Latin = latin;
    Name = name;
    Url = url;
    OrderCode = orderCode;
  }

  #region Overrides of Object

  public override string ToString()
  {
    return $"{Name}|{OrderCode}";
  }

  #endregion
}