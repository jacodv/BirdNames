using BirdNames.Dal;

namespace BirdNames.Core.Models;

[BsonCollection(nameof(BirdNamesSpecies))]
public class BirdNamesSpecies: BirdNamesSubSpecies
{
  public string Name { get; set; }
  public string BreedingRegions { get; set; }
  public string? NonBreedingRegions { get; set; }
  public string GenusLatin { get; set; }
  public List<BirdNamesSubSpecies> Subspecies { get; set; } = new();

  public BirdNamesSpecies(string version, int year, string name, string latin, string? authority, 
    string breedingRegions, string? nonBreedingRegions, 
    string? breedingSubRegions, string? nonBreedingSubRegions, 
    string genusLatin, bool extinct = false)
    : base(version, year, latin, authority, breedingSubRegions, nonBreedingSubRegions, extinct)
  {
    Name = name;
    BreedingRegions = breedingRegions;
    NonBreedingRegions = nonBreedingRegions;
    GenusLatin = genusLatin;
  }

  public void AddSubspecies(BirdNamesSubSpecies subspecies)
  {
    Subspecies.Add(subspecies);
  }

  #region Overrides of Object

  public override string ToString()
  {
    return $"{Name}|{Latin}|{GenusLatin}|";
  }

  #endregion
}