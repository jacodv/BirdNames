using System.Collections.Concurrent;
using System.Dynamic;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using BirdNames.Core.Helpers;
using BirdNames.Core.Interfaces;
using BirdNames.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace BirdNames.Core.Xml;

public sealed class BirdNamesXmlProcessor : IDisposable, IAsyncDisposable
{
  private readonly Stream _xmlSource;
  private string? _listVersion;
  private int? _listYear;

  private BirdNamesOrder? _currentOrder;
  private ConcurrentBag<BirdNamesOrder> _orders = new();

  private BirdNamesFamily? _currentFamily;
  private ConcurrentBag<BirdNamesFamily> _families = new();

  private BirdNamesGenus? _currentGenus;
  private ConcurrentBag<BirdNamesGenus> _genera = new();

  private BirdNamesSpecies? _currentSpecies;
  private ConcurrentBag<BirdNamesSpecies> _species = new();

  private Dictionary<string, BirdNamesRegion> _regions = new();

  public BirdNamesXmlProcessor(Stream xmlSource)
  {
    _xmlSource = xmlSource;
  }

  #region IDisposable

  public void Dispose()
  {
    _xmlSource.Dispose();
  }

  public async ValueTask DisposeAsync()
  {
    await _xmlSource.DisposeAsync();
  }

  #endregion

  public async Task ProcessXml(IServiceProvider serviceProvider)
  {
    var settings = new XmlReaderSettings
    {
      Async = false,
      IgnoreWhitespace = true,
      IgnoreComments = true,
      IgnoreProcessingInstructions = true,
      DtdProcessing = DtdProcessing.Ignore
    };
    using var xmlReader = XmlReader.Create(_xmlSource, settings);
    //xmlReader.MoveToContent();
    _processXDoc(xmlReader);

    Console.WriteLine($"List Version: {_listVersion}, List Year: {_listYear}");
    Console.WriteLine($"Orders: {_orders.Count}, " +
                      $"Families: {_families.Count}, " +
                      $"Genera: {_genera.Count}, " +
                      $"Species: {_species.Count}");

    var dataService = serviceProvider.GetService<IBirdNamesDataServices>()!;
    var model = new ProcessedItemsModel(_listVersion!)
    {
      Orders = _orders.ToList(),
      Families = _families.ToList(),
      Genera = _genera.ToList(),
      Species = _species.ToList(),
      Regions = _regions.Values.ToList()
    };
    await dataService.PersistProcessedItems(model);
  }

  private void _processXDoc(XmlReader reader)
  {
    var xDoc = XDocument.Load(reader);
    var listHeader = xDoc.Descendants("ioclist").First();
    _listVersion = listHeader.Attribute("version")?.Value;
    if (int.TryParse(listHeader.Attribute("year")?.Value, null, out var year))
    {
      _listYear = year;
    }

    var xOrders = xDoc.Descendants("order");
    foreach (var xElement in xOrders)
    {
      _processOrder(xElement);
    }
  }
  private void _processOrder(XElement element)
  {
    var (instance, nextElements) = _processAndAssign(element, _orders, "family");
    _currentOrder = instance;
    foreach (var xElement in nextElements)
      _processFamily(xElement);
  }
  private void _processFamily(XElement element)
  {
    var (instance, nextElements) = _processAndAssign(element, _families, "genus");
    _currentFamily = instance;
    foreach (var xElement in nextElements)
      _processGenus(xElement);
  }
  private void _processGenus(XElement element)
  {
    var (instance, nextElements) = _processAndAssign(element, _genera, "species");
    _currentGenus = instance;
    foreach (var xElement in nextElements)
      _processSpecies(xElement);
  }
  private void _processSpecies(XElement element)
  {
    var (instance, nextElements) = _processAndAssign(element, _species, "subspecies");
    _currentSpecies = instance;

    var regions = BirdNamesRegion.FromSpecies(_currentSpecies!);
    foreach (var region in regions)
    {
      if (!_regions.ContainsKey(region.Code))
        _regions.Add(region.Code, region);
      else
        BirdNamesRegion.Union(_regions[region.Code], region);
    }

    foreach (var subElement in nextElements)
      _processSubspecies(subElement);
  }
  private void _processSubspecies(XElement element)
  {
    var (instance, _) = _processAndAssign<BirdNamesSubSpecies>(parent: element, list: null, nextItem: null);
    _currentSpecies!.AddSubspecies(instance!);
  }

  private (dynamic? instance, IList<XElement> nextElement) _processAndAssign<T>(XElement parent, ConcurrentBag<T>? list, string? nextItem = null)
  {
    var nextElements = new List<XElement>();
    var temp = new ExpandoObject() as IDictionary<string, object?>;

    try
    {
      var children = parent.Elements();
      foreach (var child in children)
      {
        if (child.Name == nextItem)
        {
          nextElements.Add(child);
          continue;
        }

        if (!BirdDataHelper.PropertyNameLookup.ContainsKey(child.Name.LocalName))
        {
          Console.WriteLine($"Unknown {child.Name.LocalName}");
          continue;
        }

        var properName = BirdDataHelper.PropertyNameLookup[child.Name.LocalName];
        var value = child.Value;
        temp[properName] = value;
        _addParents(temp);
      }
      var instance = _getInstanceFromExpando<T>((temp as ExpandoObject)!);

      list?.Add(instance!);
      return (instance, nextElements);
    }
    catch (Exception e)
    {
      throw new Exception($"Error processing {parent.Name}|Type:{parent.NodeType}", e);
    }
  }
  private void _addParents(IDictionary<string, object?> temp)
  {
    temp["Version"] = _listVersion;
    temp["Year"] = _listYear;
    temp["OrderCode"] = _currentOrder?.Code;
    temp["FamilyName"] = _currentFamily?.Name;
    temp["GenusLatin"] = _currentGenus?.Latin;
  }
  private static T? _getInstanceFromExpando<T>(ExpandoObject expando)
  {
    return JsonSerializer.Deserialize<T>(
      JsonSerializer.Serialize(expando));
  }
}