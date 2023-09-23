using BirdNames.Dal.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BirdNames.Dal.Helpers;
public class MongoHelper
{
  public static async Task<bool> TestConnection(IDatabaseSettings settings)
  {
    try
    {
      var client = new MongoClient(settings.ConnectionString);
      var database = client.GetDatabase(settings.DatabaseName);
      await database.RunCommandAsync((Command<BsonDocument>)"{ping:1}");
      return true;
    }
    catch (Exception e)
    {
      Console.WriteLine(e);
      return false;
    }
  }
}
