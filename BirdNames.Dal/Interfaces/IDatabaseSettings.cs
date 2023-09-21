namespace BirdNames.Dal.Interfaces;

public interface IDatabaseSettings
{
  string DatabaseName { get; set; }
  string ConnectionString { get; set; }
  string Secret { get; set; }
}