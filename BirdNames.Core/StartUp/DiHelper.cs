using BirdNames.Core.Interfaces;
using BirdNames.Core.Models;
using BirdNames.Core.Services;
using BirdNames.Dal;
using BirdNames.Dal.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BirdNames.Core.StartUp;

public static class DiHelper
{
  public static void SetupBirdNamesCore(this IServiceCollection services)
  {
    IDatabaseSettings databaseSettings=new DatabaseSettings();
    services.AddSingleton(serviceProvider =>
    {
      databaseSettings = serviceProvider.GetRequiredService<IOptions<DatabaseSettings>>().Value;
      return databaseSettings;
    });
    services.AddScoped(typeof(IRepository<>), typeof(MongoRepository<>));
    services.AddScoped<IBirdNamesDataServices, BirdNamesDataServices>();
    services.AddScoped<ICountryService, CountryService>();
    services.AddScoped<IEBirdService, EBirdService>();
    services.AddScoped<IDatabaseStatisticsService, DatabaseStatisticsService>();
  }
}
