using BirdNames.Dal;

namespace BirdNames.Core.Models;

/// <summary>
/// Region codes and names from "eBird regions and region codes_18Apr2023.csv"
/// </summary>
[BsonCollection(nameof(EBirdCountry))]
public class EBirdCountry : CodeAndNameBase
{
  public EBirdCountry(string code, string name, string continent):base(code, name)
  {
    Continent = continent;
  }
  public string Continent { get; set; }
}