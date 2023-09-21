namespace BirdNames.Core.Interfaces;

public interface IDatabaseStatisticsService
{
  Task<Dictionary<string, int>> GetCollectionCountersAsync();
}