﻿using BirdNames.Core.Enums;
using BirdNames.Core.Services;

namespace BirdNames.Core.Models;

public class KeywordListFamily : KeywordListItem<KeywordListGenus>
{
  public string Latin { get; }

  public KeywordListFamily(string code, string name, string latin) : base(code, name)
  {
    Latin = latin;
  }

  public void BuildGenera(KeywordListCriteria criteria, IList<EBirdSpecies> species)
  {
    if (!criteria.Depth.HasFlag(KeywordDepth.Genus))
    {
      Console.WriteLine($"\t\tIgnoring genera: {this}");
      var ignoredGenus = new KeywordListGenus(EBirdService.Ignored, EBirdService.Ignored) { Ignore = true };

      ignoredGenus.BuildSpecies(criteria, species);
      Items.Add(ignoredGenus);
      return;
    }

    var genera = species
      .GroupBy(x => x.SciName.Split(' ')[0])
      .ToList();

    Console.WriteLine($"\t\tGenera:{genera.Count} for Family:{this.Latin} and Species:{species.Count}");
    foreach (var genus in genera)
    {
      var genusItem = new KeywordListGenus(genus.Key, genus.Key);
      genusItem.BuildSpecies(criteria, species.Where(x => x.SciName.StartsWith(genus.Key)).ToList());
      Items.Add(genusItem);
    }
  }
  public override async Task GetFileContent(StreamWriter writer, string lineTabs)
  {
    if (Ignore)
    {
      foreach (var keywordListGenus in Items)
        await keywordListGenus.GetFileContent(writer, lineTabs);
      return;
    }

    await writer.WriteLineAsync($"{lineTabs}{Name}");
    lineTabs += "\t";
    await writer.WriteLineAsync($"{lineTabs}{{{Latin}}}");
    foreach (var keywordListGenus in Items)
      await keywordListGenus.GetFileContent(writer, lineTabs);
  }
}