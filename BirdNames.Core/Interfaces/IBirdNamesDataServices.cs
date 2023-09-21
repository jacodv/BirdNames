using BirdNames.Core.Models;

namespace BirdNames.Core.Interfaces;

public interface IBirdNamesDataServices
{
  Task PersistProcessedItems(ProcessedItemsModel model, CancellationToken token = default);
}