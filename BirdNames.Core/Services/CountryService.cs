using BirdNames.Core.Helpers;
using BirdNames.Core.Models;
using BirdNames.Dal.Interfaces;
using Microsoft.Extensions.Logging;

namespace BirdNames.Core.Services;
public class CountryService: ICountryService
{
  private readonly ILogger<CountryService> _logger;
  private readonly IRepository<EBirdCountry> _countryRepository;

  public CountryService(ILogger<CountryService> logger, IRepository<EBirdCountry> countryRepository)
  {
    _logger = logger;
    _countryRepository = countryRepository;
  }

  public async Task PersistCountries(Stream processCountriesCsv, CancellationToken token = default(CancellationToken))
  {
    var countries = new List<EBirdCountry>();
    var linesProcessed = 0;
    try
    {
      // ReSharper disable once ConvertToLocalFunction
      Action<IList<string>> processLineValues = (values) =>
      {
        var country = new EBirdCountry(values[0], values[1], values[2]);
        countries.Add(country);
        linesProcessed++;
      };

      var totalProcessed = FileHelpers.ProcessSimpleCsvLines(
        processCountriesCsv, 
        processLineValues, 
        token, 
        hasTextQualifier: false, 
        ignoreFirstLine: true,
        delimiters: new[] { "," });
      
      await _countryRepository.ClearCollectionAsync(); // Clear the collection
      _logger.LogInformation($"Deleted all countries");

      await _countryRepository.InsertManyAsync(countries);
      _logger.LogInformation($"Inserted {totalProcessed} countries");
    }
    catch (Exception e)
    {
      throw new InvalidOperationException($"Failed to processed the csv.  LinesProcessed:{linesProcessed}.  {e.Message}", e);
    }
  }
}

public interface ICountryService
{
  Task PersistCountries(Stream processCountriesCsv, CancellationToken token = default);
}
