using BirdNames.Core.Models;
using HotChocolate.Data;
using MongoDB.Driver;

namespace BirdNames.Blazor.GraphQL;

public class BirdNamesQueries
{
  [UsePaging]
  [UseProjection]
  [UseSorting]
  [UseFiltering]
  public IExecutable<EBirdMajorRegion> GetMajorRegions(
    [Service] IMongoCollection<EBirdMajorRegion> collection) =>
    collection.AsExecutable();

  [UsePaging]
  [UseProjection]
  [UseSorting]
  [UseFiltering]
  public IExecutable<EBirdCountry> GetCountries(
    [Service] IMongoCollection<EBirdCountry> collection) =>
    collection.AsExecutable();

  [UsePaging]
  [UseProjection]
  [UseSorting]
  [UseFiltering]
  public IExecutable<EBirdSubRegion1> GetSubNational(
    [Service] IMongoCollection<EBirdSubRegion1> collection) =>
    collection.AsExecutable();

}
