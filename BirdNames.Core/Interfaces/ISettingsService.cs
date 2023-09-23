using BirdNames.Core.Settings;
using BirdNames.Dal.Interfaces;

namespace BirdNames.Core.Interfaces;

public interface ISettingsService
{
  Task SaveSettings(BirdNamesCoreSettings settings, string? baseDirectory = null);
  Task<bool> TestDatabaseConnection(IDatabaseSettings settings);
  string Protect(string valueToProtect);
  string Unprotect(string valueToUnprotect);
}