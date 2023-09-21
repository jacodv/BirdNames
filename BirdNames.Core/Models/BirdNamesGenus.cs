using BirdNames.Dal;

namespace BirdNames.Core.Models;

[BsonCollection(nameof(BirdNamesGenus))]
public class BirdNamesGenus : ModelVersionBase
{
  public string Latin { get; set; }
  public string? Authority { get; set; }
  public string FamilyName { get; set; }

  public BirdNamesGenus(string version, int year, string latin, string? authority, string familyName)
    : base(version, year)
  {
    Latin = latin;
    Authority = authority;
    FamilyName = familyName;
  }

  #region Overrides of Object

  public override string ToString()
  {
    return $"{Latin}|{FamilyName}";
  }

  #endregion
}