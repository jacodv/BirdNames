namespace BirdNames.Core.Models;

public class BirdNamesSubSpecies: ModelVersionBase
{
  public string Latin { get; set; }
  public string? Authority { get; set; }
  public string? BreedingSubRegions { get; }
  public string? NonBreedingSubRegions { get; }
  public bool Extinct { get; set; }

  public BirdNamesSubSpecies(string version, int year, string latin, string? authority, string? breedingSubRegions,
    string? nonBreedingSubRegions, bool extinct = false)
  : base(version, year)
  {
    Latin = latin;
    Authority = authority;
    BreedingSubRegions = breedingSubRegions;
    NonBreedingSubRegions = nonBreedingSubRegions;
    Extinct = extinct;
  }
}