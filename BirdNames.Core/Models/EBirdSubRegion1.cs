using BirdNames.Dal;

namespace BirdNames.Core.Models;

[BsonCollection(nameof(EBirdSubRegion1))]
public class EBirdSubRegion1 : CodeAndNameBase
{
  public EBirdSubRegion1(string code, string name, string country):base(code, name)
  {
    Country = country;
  }
  public string Country { get; set; }
  public List<string> SpeciesCodes { get; set; } = new();
}