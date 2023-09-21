using BirdNames.Core.Helpers;
using BirdNames.Core.Models;
using BirdNames.Dal.Interfaces;

namespace BirdNames.Core.Services;
public class CountryService: ICountryService
{
  private readonly IRepository<EBirdCountry> _countryRepository;

  public CountryService(IRepository<EBirdCountry> countryRepository)
  {
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
      Console.WriteLine($"Deleted all countries");

      await _countryRepository.InsertManyAsync(countries);
      Console.WriteLine($"Inserted {totalProcessed} countries");
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
