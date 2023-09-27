using Microsoft.Extensions.Logging;

namespace BirdNames.Core.Models;

public class KeywordListGenus : KeywordListItem<EBirdSpecies>
{
  public KeywordListGenus(string code, string name) : base(code, name)
  {
  }

  public void BuildSpecies(KeywordListCriteria criteria, IList<EBirdSpecies> species, ILogger logger)
  {
    logger.LogInformation($"\t\t\tSpecies:{species.Count} species for Genus:{this}");
    Items.AddRange(species);
  }

  public override async Task GetFileContent(StreamWriter writer, string lineTabs)
  {
    if (Ignore)
    {
      foreach (var species in Items)
        await species.GetFileContent(writer, lineTabs, true);
      return;
    }

    await _writeLineAsync(writer, $"{lineTabs}{Name}");
    lineTabs += "\t";
    foreach (var species in Items)
      await species.GetFileContent(writer, lineTabs, false);
  }
}