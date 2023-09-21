using BirdNames.Core.Models;
using BirdNames.Dal;
using BirdNames.Dal.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace BirdNames.Core.StartUp;
public class SetupDatabase
{
  public static async Task CreateIndexes(IServiceProvider serviceProvider)
  {
    await _createOrdersIndexes(((MongoRepository<BirdNamesOrder>) 
      serviceProvider.GetService<IRepository<BirdNamesOrder>>()!).Collection);
    await _createFamilyIndexes(((MongoRepository<BirdNamesFamily>) 
      serviceProvider.GetService<IRepository<BirdNamesFamily>>()!).Collection);
    await _createGenusIndexes(((MongoRepository<BirdNamesGenus>) 
      serviceProvider.GetService<IRepository<BirdNamesGenus>>()!).Collection);
    await _createSpeciesIndexes(((MongoRepository<BirdNamesSpecies>) 
      serviceProvider.GetService<IRepository<BirdNamesSpecies>>()!).Collection);
    await _createBirdNamesRegionIndexes(((MongoRepository<BirdNamesRegion>) 
      serviceProvider.GetService<IRepository<BirdNamesRegion>>()!).Collection);
    await _createMajorRegionIndexes(((MongoRepository<EBirdMajorRegion>) 
      serviceProvider.GetService<IRepository<EBirdMajorRegion>>()!).Collection);
    await _createCountryIndexes(((MongoRepository<EBirdCountry>) 
      serviceProvider.GetService<IRepository<EBirdCountry>>()!).Collection);
    await _createSubRegion1Indexes(((MongoRepository<EBirdSubRegion1>) 
      serviceProvider.GetService<IRepository<EBirdSubRegion1>>()!).Collection);
    await _createSubRegion2Indexes(((MongoRepository<EBirdSubRegion2>) 
      serviceProvider.GetService<IRepository<EBirdSubRegion2>>()!).Collection);
    await _createEBirdSpeciesIndexes(((MongoRepository<EBirdSpecies>) 
      serviceProvider.GetService<IRepository<EBirdSpecies>>()!).Collection);
  }

  private static async Task _createOrdersIndexes(IMongoCollection<BirdNamesOrder> collection)
  {
    await _createModelBaseIndexes(collection);

    var codeIndex = Builders<BirdNamesOrder>.IndexKeys.Ascending(x => x.Code);
    await _createIndex(collection, codeIndex, isUnique:false);
  }
  private static async Task _createFamilyIndexes(IMongoCollection<BirdNamesFamily> collection)
  {
    await _createModelBaseIndexes(collection);

    var nameIndex = Builders<BirdNamesFamily>.IndexKeys.Ascending(x => x.Name);
    await _createIndex(collection, nameIndex, isUnique:false);

    var codeIndex = Builders<BirdNamesFamily>.IndexKeys.Ascending(x => x.OrderCode);
    await _createIndex(collection, codeIndex, isUnique:false);
  }
  private static async Task _createGenusIndexes(IMongoCollection<BirdNamesGenus> collection)
  {
    await _createModelBaseIndexes(collection);

    var latinIndex = Builders<BirdNamesGenus>.IndexKeys.Ascending(x => x.Latin);
    await _createIndex(collection, latinIndex, isUnique:false);

    var nameIndex = Builders<BirdNamesGenus>.IndexKeys.Ascending(x => x.FamilyName);
    await _createIndex(collection, nameIndex, isUnique:false);
  }
  private static async Task _createSpeciesIndexes(IMongoCollection<BirdNamesSpecies> collection)
  {
    await _createModelBaseIndexes(collection);

    var codeIndex = Builders<BirdNamesSpecies>.IndexKeys.Ascending(x => x.Name);
    await _createIndex(collection, codeIndex, isUnique:false);

    var genusLatinIndex = Builders<BirdNamesSpecies>.IndexKeys.Ascending(x => x.GenusLatin);
    await _createIndex(collection, genusLatinIndex, isUnique:false);

    var latinIndex = Builders<BirdNamesSpecies>.IndexKeys.Ascending(x => x.Latin);
    await _createIndex(collection, latinIndex, isUnique:false);
  }
  private static async Task _createBirdNamesRegionIndexes(IMongoCollection<BirdNamesRegion> collection)
  {
    await _createModelBaseIndexes(collection);

    var codeIndex = Builders<BirdNamesRegion>.IndexKeys.Ascending(x => x.Code);
    await _createIndex(collection, codeIndex, isUnique:false);
  }
  private static  Task _createCountryIndexes(IMongoCollection<EBirdCountry> collection)
  {
    return _createCodeIndex(collection);
  }
  private static async Task _createSubRegion1Indexes(IMongoCollection<EBirdSubRegion1> collection)
  {
    await _createCodeAndNameIndexes(collection);
    var countryIndex = Builders<EBirdSubRegion1>.IndexKeys.Ascending(x => x.Country);
    await _createIndex(collection, countryIndex, isUnique:false);
  }
  private static async Task _createSubRegion2Indexes(IMongoCollection<EBirdSubRegion2> collection)
  {
    await _createCodeAndNameIndexes(collection);
    var sub1Index = Builders<EBirdSubRegion2>.IndexKeys.Ascending(x => x.SubRegion1);
    await _createIndex(collection, sub1Index, isUnique:false);
    var countryIndex = Builders<EBirdSubRegion2>.IndexKeys.Ascending(x => x.Country);
    await _createIndex(collection, countryIndex, isUnique:false);
  }
  private static Task _createMajorRegionIndexes(IMongoCollection<EBirdMajorRegion> collection)
  {
    return _createCodeIndex(collection);
  }
  private static async Task _createEBirdSpeciesIndexes(IMongoCollection<EBirdSpecies> collection)
  {
    await  _createCodeIndex(collection);
    var nameIndex = Builders<EBirdSpecies>.IndexKeys.Ascending(x => x.Name);
    await _createIndex(collection, nameIndex, isUnique:true);

    var sciNameIndex = Builders<EBirdSpecies>.IndexKeys.Ascending(x => x.SciName);
    await _createIndex(collection, sciNameIndex, isUnique:true);
  }
  private static async Task _createCodeAndNameIndexes<T>(IMongoCollection<T> collection)
    where T : CodeAndNameBase
  {
    await _createCodeIndex(collection);
    var nameIndex = Builders<T>.IndexKeys.Ascending(x => x.Name);
    await _createIndex(collection, nameIndex, isUnique:false);
  }
  private static async Task _createCodeIndex<T>(IMongoCollection<T> collection)
    where T : CodeAndNameBase
  {
    var codeIndex = Builders<T>.IndexKeys.Ascending(x => x.Code);
    await _createIndex(collection, codeIndex, isUnique:true);
  }
  private static async Task _createModelBaseIndexes<T>(IMongoCollection<T> modelBaseCollection)
  where T : ModelVersionBase
  {

    var versionIndex = Builders<T>.IndexKeys.Descending(x => x.Version);
    await _createIndex(modelBaseCollection, versionIndex, isUnique:false);
  }
  private static async Task _createIndex<T>(IMongoCollection<T> userCollection, IndexKeysDefinition<T> indexKeys, bool isUnique = true)
  {
    var createIndexOptions = new CreateIndexOptions()
    {
      Unique = isUnique
    };
    var createIndexModel = new CreateIndexModel<T>(indexKeys, createIndexOptions);
    await userCollection.Indexes.CreateOneAsync(createIndexModel);
  }
}
