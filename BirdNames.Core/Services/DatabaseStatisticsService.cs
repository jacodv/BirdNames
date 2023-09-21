using BirdNames.Core.Interfaces;
using BirdNames.Core.Models;
using BirdNames.Dal.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BirdNames.Core.Services;
public class DatabaseStatisticsService: IDatabaseStatisticsService
{
  private static Dictionary<string, int> _collectionCounters=new();
  private readonly IRepository<EBirdMajorRegion> _majorRegionRepository;

  public DatabaseStatisticsService(IRepository<EBirdMajorRegion> majorRegionRepository)
  {
    _majorRegionRepository = majorRegionRepository;
  }
  public async Task<Dictionary<string, int>> GetCollectionCountersAsync()
  {
    if(_collectionCounters.Count>0)
      return _collectionCounters;

    var result = new Dictionary<string, int>();
    var database = _majorRegionRepository.GetCollection().Database;
    var collectionNames = (await database.ListCollectionNamesAsync()).ToList();
    foreach (var collectionName in collectionNames)
    {
      var collection = database.GetCollection<BsonDocument>(collectionName);
      var count = await collection.CountDocumentsAsync(new BsonDocument());
      result.Add(collectionName, (int)count);
    }

    _collectionCounters= result;
    return result;
  }

}