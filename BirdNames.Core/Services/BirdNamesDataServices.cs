using BirdNames.Core.Helpers;
using BirdNames.Core.Interfaces;
using BirdNames.Core.Models;
using BirdNames.Dal.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;

// ReSharper disable ConvertToLocalFunction
#pragma warning disable IDE0039

namespace BirdNames.Core.Services;

public sealed class BirdNamesDataServices : IBirdNamesDataServices
{
  private readonly ILogger<BirdNamesDataServices> _logger;
  private readonly IRepository<BirdNamesOrder> _orderRepository;
  private readonly IRepository<BirdNamesFamily> _familyRepository;
  private readonly IRepository<BirdNamesGenus> _genusRepository;
  private readonly IRepository<BirdNamesSpecies> _speciesRepository;
  private readonly IRepository<BirdNamesRegion> _regionsRepository;
  private readonly IValidator<BirdNamesOrder> _orderValidator;
  private readonly IValidator<BirdNamesFamily> _familyValidator;
  private readonly IValidator<BirdNamesGenus> _genusValidator;
  private readonly IValidator<BirdNamesSpecies> _speciesValidator;

  public BirdNamesDataServices(
    ILogger<BirdNamesDataServices> logger,
    IRepository<BirdNamesOrder> orderRepository,
    IRepository<BirdNamesFamily> familyRepository,
    IRepository<BirdNamesGenus> genusRepository,
    IRepository<BirdNamesSpecies> speciesRepository,
    IRepository<BirdNamesRegion> regionsRepository,
    IValidator<BirdNamesOrder> orderValidator,
    IValidator<BirdNamesFamily> familyValidator,
    IValidator<BirdNamesGenus> genusValidator,
    IValidator<BirdNamesSpecies> speciesValidator
    )
  {
    _logger = logger;
    _orderRepository = orderRepository;
    _familyRepository = familyRepository;
    _genusRepository = genusRepository;
    _speciesRepository = speciesRepository;
    _regionsRepository = regionsRepository;
    _orderValidator = orderValidator;
    _familyValidator = familyValidator;
    _genusValidator = genusValidator;
    _speciesValidator = speciesValidator;
  }

  public async Task PersistProcessedItems(ProcessedItemsModel model, CancellationToken token=default)
  {
    await _persist(model.Version, model.Orders, _orderRepository, _orderValidator, token, logger:_logger);
    await _persist(model.Version, model.Families, _familyRepository, _familyValidator, token, logger:_logger);
    await _persist(model.Version, model.Genera, _genusRepository, _genusValidator, token, logger:_logger);
    await _persist(model.Version, model.Species, _speciesRepository, _speciesValidator, token, logger:_logger);
    await _persist(model.Version, model.Regions, _regionsRepository, null, token, logger:_logger);
  }

  private static async Task _persist<T>(string version, ICollection<T> items, IRepository<T> repo, IValidator<T>? validator, CancellationToken token, ILogger logger)
    where T : ModelVersionBase
  {
    var hasValidationErrors = false;
    var validateCount = 0;
    var handleItem = (T item) =>
    {
      try
      {
        var validationResult = validator?.Validate(item);
        Interlocked.Increment(ref validateCount);
        if(validateCount % 200 == 0)
          logger.LogDebug($"Validated {validateCount} {typeof(T)}");
        if (validationResult?.IsValid==true)
          return Task.CompletedTask;

        hasValidationErrors = true;
        logger.LogWarning($"Validation failed for: {typeof(T)}:{item}");
        foreach (var error in validationResult!.Errors)
          logger.LogWarning($"  {error}");
        throw new InvalidOperationException("Processing stopped due to validation errors");
      }
      catch (Exception e)
      {
        logger.LogError($"Validation failed for: {typeof(T)}:{item}.  {e.Message}", e);
        hasValidationErrors = true;
        return Task.CompletedTask;
      }
    };

    if(validator!=null)
      await items.ProcessInParallel(true, handleItem, _ => false, token,delay:0, logger:logger);

    // If there are validation errors, don't persist
    if(token.IsCancellationRequested)
      return;
    if (hasValidationErrors)
      throw new InvalidOperationException("Processing stopped due to validation errors");

    var result = await repo.DeleteManyAsync(x=>x.Version == version);
    logger.LogInformation($"Deleted {result.DeletedCount} {typeof(T)}");

    await repo.InsertManyAsync(items);
    logger.LogInformation($"Inserted: {items.Count} {typeof(T)}");
  }
}