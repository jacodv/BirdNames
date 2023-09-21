using BirdNames.Core.Enums;
using BirdNames.Core.Services;

namespace BirdNames.Core.Models;

public class KeywordListOrder : KeywordListItem<KeywordListFamily>
{
  public KeywordListOrder(string code, string name) : base(code, name)
  {
  }

  public void BuildFamilies(KeywordListCriteria criteria, IList<EBirdSpecies> species)
  {
    if (!criteria.Depth.HasFlag(KeywordDepth.Family))
    {
      Console.WriteLine($"\tIgnoring families: {this}");
      var ignoredFamily = new KeywordListFamily(EBirdService.Ignored, EBirdService.Ignored, EBirdService.Ignored) { Ignore = true };

      ignoredFamily.BuildGenera(criteria, species);
      Items.Add(ignoredFamily);
      return;
    }

    var families = species
      .GroupBy(x => new { x.FamilyCode, x.FamilyComName, x.FamilySciName })
      .ToList();

    Console.WriteLine($"\tFamilies:{families.Count} for Order:{this} and Species:{species.Count}");
    foreach (var family in families)
    {
      var familyItem = new KeywordListFamily(family.Key.FamilyCode, family.Key.FamilyComName, family.Key.FamilySciName);
      var genus = familyItem.Latin.Split(' ')[0];
      Console.WriteLine($"\t\tGenus:{genus} - {family.Key.FamilySciName}");
      familyItem.BuildGenera(criteria, family.ToList());
      Items.Add(familyItem);
    }
  }
  public override async Task GetFileContent(StreamWriter writer, string lineTabs)
  {
    if (Ignore)
    {
      foreach (var keywordListFamily in Items)
        await keywordListFamily.GetFileContent(writer, lineTabs);
      return;
    }

    await writer.WriteLineAsync($"{lineTabs}{Code}");
    lineTabs += "\t";
    var synonym = Code.ToLower() == "struthioniformes" || Code.ToLower() == "tinamiformes" ?
      "Palaeognathae" :
      "Neognathae";
    await writer.WriteLineAsync($"{lineTabs}{{{synonym}}}");
    foreach (var keywordListFamily in Items)
      await keywordListFamily.GetFileContent(writer, lineTabs);
  }
}