using BirdNames.Core.Enums;
using BirdNames.Core.Services;
using Microsoft.Extensions.Logging;

namespace BirdNames.Core.Models;

public class KeywordListOrder : KeywordListItem<KeywordListFamily>
{
  public KeywordListOrder(string code, string name) : base(code, name)
  {
  }

  public void BuildFamilies(KeywordListCriteria criteria, IList<EBirdSpecies> species, ILogger logger)
  {
    if (!criteria.Depth.HasFlag(KeywordDepth.Family))
    {
      logger.LogInformation($"\tIgnoring families: {this}");
      var ignoredFamily = new KeywordListFamily(EBirdService.Ignored, EBirdService.Ignored, EBirdService.Ignored) { Ignore = true };

      ignoredFamily.BuildGenera(criteria, species, logger);
      Items.Add(ignoredFamily);
      return;
    }

    var families = species
      .GroupBy(x => new { x.FamilyCode, x.FamilyComName, x.FamilySciName })
      .ToList();

    logger.LogInformation($"\tFamilies:{families.Count} for Order:{this} and Species:{species.Count}");
    foreach (var family in families)
    {
      if (family.Key?.FamilyCode == null && family.Key?.FamilyComName == null && family.Key?.FamilySciName == null)
      {
        logger.LogInformation($"\t\tSkipping empty object");
        continue;
      }

      var familyItem = new KeywordListFamily(family.Key.FamilyCode, family.Key.FamilyComName, family.Key.FamilySciName);
      var genus = familyItem.Latin.Split(' ')[0];
      logger.LogInformation($"\t\tGenus:{genus} - {family.Key.FamilySciName}");
      familyItem.BuildGenera(criteria, family.ToList(), logger);
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

    await _writeLineAsync(writer, $"{lineTabs}{Code}");
    lineTabs += "\t";
    var synonym = Code.ToLower() == "struthioniformes" || Code.ToLower() == "tinamiformes" ?
      "Palaeognathae" :
      "Neognathae";
    await _writeLineAsync(writer, $"{lineTabs}{{{synonym}}}");
    foreach (var keywordListFamily in Items)
      await keywordListFamily.GetFileContent(writer, lineTabs);
  }
}