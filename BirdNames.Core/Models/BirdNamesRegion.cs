using BirdNames.Core.Helpers;
using BirdNames.Dal;

namespace BirdNames.Core.Models;

[BsonCollection(nameof(BirdNamesRegion))]
public class BirdNamesRegion: ModelVersionBase
{
  public string Code { get; set; }
  public HashSet<string> Subregions { get; set; } = new();

  public BirdNamesRegion(string version, int year, string code) : base(version, year)
  {
    Code = code;
  }

  public string? Name { get; set; }
  public string? Notes { get; set; }


  public static List<BirdNamesRegion> FromSpecies(BirdNamesSpecies birdNamesSpecies)
  {
    var addCoastal = false;
    var nonBreedingSubRegions = new List<string>();
    var regions = new List<BirdNamesRegion>();
    var regionCodesRaw=birdNamesSpecies.BreedingRegions.Split(',').Select(x=>x.Trim()).ToHashSet();
    var regionCodes = new HashSet<string>();
    foreach (var regionCode in regionCodesRaw)
    {
      if (regionCode.Contains(' '))
      {
        var items = regionCode.Split(' ');
        foreach(var item in items)
          if (BirdDataHelper.RegionsLookup.ContainsKey(item))
            regionCodes.Add(item);
        nonBreedingSubRegions.Add(regionCode);
      }
      else
        regionCodes.Add(regionCode);
    }

    if (!string.IsNullOrEmpty(birdNamesSpecies.NonBreedingRegions))
    {
      var nbrRegionCodes = birdNamesSpecies.NonBreedingRegions.Split(',').Select(x => x.Trim());
      foreach (var nbrRegionCode in nbrRegionCodes)
      {
        if (!BirdDataHelper.RegionsLookup.ContainsKey(nbrRegionCode))
          nonBreedingSubRegions.Add(nbrRegionCode);
        else
          regionCodes.Add(nbrRegionCode);
      }
    }
    foreach (var regionCode in regionCodes)
    {
      if (!BirdDataHelper.RegionsLookup.ContainsKey(regionCode))
      {
        if (regionCode == "adjacent coast")
        {
          addCoastal = true;
          continue;
        }
        else
        {
          nonBreedingSubRegions.Add(regionCode);
          Console.WriteLine($"Region code {regionCode} not found in RegionsLookup.  Species:{birdNamesSpecies.Name} br:{birdNamesSpecies.BreedingRegions}, nbr:{birdNamesSpecies.NonBreedingRegions}");
          continue;
        }
      }

      var (name, notes) = BirdDataHelper.RegionsLookup[regionCode];

      var region = new BirdNamesRegion(birdNamesSpecies.Version, birdNamesSpecies.Year, regionCode);
      if(!string.IsNullOrEmpty(birdNamesSpecies.BreedingSubRegions))
        region.Subregions.Add(birdNamesSpecies.BreedingSubRegions);
      if(!string.IsNullOrEmpty(birdNamesSpecies.NonBreedingSubRegions))
        region.Subregions.Add(birdNamesSpecies.NonBreedingSubRegions);
      if(addCoastal)
        region.Subregions.Add("adjacent coast");
      if (nonBreedingSubRegions.Any())
        region.Subregions.UnionWith(nonBreedingSubRegions);

      region.Name = name;
      region.Notes = notes;

      regions.Add(region);
    }

    if (addCoastal)
      foreach (var region in regions)
        region.Subregions.Add("adjacent coast");

    return regions;
  }

  public static void Union(BirdNamesRegion birdNamesRegion, BirdNamesRegion other)
  {
    birdNamesRegion.Subregions.UnionWith(other.Subregions);
  }
}