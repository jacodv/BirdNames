using BirdNames.Core.Interfaces;
using BirdNames.Core.Models;
using BirdNames.Core.Services;
using BirdNames.Dal;
using BirdNames.Dal.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BirdNames.Core.StartUp;

public static class DiHelper
{
  public const string ApplicationName= "BirdNames";
  public static void SetupBirdNamesCore(this IServiceCollection services, string? baseDirectory=null)
  {
    var dataProtection = services.AddDataProtection();
    dataProtection.PersistKeysToFileSystem(new DirectoryInfo(baseDirectory??AppContext.BaseDirectory));
    dataProtection.SetApplicationName(ApplicationName);


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
    services.AddScoped<ISettingsService, SettingsService>();
  }
}
