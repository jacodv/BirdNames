using BirdNames.Dal.Interfaces;

namespace BirdNames.Core.Models;

public  class DatabaseSettings: IDatabaseSettings
{
  #region Implementation of IDatabaseSettings

  public string DatabaseName { get; set; } = "BirdNames";
  public string ConnectionString { get; set; } = "mongodb://localhost";
  public string Secret { get; set; } = "B1rdN@m3sS3cret!";

  #endregion
}
