using BirdNames.Core.Interfaces;
using BirdNames.Core.Models;
using BirdNames.Core.Services;
using Microsoft.Extensions.DependencyInjection;
namespace BirdNames.Core.Xml;

public static class BirdNamesFx
{
  public static async Task ProcessXml(string path, IServiceProvider serviceProvider)
  {
    await using var fileStream = File.OpenRead(path);
    await ProcessXml(fileStream, serviceProvider);
  }

  public static async Task ProcessXml(Stream xmlSource, IServiceProvider serviceProvider)
  {
    await using var xmlProcessor = new BirdNamesXmlProcessor(xmlSource);
    await xmlProcessor.ProcessXml(serviceProvider);
  }

  public static async Task ProcessCountriesCsv(string path, IServiceProvider serviceProvider)
  {
    await using var fileStream = File.OpenRead(path);
    await ProcessCountriesCsv(fileStream, serviceProvider);
  }

  public static Task ProcessCountriesCsv(Stream csvSource, IServiceProvider serviceProvider)
  {
    var countryService = serviceProvider.GetService<ICountryService>();
    return countryService!.PersistCountries(csvSource);
  }

  public static Task ProcessSubRegions1(bool refresh, IServiceProvider serviceProvider)
  {
    var eBirdService = serviceProvider.GetService<IEBirdService>();
    return eBirdService!.ProcessSubRegion1(refresh);
  }
  public static Task ProcessSubRegions2(bool refresh, IServiceProvider serviceProvider)
  {
    var eBirdService = serviceProvider.GetService<IEBirdService>();
    return eBirdService!.ProcessSubRegion2(refresh);
  }
  public static Task ProcessMajorRegions(bool refresh, IServiceProvider serviceProvider)
  {
    var eBirdService = serviceProvider.GetService<IEBirdService>();
    return eBirdService!.ProcessMajorRegions(refresh);
  }
  public static Task ProcessSubRegions1Species(bool refresh, IServiceProvider serviceProvider)
  {
    var eBirdService = serviceProvider.GetService<IEBirdService>();
    return eBirdService!.ProcessSubRegion1Species(refresh);
  }
  public static Task ProcessUniqueSpeciesInfo(bool refresh, IServiceProvider serviceProvider)
  {
    var eBirdService = serviceProvider.GetService<IEBirdService>();
    return eBirdService!.ProcessSubRegion1SpeciesInfo(refresh);
  }
  public static Task VerifyAllUniqueSpecies(bool refresh, IServiceProvider serviceProvider)
  {
    var eBirdService = serviceProvider.GetService<IEBirdService>();
    return eBirdService!.VerifyAllUniqueSpecies(refresh);
  }

  public static Task<MemoryStream> DownloadKeywords(IServiceProvider serviceProvider, KeywordListCriteria criteria)
  {
    var eBirdService = serviceProvider.GetService<IEBirdService>();
    return eBirdService!.DownloadKeywords(criteria, CancellationToken.None);
  }

  public static Task TempWork(IServiceProvider serviceProvider)
  {
    var eBirdService = serviceProvider.GetService<IEBirdService>();
    return eBirdService!.TempWork();
  } 
}