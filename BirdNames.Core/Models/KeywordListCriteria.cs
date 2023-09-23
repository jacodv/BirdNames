using System.Text;
using BirdNames.Core.Enums;

namespace BirdNames.Core.Models;

public class KeywordListCriteria
{
  private string? _country;
  private string? _subRegion1;

  public KeywordListCriteria(string majorRegion)
  {
    MajorRegion = majorRegion.ToLower();
    Depth = KeywordDepth.Order | KeywordDepth.Family | KeywordDepth.Genus | KeywordDepth.Species;
    HeaderSynonyms = HeaderSynonym.Birds | HeaderSynonym.Aves | HeaderSynonym.Chordata | HeaderSynonym.Animalia | HeaderSynonym.Eumaniraptora | HeaderSynonym.Tetrapoda | HeaderSynonym.Avilalae | HeaderSynonym.Neornithes;
  }

  public string MajorRegion { get; set; }
  public string? Country
  {
    get => _country;
    set => _country = value?.ToUpper();
  }
  public string? SubRegion1
  {
    get => _subRegion1;
    set => _subRegion1 = value?.ToUpper();
  }
  public bool OrderByName { get; set; }
  public KeywordDepth Depth { get; set; }
  public HeaderSynonym HeaderSynonyms { get; set; }

  public bool IsValid()
  {
    return !string.IsNullOrWhiteSpace(MajorRegion) ||
           !string.IsNullOrEmpty(Country) ||
           !string.IsNullOrEmpty(SubRegion1);
  }
  public override string ToString()
  {
    var sb = new StringBuilder(MajorRegion);
    if (!string.IsNullOrEmpty(Country))
    {
      if (sb.Length > 0)
        sb.Append($"|{Country}");
      else
        sb.Append($"{Country}");
    }

    if (!string.IsNullOrEmpty(SubRegion1))
    {
      if (sb.Length > 0)
        sb.Append($"|{SubRegion1}");
      else
        sb.Append($"{SubRegion1}");
    }
    return sb.ToString();
  }
}