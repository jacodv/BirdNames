namespace BirdNames.Core.Helpers;
public static class EnumHelper
{
  public static SortedList<int,string> GetOrderedEnumValues<T>()
    where T : Enum
  {
    SortedList<int,string> values = new();

    var enumValues = Enum.GetValues(typeof(T));
    foreach (var enumValue in enumValues)
      values.Add((int)enumValue, enumValue.ToString()!);
    return values;
  }}
