using BirdNames.Dal;

namespace BirdNames.Core.Models;

[BsonCollection(nameof(BirdNamesOrder))]
public class BirdNamesOrder : ModelVersionBase
{
  public string Code { get; set; }
  public string Latin { get; set; }
  public string? Note { get; set; }

  public BirdNamesOrder(string version, int year, string code, string latin, string? note = null)
    : base(version, year)
  {
    Latin = latin;
    // Some orders have no code, so use the latin name instead
    Code = string.IsNullOrEmpty(code)?latin:code;
    Note = note;
  }

  #region Overrides of Object

  public override string ToString()
  {
    return $"{Code}|{Latin}";
  }

  #endregion
}