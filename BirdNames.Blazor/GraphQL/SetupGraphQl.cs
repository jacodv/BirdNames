namespace BirdNames.Blazor.GraphQL;

public static class SetupGraphQl
{
  public static IServiceCollection ConfigureGraphQl(this IServiceCollection services)
  {
    services
      .AddGraphQLServer()
      .AddQueryType<BirdNamesQueries>()
      .AddMongoDbFiltering()
      .AddMongoDbSorting()
      .AddMongoDbProjections()
      .AddMongoDbPagingProviders();

    return services;
  }
}
