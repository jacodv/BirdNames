using System.Collections.Concurrent;
using System.Text.Json;
using BirdNames.Core.Helpers;
using BirdNames.Core.Interfaces;
using BirdNames.Core.Models;
using BirdNames.Core.Settings;
using BirdNames.Dal.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Options;

// ReSharper disable ConvertToLocalFunction
#pragma warning disable IDE0039

namespace BirdNames.Core.Services;
public class EBirdService : IEBirdService
{
  public const string Ignored = "Ignored";
  private readonly string _baseUrl;

  private readonly IRepository<EBirdMajorRegion> _majorRegionRepository;
  private readonly IRepository<EBirdCountry> _countryRepository;
  private readonly IRepository<EBirdSubRegion1> _subRegion1Repository;
  private readonly IRepository<EBirdSubRegion2> _subRegion2Repository;
  private readonly IRepository<EBirdSpecies> _speciesRepository;
  private readonly IValidator<EBirdSpecies> _speciesValidator;
  private readonly IOptions<BirdNamesCoreSettings> _settings;
  private readonly HttpClient?  _client;

  public EBirdService(
    IRepository<EBirdMajorRegion> majorRegionRepository,
    IRepository<EBirdCountry> countryRepository,
    IRepository<EBirdSubRegion1> subRegion1Repository,
    IRepository<EBirdSubRegion2> subRegion2Repository,
    IRepository<EBirdSpecies> speciesRepository,
    IValidator<EBirdSpecies> speciesValidator,
    IOptions<BirdNamesCoreSettings> settings)
  {
    _majorRegionRepository = majorRegionRepository;
    _countryRepository = countryRepository;
    _subRegion1Repository = subRegion1Repository;
    _subRegion2Repository = subRegion2Repository;
    _speciesRepository = speciesRepository;
    _speciesValidator = speciesValidator;
    _settings = settings;

    try
    {
      _baseUrl = settings.Value.EbirdBaseUrl;
      _client = new HttpClient() { BaseAddress = new Uri(_baseUrl, UriKind.Absolute) };
      _client.DefaultRequestHeaders.Add(settings.Value.EbirdApiKeyName, settings.Value.EbirdApiKeyValue);
    }
    catch
    {
      Console.WriteLine($"ERROR: Failed to construct:{nameof(EBirdService)} from settings");
      _client = null;
    }
  }

  public async Task ProcessSubRegion1(bool refresh, CancellationToken token = default)
  {
    var countries = _countryRepository.AsQueryable().ToList();

    var handleItem = async (EBirdCountry country) =>
    {
      await _getAndPersistSubRegions1(country.Code, token);
    };
    var exist = (EBirdCountry country) =>
             _subRegion1Repository.AsQueryable().Any(x => x.Country == country.Code);

    await countries.ProcessInParallel(refresh, handleItem, exist, token);
  }
  public async Task ProcessSubRegion2(bool refresh, CancellationToken token = default)
  {
    var subRegions1 = _subRegion1Repository.AsQueryable().ToList();
    if (subRegions1.Count == 0)
      return;

    var handleItem = async (EBirdSubRegion1 subRegion1) =>
    {
      await _getAndPersistSubRegions2(subRegion1.Code, subRegion1.Country, token);
    };
    var exist = (EBirdSubRegion1 subRegion1) =>
             _subRegion2Repository.AsQueryable().Any(x => x.SubRegion1 == subRegion1.Code);

    await subRegions1.ProcessInParallel(refresh, handleItem, exist, token);
  }
  public async Task ProcessMajorRegions(bool refresh, CancellationToken token = default)
  {
    var majorRegions = _getMajorRegions();

    await _majorRegionRepository.ClearCollectionAsync();
    Console.WriteLine($"Deleted all major regions");

    var handleItem = async (EBirdMajorRegion majorRegion) =>
    {
      var countries = await _getCodeAndNameResult("country", majorRegion.Code, token);
      majorRegion.Countries = countries.Select(c => c.Code).ToList();

      await _majorRegionRepository.InsertOneAsync(majorRegion);
      Console.WriteLine($"Inserted MajorRegion:{majorRegion.Code}|{majorRegion.Name}.");
    };
    var exist = (EBirdMajorRegion majorRegion) =>
      _majorRegionRepository.AsQueryable().Any(x => x.Code == majorRegion.Code);

    await majorRegions.ProcessInParallel(refresh, handleItem, exist, token);
  }
  public async Task ProcessSubRegion1Species(bool refresh, CancellationToken token = default)
  {
    var subRegions1 = _subRegion1Repository
      .AsQueryable()
      .Select(x => x.Code)
      .ToList();

    var handleItem = async (string subRegion1) =>
    {
      try
      {
        var subRegion1Doc = await _subRegion1Repository.FindOneAsync(x => x.Code == subRegion1);
        if (subRegion1Doc == null)
          throw new Exception($"Failed to find SubRegion1:{subRegion1}");

        var speciesCodes = await _getSpeciesCode(subRegion1, token);
        subRegion1Doc.SpeciesCodes = speciesCodes;

        await _subRegion1Repository.ReplaceOneAsync(subRegion1Doc);
        Console.WriteLine($"Updated species for {subRegion1} and {speciesCodes.Count} species");
      }
      catch (Exception e)
      {
        Console.WriteLine($"Failed to process species for {subRegion1}.  {e.Message}");
      }
    };

    var exist = (string subRegion1) =>
      _subRegion1Repository
        .AsQueryable()
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        .Any(x => x.Code == subRegion1 && x.SpeciesCodes != null && x.SpeciesCodes.Count > 0);

    await subRegions1.ProcessInParallel(refresh, handleItem, exist, token);
  }
  public async Task ProcessSubRegion1SpeciesInfo(bool refresh, CancellationToken token = default)
  {
    var uniqueSpeciesCodes = _getUniqueSpeciesCodes();

    //Process in batches of 10
    const int batchSize = 10;
    var batches = _getBatchOf(uniqueSpeciesCodes.ToList(), batchSize);

    await batches.ProcessInParallel(refresh, _handleSpeciesCodesBatch, _speciesExist, token);
  }
  public async Task VerifyAllUniqueSpecies(bool refresh, CancellationToken token = default)
  {
    var uniqueSpeciesCodes = _getUniqueSpeciesCodes();
    var speciesNotInDb = new ConcurrentBag<string>();

    await Parallel.ForEachAsync(uniqueSpeciesCodes, token, (speciesCode, t) =>
    {
      if (t.IsCancellationRequested)
        return ValueTask.CompletedTask;

      var found = _speciesRepository.AsQueryable().Any(x => x.Code == speciesCode);
      if (!found)
        speciesNotInDb.Add(speciesCode);
      return ValueTask.CompletedTask;
    });

    if (speciesNotInDb.Count == 0)
      return;

    const int batchSize = 10;
    var batches = _getBatchOf(speciesNotInDb.ToList(), batchSize);

    Console.WriteLine($"Found {speciesNotInDb.Count} species not in the database.  Refreshing {refresh}");
    if (!refresh)
    {
      foreach (var batch in batches)
        Console.WriteLine($"Species not in database: {string.Join(',', batch)}");
      return;
    }

    await batches.ProcessInParallel(refresh, _handleSpeciesCodesBatch, _speciesExist, token);
  }
  public async Task<MemoryStream?> DownloadKeywords(KeywordListCriteria criteria, CancellationToken token = default)
  {
    var keywordSource = _buildKeywordSource(criteria);
    if (keywordSource == null)
    {
      Console.WriteLine($"No keywords found for {criteria}");
      return null;
    }

    Console.WriteLine($"Found {keywordSource.Orders.Count} orders");
    Console.WriteLine($"Found {keywordSource.Orders.Sum(x => x.Items.Count)} families");
    Console.WriteLine($"Found {keywordSource.Orders.Sum(x => x.Items.Sum(y => y.Items.Count))} genera");
    Console.WriteLine($"Found {keywordSource.Orders.Sum(x => x.Items.Sum(y => y.Items.Sum(z => z.Items.Count)))} species");

    var memoryStream = new MemoryStream();
    await using var writer = new StreamWriter(memoryStream, leaveOpen:true);
    await keywordSource.GetFileContent(writer);
    return memoryStream;
  }
  public Task TempWork()
  {
    throw new NotImplementedException();

    var speciesCount = _speciesRepository.AsQueryable().Count();
    var speciesSciNames = _speciesRepository
      .AsQueryable()
      .Select(x => x.SciName)
      .Distinct()
      .ToList();

    Console.WriteLine($"Found {speciesCount} species with {speciesSciNames.Count} unique scientific names");

    Dictionary<int, int> spaceCountDictionary = new();
    foreach (var speciesSciName in speciesSciNames)
    {
      var values = speciesSciName.Split(' ');
      var spaceCount = values.Length - 1;
      if (spaceCountDictionary.ContainsKey(spaceCount))
        spaceCountDictionary[spaceCount]++;
      else
        spaceCountDictionary.Add(spaceCount, 1);
    }

    foreach (var (key, value) in spaceCountDictionary)
      Console.WriteLine($"Space count: {key} has {value} species");

    return Task.CompletedTask;
  }
  public bool IsValid()
  {
    // ReSharper disable once ConvertIfStatementToReturnStatement
    if(string.IsNullOrWhiteSpace(_baseUrl) || _client==null || _settings?.Value is not { AdminPasswordSet: true })
      return false;

    return true;
  }
  public async Task<bool> ValidateSettings(BirdNamesCoreSettings settings)
  {
    var client = new HttpClient() { BaseAddress = new Uri(settings.EbirdBaseUrl, UriKind.Absolute) };
    client.DefaultRequestHeaders.Add(settings.EbirdApiKeyName, settings.EbirdApiKeyValue);

    var path = "/v2/ref/region/list/subnational1/ZA";
    var response = await client.GetAsync(path);
    return response.IsSuccessStatusCode;
  }
  

  #region Private
  private async Task _getAndPersistSubRegions1(string countryCode, CancellationToken token)
  {
    var subregions = await _getCodeAndNameResult("subnational1", countryCode, token);

    var sub1 = subregions!
      .Select(s => new EBirdSubRegion1(s.Code, s.Name, countryCode))
      .ToList();

    var result = await _subRegion1Repository.DeleteManyAsync(x => x.Country == countryCode);
    if (result.DeletedCount > 0)
      Console.WriteLine($"Deleted {result.DeletedCount} subregions1 for {countryCode}");

    if (sub1.Count == 0)
      return;

    await _subRegion1Repository.InsertManyAsync(sub1);
    Console.WriteLine($"Inserted {sub1.Count} subregions1 for {countryCode}");
  }
  private async Task _getAndPersistSubRegions2(string subRegion1, string country, CancellationToken token)
  {
    var subregions = await _getCodeAndNameResult("subnational2", subRegion1, token);

    var sub2 = subregions!
      .Select(s => new EBirdSubRegion2(s.Code, s.Name, subRegion1, country))
      .ToList();

    var result = await _subRegion2Repository.DeleteManyAsync(x => x.Country == subRegion1);
    if (result.DeletedCount > 0)
      Console.WriteLine($"Deleted {result.DeletedCount} subregions2 for {country}:{subRegion1}");

    if (sub2.Count == 0)
      return;

    await _subRegion2Repository.InsertManyAsync(sub2);
    Console.WriteLine($"Inserted {sub2.Count} subregions2 for {country}:{subRegion1}");
  }
  private static List<EBirdMajorRegion> _getMajorRegions()
  {
    return new List<EBirdMajorRegion>()
    {
      new("world", "World"),
      new("wh", "Western Hemisphere"),
      new("na", "North America"),
      new("ca", "Central America"),
      new("sa", "South America"),
      new("caribbean", "West Indies"),
      new("lower48", "USA Lower 48"),
      new("aba", "ABA Area"),
      new("abac", "ABA Continental"),
      new("aou", "AOU Area"),
      new("eh", "Eastern Hemisphere"),
      new("af", "Africa"),
      new("saf", "Southern Africa"),
      new("as", "Asia"),
      new("es", "Eurasia"),
      new("eu", "Europe"),
      new("au", "Australasia (ABA)"),
      new("aut", "Australia and Territories"),
      new("wp", "Western Palearctic"),
      new("sp", "South Polar"),
      new("ao", "Atlantic/Arctic Oceans"),
      new("io", "Indian Ocean"),
      new("po", "Pacific Ocean"),
    };
  }
  private async Task<IList<CodeAndName>> _getCodeAndNameResult(string regionType, string filterCode, CancellationToken token)
  {
    var path = $"/v2/ref/region/list/{regionType}/{filterCode}";

    var response = await _client.GetAsync(path, token);
    if (!response.IsSuccessStatusCode)
      throw new Exception($"Error getting {regionType} for {filterCode}");

    Console.WriteLine($"Loading {regionType} for {filterCode}");
    var json = await response.Content.ReadAsStringAsync(token);

    return JsonSerializer.Deserialize<List<CodeAndName>>(json) ??
           new List<CodeAndName>();
  }
  private async Task<List<string>> _getSpeciesCode(string regionCode, CancellationToken token)
  {
    var path = $"/v2/product/spplist/{regionCode}";

    var response = await _client.GetAsync(path, token);
    response.EnsureSuccessStatusCode();

    Console.WriteLine($"Loading species for {regionCode}");
    var json = await response.Content.ReadAsStringAsync(token);

    return JsonSerializer.Deserialize<List<string>>(json) ??
           new List<string>();
  }
  private async Task<IList<EBirdSpecies>> _getSpecies(IEnumerable<string> speciesCodes, CancellationToken token)
  {
    var speciesCodesString = string.Join(',', speciesCodes);
    var path = $"/v2/ref/taxonomy/ebird?species={speciesCodesString}&fmt=json";

    var response = await _client.GetAsync(path, token);
    response.EnsureSuccessStatusCode();

    Console.WriteLine($"Loading species for {speciesCodesString}");
    var json = await response.Content.ReadAsStringAsync(token);

    return JsonSerializer.Deserialize<IList<EBirdSpecies>>(json) ??
           throw new Exception($"Failed to load species for {speciesCodesString}");
  }
  private HashSet<string> _getUniqueSpeciesCodes()
  {
    var uniqueSpeciesCodes = new HashSet<string>();
    var subRegions1 = _subRegion1Repository
      .AsQueryable();

    foreach (var subRegion1 in subRegions1)
      uniqueSpeciesCodes.UnionWith(subRegion1.SpeciesCodes);

    Console.WriteLine($"Found {uniqueSpeciesCodes.Count} unique species codes");
    return uniqueSpeciesCodes;
  }
  private async Task _handleSpeciesCodesBatch(IList<string> speciesCodesBatch)
  {
    try
    {
      var speciesList = await _getSpecies(speciesCodesBatch, CancellationToken.None);
      if (speciesList.Count == 0)
        throw new Exception($"Failed to load species for {string.Join(',', speciesCodesBatch)}");
      foreach (var species in speciesList)
      {
        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
        var validationResult = _speciesValidator.Validate(species);
        if (!validationResult.IsValid)
          throw new Exception($"Failed to validate species {species}.\n\t{string.Join("\n\t", validationResult.Errors)}");
      }

      await _speciesRepository.InsertManyAsync(speciesList);
    }
    catch (Exception e)
    {
      throw new InvalidOperationException($"Failed to process species for {string.Join(',', speciesCodesBatch)}.  {e.Message}");
    }
  }
  private bool _speciesExist(IList<string> speciesCodesBatch)
  {
    return _speciesRepository
      .AsQueryable()
      .Any(x => speciesCodesBatch.Contains(x.Code));
  }
  private static List<IList<string>> _getBatchOf(IEnumerable<string> uniqueSpeciesCodes, int batchSize)
  {
    var batches = new List<IList<string>>();
    foreach (var batch in uniqueSpeciesCodes.Batch(batchSize))
      batches.Add(batch.ToList());
    return batches;
  }
  private IList<string> _getBirdCodesFromCriteria(KeywordListCriteria criteria)
  {
    if (!string.IsNullOrWhiteSpace(criteria.SubRegion1))
      return _subRegion1Repository.AsQueryable()
        .Where(x => x.Code == criteria.SubRegion1)
        .SelectMany(x => x.SpeciesCodes)
        .Distinct()
        .ToList();

    if (!string.IsNullOrEmpty(criteria.Country))
      return _subRegion1Repository.AsQueryable()
        .Where(x => x.Country == criteria.Country)
        .SelectMany(x => x.SpeciesCodes)
        .Distinct()
        .ToList();

    var countries = _majorRegionRepository.AsQueryable()
      .Where(x => x.Code == criteria.MajorRegion)
      .SelectMany(x => x.Countries)
      .Distinct()
      .ToList();

    return _subRegion1Repository.AsQueryable()
      .Where(x => countries.Contains(x.Country))
      .SelectMany(x => x.SpeciesCodes)
      .Distinct()
      .ToList();
  }
  private KeywordListSource? _buildKeywordSource(KeywordListCriteria criteria)
  {
    var uniqueBirdCodes = _getBirdCodesFromCriteria(criteria);

    var speciesQuery = _speciesRepository.AsQueryable()
      .Where(x => uniqueBirdCodes.Contains(x.Code));
    if (criteria.OrderByName)
      speciesQuery = speciesQuery.OrderBy(x => x.Name);

    var species = new ConcurrentBag<EBirdSpecies>(speciesQuery);

    Console.WriteLine($"Found {species.Count} species for criteria {criteria}");
    if (species.Count == 0)
      return null;

    var source = new KeywordListSource(criteria, DateTime.Now);
    source.BuildOrders(criteria, species.ToList());

    return source;
  }
  #endregion

}
