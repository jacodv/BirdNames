using BirdNames.Dal;

namespace BirdNames.Core.Models;

[BsonCollection(nameof(EBirdSubRegion2))]
public class EBirdSubRegion2 : CodeAndNameBase
{
  public EBirdSubRegion2(string code, string name, string subRegion1, string country)
    :base(code, name)
  {
    SubRegion1 = subRegion1;
    Country = country;
  }
  public string SubRegion1 { get; set; }
  public string Country { get; set; }
}