using BirdNames.Core.Enums;
using BirdNames.Core.Services;

namespace BirdNames.Core.Models;

public class KeywordListSource
{
  private string _lineTabs = string.Empty;
  public KeywordListCriteria Criteria { get; }
  public DateTime ListDate { get; }

  public KeywordListSource(KeywordListCriteria criteria, DateTime listDate)
  {
    Criteria = criteria;
    ListDate = listDate;
  }

  public void WriteFileHeader(StreamWriter writer)
  {
    writer.WriteLine($"[BirdNames Generated {ListDate:yyyy-MM-dd} for {Criteria}]");
    _lineTabs = "\t";
    foreach (var fileHeaderSynonym in Criteria.HeaderSynonyms.ToString("G").Split(','))
      writer.WriteLine($"{_lineTabs}{{{fileHeaderSynonym.Trim()}}}");
  }
  public List<KeywordListOrder> Orders { get; set; } = new();

  public void BuildOrders(KeywordListCriteria criteria, IList<EBirdSpecies> species)
  {
    if (!criteria.Depth.HasFlag(KeywordDepth.Order))
    {
      Console.WriteLine($"Ignoring orders");
      var ignoredOrder = new KeywordListOrder(EBirdService.Ignored, EBirdService.Ignored) { Ignore = true };

      ignoredOrder.BuildFamilies(criteria, species);
      Orders.Add(ignoredOrder);
      return;
    }

    var orders = species
      .GroupBy(x => x.Order)
      .Select(x => new KeywordListOrder(x.Key, x.First().Order))
      .ToList();

    Console.WriteLine($"Orders:{orders.Count} from Species:{species.Count} for {criteria}");
    foreach (var order in orders)
    {
      var orderItem = new KeywordListOrder(order.Code, order.Name);
      orderItem.BuildFamilies(criteria, species.Where(x => x.Order == order.Code).ToList());
      Orders.Add(orderItem);
    }
  }
  public async Task GetFileContent(StreamWriter writer)
  {
    WriteFileHeader(writer);

    foreach (var order in Orders)
      await order.GetFileContent(writer, _lineTabs);
  }
}