using System.Text;
using System.Text.Json.Serialization;
using BirdNames.Dal;
using MongoDB.Bson.Serialization.Attributes;

namespace BirdNames.Core.Models;

[BsonCollection(nameof(EBirdSpecies))]
public class EBirdSpecies: CodeAndNameBase
{
  public EBirdSpecies(string code, string name, string sciName, string category, double taxonOrder, string order, string familyCode, string familyComName, string familySciName) : base(code, name)
  {
    SciName = sciName;
    Category = category;
    TaxonOrder = taxonOrder;
    Order = order;
    FamilyCode = familyCode;
    FamilyComName = familyComName;
    FamilySciName = familySciName;
    Name = name;
    Code = code;
  }

  [JsonPropertyName("sciName")]
  public string SciName { get; set; }
  [JsonPropertyName("category")]
  public string Category { get; set; }
  [JsonPropertyName("taxonOrder")]
  public double TaxonOrder { get; set; }
  [JsonPropertyName("bandingCodes")]
  public List<string> BandingCodes { get; set; } = new();
  [JsonPropertyName("comNameCodes")]
  public List<string> ComNameCodes { get; set; } = new();
  [JsonPropertyName("sciNameCodes")]
  public List<string> SciNameCodes { get; set; } = new();
  [JsonPropertyName("order")]
  public string Order { get; set; }
  [JsonPropertyName("familyCode")]
  public string FamilyCode { get; set; }
  [JsonPropertyName("familyComName")]
  public string FamilyComName { get; set; }
  [JsonPropertyName("familySciName")]
  public string FamilySciName { get; set; }

  #region REquired For Deserializaion from EBird Api
  private string _comName = string.Empty;
  [BsonIgnore]
  [JsonPropertyName("comName")]
  public string ComName
  {
    get => _comName;
    set
    {
      _comName = value;
      Name = value;
    }
  }

  private string _speciesCode = string.Empty;
  [BsonIgnore]
  [JsonPropertyName("speciesCode")]
  public string SpeciesCode
  {
    get => _speciesCode;
    set
    {
      _speciesCode = value;
      Code = value;
    }
  }
  #endregion

  #region Overrides of CodeAndNameBase

  public override string ToString()
  {
    return $"{Code}|{Name}{SciName}";
  }

  #endregion

  public async Task GetFileContent(StreamWriter writer, string lineTabs, bool includeGenus)
  {
    await writer.WriteLineAsync($"{lineTabs}{Name}");
    if(includeGenus)
      await writer.WriteLineAsync($"\t{lineTabs}{{{SciName}}}");
    else
      await writer.WriteLineAsync($"\t{lineTabs}{{{GetSciNameOnly()}}}");
  }
  public string GetSciNameOnly()
  {
    var values= SciName.Split(' ');
    var nameOnly = SciName.Replace(values[0], string.Empty).Trim();
    return char.ToUpper(nameOnly[0])+nameOnly[1..];
  }
}