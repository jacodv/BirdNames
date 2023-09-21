using BirdNames.Dal.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;
using System.Xml.Serialization;

namespace BirdNames.Dal;

public class MongoRepository<TDocument> : IRepository<TDocument>
    where TDocument : IDocument
  {
    public readonly IMongoCollection<TDocument> Collection;
    public IMongoCollection<TDocument> GetCollection() => Collection;

    public MongoRepository(IDatabaseSettings settings)
    {
      var database = new MongoClient(settings.ConnectionString).GetDatabase(settings.DatabaseName);
      Collection = database.GetCollection<TDocument>(GetCollectionName(typeof(TDocument)));
    }

    private protected string GetCollectionName(Type documentType)
    {
      return ((BsonCollectionAttribute)documentType
        .GetCustomAttributes(typeof(BsonCollectionAttribute), true)
        .FirstOrDefault())?.CollectionName;
    }

    public virtual IQueryable<TDocument> AsQueryable()
    {
      return Collection.AsQueryable();
    }

    public virtual IEnumerable<TDocument> FilterBy(Expression<Func<TDocument, bool>> filterExpression)
    {
      return Collection.Find(filterExpression).ToEnumerable();
    }
    public virtual IEnumerable<TProjected> FilterBy<TProjected>(Expression<Func<TDocument, bool>> filterExpression, Expression<Func<TDocument, TProjected>> projectionExpression)
    {
      return Collection.Find(filterExpression).Project(projectionExpression).ToEnumerable();
    }

    public virtual TDocument FindOne(Expression<Func<TDocument, bool>> filterExpression)
    {
      return Collection.Find(filterExpression).FirstOrDefault();
    }
    public virtual Task<TDocument> FindOneAsync(Expression<Func<TDocument, bool>> filterExpression)
    {
      return Collection.Find(filterExpression).FirstOrDefaultAsync();
    }
    public virtual TDocument FindById(string id)
    {
      var objectId = new ObjectId(id);
      var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, objectId);
      return Collection.Find(filter).SingleOrDefault();
    }
    public virtual Task<TDocument> FindByIdAsync(string id)
    {
      var objectId = new ObjectId(id);
      var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, objectId);
      return Collection.Find(filter).SingleOrDefaultAsync();
    }


    public virtual void InsertOne(TDocument document)
    {
      Collection.InsertOne(document);
    }
    public virtual Task InsertOneAsync(TDocument document)
    {
      return Collection.InsertOneAsync(document);
    }
    public void InsertMany(ICollection<TDocument> documents)
    {
      Collection.InsertMany(documents);
    }
    public virtual async Task InsertManyAsync(ICollection<TDocument> documents)
    {
      await Collection.InsertManyAsync(documents);
    }

    public void ReplaceOne(TDocument document)
    {
      var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
      Collection.FindOneAndReplace(filter, document);
    }
    public virtual async Task ReplaceOneAsync(TDocument document)
    {
      var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
      await Collection.FindOneAndReplaceAsync(filter, document);
    }

    public void DeleteOne(Expression<Func<TDocument, bool>> filterExpression)
    {
      Collection.FindOneAndDelete(filterExpression);
    }
    public Task DeleteOneAsync(Expression<Func<TDocument, bool>> filterExpression)
    {
      return Collection.FindOneAndDeleteAsync(filterExpression);
    }
    public void DeleteById(string id)
    {
      var objectId = new ObjectId(id);
      var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, objectId);
      Collection.FindOneAndDelete(filter);
    }
    public Task DeleteByIdAsync(string id)
    {
      var objectId = new ObjectId(id);
      var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, objectId);
      return Collection.FindOneAndDeleteAsync(filter);
    }
    public DeleteResult DeleteMany(Expression<Func<TDocument, bool>> filterExpression)
    {
      return Collection.DeleteMany(filterExpression);
    }
    public Task<DeleteResult> DeleteManyAsync(Expression<Func<TDocument, bool>> filterExpression)
    {
      return Collection.DeleteManyAsync(filterExpression);
    }
    
    public BulkWriteResult<TDocument> UpsertMany(ICollection<TDocument> documents, Func<TDocument,Expression<Func<TDocument, bool>>> predicate)
    {
      var bulkOps = _buildUpsertOperations(documents, predicate);
      return Collection.BulkWrite(bulkOps);
    }
    public Task<BulkWriteResult<TDocument>> UpsertManyAsync(ICollection<TDocument> documents, Func<TDocument,Expression<Func<TDocument, bool>>> predicate)
    {
      var bulkOps = _buildUpsertOperations(documents, predicate);
      return  Collection.BulkWriteAsync(bulkOps);
    }

    public void ClearCollection()
    { 
      Collection.DeleteMany(FilterDefinition<TDocument>.Empty);
    }
    public Task ClearCollectionAsync()
    {
      return Collection.DeleteManyAsync(FilterDefinition<TDocument>.Empty);
    }

    #region Private
    private static List<WriteModel<TDocument>> _buildUpsertOperations(ICollection<TDocument> documents, Func<TDocument,Expression<Func<TDocument, bool>>> predicate)
    {
      var bulkOps = new List<WriteModel<TDocument>>();
      // ReSharper disable once LoopCanBeConvertedToQuery
      foreach (var document in documents)
      {
        var filter = Builders<TDocument>.Filter.Where(predicate(document));
        var upsertOne = new ReplaceOneModel<TDocument>(filter, document) { IsUpsert = true };
        bulkOps.Add(upsertOne);
      }

      return bulkOps;
    }
    #endregion
  }
