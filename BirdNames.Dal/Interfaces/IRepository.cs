using MongoDB.Driver;
using System.Linq.Expressions;

namespace BirdNames.Dal.Interfaces;

public interface IRepository<TDocument> where TDocument : IDocument
{
  IMongoCollection<TDocument> GetCollection();
  IQueryable<TDocument> AsQueryable();

  IEnumerable<TDocument> FilterBy(Expression<Func<TDocument, bool>> filterExpression);
  IEnumerable<TProjected> FilterBy<TProjected>(Expression<Func<TDocument, bool>> filterExpression, Expression<Func<TDocument, TProjected>> projectionExpression);

  TDocument FindOne(Expression<Func<TDocument, bool>> filterExpression);
  Task<TDocument> FindOneAsync(Expression<Func<TDocument, bool>> filterExpression);
  TDocument FindById(string id);
  Task<TDocument> FindByIdAsync(string id);

  void InsertOne(TDocument document);
  Task InsertOneAsync(TDocument document);
  void InsertMany(ICollection<TDocument> documents);
  Task InsertManyAsync(ICollection<TDocument> documents);

  void ReplaceOne(TDocument document);
  Task ReplaceOneAsync(TDocument document);

  void DeleteOne(Expression<Func<TDocument, bool>> filterExpression);
  Task DeleteOneAsync(Expression<Func<TDocument, bool>> filterExpression);
  void DeleteById(string id);
  Task DeleteByIdAsync(string id);
  DeleteResult DeleteMany(Expression<Func<TDocument, bool>> filterExpression);
  Task<DeleteResult> DeleteManyAsync(Expression<Func<TDocument, bool>> filterExpression);

  BulkWriteResult<TDocument> UpsertMany(ICollection<TDocument> documents, Func<TDocument,Expression<Func<TDocument, bool>>> predicate);
  Task<BulkWriteResult<TDocument>> UpsertManyAsync(ICollection<TDocument> documents, Func<TDocument,Expression<Func<TDocument, bool>>> predicate);
  
  void ClearCollection();
  Task ClearCollectionAsync();
}
