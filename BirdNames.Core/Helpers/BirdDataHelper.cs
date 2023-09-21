namespace BirdNames.Core.Helpers;
public static class BirdDataHelper
{
  public static Dictionary<string,(string region, string notes)> RegionsLookup { get; } = new()
  {
    {"NA",("North America", "Includes the Caribbean")},
    {"MA",("Middle  America", "Mexico through Panama")},
    {"SA",("South  America", "Includes Trinidad, the Galápagos Is. and the Falkland Is")},
    {"LA",("Latin  America", "Middle & South America: Replaced with MA, SA 6.3")},
    {"AF",("Africa", "Entire continent (including North Africa and Madagascar) rather than Africa south of Sahara")},
    {"PAL",("Eurasia", "formerly (EU)–Europe and Asia from the Middle East through central Asia north of the  Himalayas, Siberia and northern China to Japan.")},
    {"EU",("Eurasia", "Europe and Asia from the Middle East through central Asia north of the  Himalayas, Siberia and northern China to Japan.")},
    {"OR",("Oriental  Region", "South Asia from Pakistan to Taiwan, plus southeast Asia, the Philippines  and Greater Sundas")},
    {"AU",("Australasia", "Wallacea (Indonesian islands east of Wallace’s  Line), New Guinea and its islands, Australia, New Zealand and its subantarctic islands, the Solomons, New Caledonia and Vanuatu")},
    {"AO",("Atlantic Ocean", "")},
    {"PO",("Pacific Ocean", "")},
    {"IO",("Indian Ocean", "")},
    {"TrO",("Tropical Ocean", "")},
    {"TO",("Temperate Ocean", "")},
    {"NO",("Northern Ocean", "")},
    {"SO",("Southern Ocean", "")},
    {"AN",("AN", "Antarctica ")},
    {"Worldwide",("Worldwide", "")},
  };

  public static Dictionary<string, string> PropertyNameLookup = new()
  {
    {"code", "Code"},
    {"latin_name", "Latin"},
    {"note", "Note"},
    {"english_name", "Name"},
    {"url", "Url"},
    {"authority", "Authority"},
    {"breeding_regions", "BreedingRegions"},
    {"nonbreeding_regions", "NonBreedingRegions"},
    {"breeding_subregions", "BreedingSubRegions"},
    {"nonbreeding_subregions", "NonBreedingSubRegions"},
    {"extinct", "Extinct"},
  };
}
