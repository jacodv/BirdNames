using BirdNames.Dal;

namespace BirdNames.Core.Models;

[BsonCollection(nameof(EBirdMajorRegion))]
public class EBirdMajorRegion : CodeAndNameBase
{
  public EBirdMajorRegion(string code, string name)
    :base(code, name)
  {
    
  }
  public List<string> Countries { get; set; } = new();
}