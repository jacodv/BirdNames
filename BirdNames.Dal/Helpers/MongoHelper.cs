using BirdNames.Dal.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BirdNames.Dal.Helpers;
public static class MongoHelper
{
  public static async Task TestConnection(IDatabaseSettings settings)
  {
    // ReSharper disable once UseObjectOrCollectionInitializer
    var client = new MongoClient(settings.ConnectionString);
      client.Settings.ConnectTimeout = TimeSpan.FromSeconds(5);
      var database = client.GetDatabase(settings.DatabaseName);
      await database.RunCommandAsync((Command<BsonDocument>)"{ping:1}");
  }
}
