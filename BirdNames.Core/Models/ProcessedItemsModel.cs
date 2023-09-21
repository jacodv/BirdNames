namespace BirdNames.Core.Models;

public class ProcessedItemsModel
{
  public string Version { get; }

  public ProcessedItemsModel(string version)
  {
    Version = version;
  }
  public List<BirdNamesOrder> Orders { get; set; } = new();
  public List<BirdNamesFamily> Families { get; set; } = new();
  public List<BirdNamesGenus> Genera { get; set; } = new();
  public List<BirdNamesSpecies> Species { get; set; } = new();
  public List<BirdNamesRegion> Regions { get; set; } = new();
}