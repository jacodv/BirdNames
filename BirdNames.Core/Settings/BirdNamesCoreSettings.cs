namespace BirdNames.Core.Settings;
public  class BirdNamesCoreSettings
{
  public string EbirdApiKeyName { get; set; } = "x-ebirdapitoken";
  public string? EbirdApiKeyValue { get; set; }
  public string EbirdBaseUrl { get; set; } = "https://api.ebird.org";
  public string? AdminPassword { get; set; }
  public string? AdminPasswordHint { get; set; }
  public bool AdminPasswordSet { get; set; }
}
