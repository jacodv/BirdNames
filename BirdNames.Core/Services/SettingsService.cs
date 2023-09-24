using System.Dynamic;
using BirdNames.Core.Interfaces;
using BirdNames.Core.Settings;
using BirdNames.Dal.Helpers;
using BirdNames.Dal.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;

namespace BirdNames.Core.Services;
public class SettingsService: ISettingsService
{
  private readonly ILogger<SettingsService> _logger;
  private readonly IDataProtectionProvider _dataProtectionProvider;

  public SettingsService(ILogger<SettingsService> logger, IDataProtectionProvider dataProtectionProvider)
  {
    _logger = logger;
    _dataProtectionProvider = dataProtectionProvider;
  }

  public async Task SaveSettings(BirdNamesCoreSettings settings, string? baseDirectory = null)
  {
    try
    {

      var filePath = Path.Combine(baseDirectory ?? AppContext.BaseDirectory, "appsettings.json");
      if(!File.Exists(filePath))
        throw new FileNotFoundException("appsettings.json not found", filePath);

      var json = await File.ReadAllTextAsync(filePath);
      dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandoObject>(json) ?? 
                         new ExpandoObject();

      jsonObj.BirdNamesCoreSettings = settings;

      var output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
      await File.WriteAllTextAsync(filePath, output);
    }
    catch (Exception e)
    {
      throw new InvalidOperationException($"Failed to save settings.  {e.Message}", e);
    }
  }

  public async Task<bool> TestDatabaseConnection(IDatabaseSettings settings)
  {
    try
    {
      await MongoHelper.TestConnection(settings);
      return true;
    }
    catch (Exception e)
    {
      _logger.LogError(e, $"Failed to connect to database.  {e.Message}");
      return false;
    }
  }

  public string Protect(string valueToProtect)
  {
    var protector = _dataProtectionProvider.CreateProtector("BirdNames.Core.Services.SettingsService");
    return protector.Protect(valueToProtect);
  }

  public string Unprotect(string valueToUnprotect)
  {
    var protector = _dataProtectionProvider.CreateProtector("BirdNames.Core.Services.SettingsService");
    return protector.Unprotect(valueToUnprotect);
  }
}