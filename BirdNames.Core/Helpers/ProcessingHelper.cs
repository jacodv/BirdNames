using Microsoft.Extensions.Logging;

namespace BirdNames.Core.Helpers;
public static class ProcessingHelper
{
  public static async Task ProcessInParallel<T>(this IEnumerable<T> items, bool refresh, Func<T,Task>? handleItem, Func<T,bool> existsExpression, CancellationToken token, ILogger logger, int delay = 250)
  {
    var options = new ParallelOptions { MaxDegreeOfParallelism = 4 };
    var hasErrors = false;
    await Parallel.ForEachAsync(items, options, async (item, _) =>
    {
      try
      {
        if (hasErrors || token.IsCancellationRequested)
          return;

        var exist = existsExpression(item);
        if (exist && !refresh)
        {
          logger.LogDebug($"Skipping {typeof(T)}:{item}");
          return;
        }

        if(handleItem!=null)
          await handleItem(item);
      }
      catch (Exception e)
      {
        logger.LogWarning($"ProcessInParallel failed: {typeof(T)}:{item}.  {e.Message}");
        hasErrors = true;
      }
      if(delay>0)
        await Task.Delay(delay, token); // Give the servers a breather...
    });

  }

  public static IEnumerable<IEnumerable<TSource>> Batch<TSource>(this IEnumerable<TSource> source, int size)
  {
    TSource[]? bucket = null;
    var count = 0;

    foreach (var item in source)
    {
      bucket ??= new TSource[size];

      bucket[count++] = item;
      if (count != size)
        continue;

      yield return bucket;

      bucket = null;
      count = 0;
    }

    if (bucket != null && count > 0)
      yield return bucket.Take(count);
  }
}
