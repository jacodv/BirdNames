using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.Reflection;
using BirdNames.Core.Enums;
using BirdNames.Core.Helpers;
using BirdNames.Core.Models;
using BirdNames.Core.Settings;
using BirdNames.Core.StartUp;
using BirdNames.Core.Xml;
using BirdNames.Tool.Helpers;
using FluentValidation;

namespace BirdNames.Tool;

internal class Program
{
  public static IConfiguration? Configuration { get; set; }
  public static IServiceProvider? ServiceProvider { get; set; }
  public static string BasePath { get; set; } = string.Empty;
  public const string SettingsFileName = "appsettings.json";

  // ReSharper disable once InconsistentNaming
  // ReSharper disable once ArrangeTypeMemberModifiers
  static async Task<int> Main(string[] args)
  {
    BasePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
    if (!Directory.Exists(BasePath))
    {
      Console.WriteLine($"BasePath: {BasePath} does not exist.");
      return 1;
    }

    var services = _init();
    services.SetupBirdNamesCore();

    ServiceProvider = services.BuildServiceProvider();

    var rootCommand = new RootCommand("Bird Names command line tool");
    rootCommand.AddCommand(_getProcessXmlCommand());
    rootCommand.AddCommand(_processMajorRegions());
    rootCommand.AddCommand(_processCountriesCsvCommand());
    rootCommand.AddCommand(_processSubRegions1Command());
    rootCommand.AddCommand(_processSubRegions2Command());
    rootCommand.AddCommand(_processSubRegions1Species());
    rootCommand.AddCommand(_processUniqueSpecies());
    rootCommand.AddCommand(_processVerifyPersistedSpecies());
    rootCommand.AddCommand(_downloadKeyWords());
    rootCommand.AddCommand(_tempWorkCommand());

    await SetupDatabase.CreateIndexes(ServiceProvider);
    return await rootCommand.InvokeAsync(args);
  }

  private static IServiceCollection _init()
  {
    Configuration = new ConfigurationBuilder()
      .SetBasePath(BasePath!)
      .AddJsonFile(SettingsFileName, true, true)
      .AddEnvironmentVariables()
      .Build();
    var services = new ServiceCollection()
      .Configure<DatabaseSettings>(settings =>
      {
        Configuration.GetSection(nameof(DatabaseSettings)).Bind(settings);
      })
      .Configure<BirdNamesCoreSettings>(settings =>
      {
        Configuration.GetSection(nameof(BirdNamesCoreSettings)).Bind(settings);
      })
      .AddOptions();

    services.AddValidatorsFromAssemblyContaining<ModelVersionBaseValidator<BirdNamesOrder>>();

    return services;
  }
  private static Command _getProcessXmlCommand()
  {
    var command = new Command("process-bird-names-xml", "Process the xml master list from: https://www.worldbirdnames.org/new/ioc-lists/master-list-2/");
    var sourceFileOption = command.AddSourceFileOption("Full path and file name of the source xml document");

    command.SetHandler(async (sourceFile) =>
    {
      await BirdNamesFx.ProcessXml(sourceFile, ServiceProvider!);
      Console.WriteLine("\nDone");
      Console.ReadKey();
    },sourceFileOption);

    return command;
  }
  private static Command _processCountriesCsvCommand()
  {
    var command = new Command("process-countries-csv", "Process countries.  Expected CSV format Code,Name,Continent");
    var sourceFileOption = command.AddSourceFileOption("Full path and file name of the source csv file");

    command.SetHandler(async (csvFile) =>
    {
      try
      {
        await BirdNamesFx.ProcessCountriesCsv(csvFile, ServiceProvider!);
      }
      catch (Exception e)
      {
        Console.WriteLine($"Failed to process the Countries CSV.  {e.Message}.\n\n{e}");
      }
    }, sourceFileOption);

    return command;
  }
  private static Command _processSubRegions1Command()
  {
    var command = new Command("process-subregions1", "Process subregions1.  Use the persisted country codes to populate the sub-regions1.  Typically State/province of each country.");
    var refreshOption = command.AddOption<bool>("--refresh", "If false, existing data will be used", false, "-r");
    refreshOption.SetDefaultValue(false);

    command.SetHandler(async (refresh) =>
    {
      try
      {
        await BirdNamesFx.ProcessSubRegions1(refresh, ServiceProvider!);
      }
      catch (Exception e)
      {
        Console.WriteLine($"Failed to process the sub regions2 for country.  {e.Message}.\n\n{e}");
      }
    },refreshOption);


    return command;
  }
  private static Command _processSubRegions2Command()
  {
    var command = new Command("process-subregions2", "Process subregions2.  Use the persisted sub region 1 codes to populate the sub-regions2.  Typically County/suburbs of each sub region.");
    var refreshOption = command.AddOption<bool>("--refresh", "If false, existing data will be used", false, "-r");
    refreshOption.SetDefaultValue(false);

    command.SetHandler(async (refresh) =>
    {
      try
      {
        await BirdNamesFx.ProcessSubRegions2(refresh, ServiceProvider!);
      }
      catch (Exception e)
      {
        Console.WriteLine($"Failed to process the sub regions2 for country.  {e.Message}.\n\n{e}");
      }
    }, refreshOption);


    return command;
  }
  private static Command _processMajorRegions()
  {
    var command = new Command("process-major-regions", "Process major regions.  Use the persisted country codes to populate the major regions.  Typically Continent of each country.");
    var refreshOption = command.AddOption<bool>("--refresh", "If false, existing data will be used", false, "-r");

    command.SetHandler(async (refresh) =>
    {
      try
      {
        await BirdNamesFx.ProcessMajorRegions(refresh, ServiceProvider!);
      }
      catch (Exception e)
      {
        Console.WriteLine($"Failed to process the major regions.  {e.Message}.\n\n{e}");
      }
    }, refreshOption);

    return command;
  }
  private static Command _processSubRegions1Species()
  {
    var command = new Command("process-species-subregion1", "Fetch all the species codes for each persisted sub region 1.  Typically these are provinces or states in a country");
    var refreshOption = command.AddOption<bool>("--refresh", "If false, existing data will be used", false, "-r");

    command.SetHandler(async (refresh) =>
    {
      try
      {
        await BirdNamesFx.ProcessSubRegions1Species(refresh, ServiceProvider!);
      }
      catch (Exception e)
      {
        Console.WriteLine($"Failed to process the sub region 1 species codes.  {e.Message}.\n\n{e}");
      }
    }, refreshOption);

    return command;
  }
  private static Command _processUniqueSpecies()
  {
    var command = new Command("process-unique-species", "Fetch all the unique species codes from all the persisted sub region 1 (provinces or states).  Get the species info from https://api.ebird.org/v2/ref/taxonomy/ebird");
    var refreshOption = command.AddOption<bool>("--refresh", "If false, existing data will be used", false, "-r");

    command.SetHandler(async (refresh) =>
    {
      try
      {
        await BirdNamesFx.ProcessUniqueSpeciesInfo(refresh, ServiceProvider!);
      }
      catch (Exception e)
      {
        Console.WriteLine($"Failed to process the unique species.  {e.Message}.\n\n{e}");
      }
    }, refreshOption);

    return command;
  }
  private static Command _processVerifyPersistedSpecies()
  {
    var command = new Command("process-verify-species", "Verify the persisted species in the database against the persisted unique species codes for the sub region 1 documents");
    var refreshOption = command.AddOption<bool>("--refresh", "If false, the codes will only be printed", false, "-r");

    command.SetHandler(async (refresh) =>
    {
      try
      {
        await BirdNamesFx.VerifyAllUniqueSpecies(refresh, ServiceProvider!);
      }
      catch (Exception e)
      {
        Console.WriteLine($"Failed to verify unique species.  {e.Message}.\n\n{e}");
      }
    }, refreshOption);

    return command;
  }
  private static Command _downloadKeyWords()
  {
    var command = new Command("download-keywords", "Download the keywords from eBird by providing the list criteria");
    
    var pathOption = command.AddOption<string>("--path", "Path to save the keyword file.  Example: c:\\temp\\keywords.txt", true, "-p");
    var majorOption = command.AddOption<string>("--major-region", "Major region code.  Example: saf.  See options available https://ebird.org/explore", false, "-m");
    majorOption.SetDefaultValue(string.Empty);
    var countryOption = command.AddOption<string>("--country", "Country code.  Example: US", false, "-c");
    countryOption.SetDefaultValue(string.Empty);
    var subRegion1Option = command.AddOption<string>("--subregion1", "Sub region 1 code.  Example: US-CA", false, "-s");
    subRegion1Option.SetDefaultValue(string.Empty);
    
    var depthOption = command.AddOption<int>("--depth", "Depth of the keyword tree to download.  Default 15.  Example: 1 (Order only) or 15 (Order,Family,Genus and Species)", false, "-d");
    _defineFlagsOption(depthOption, EnumHelper.GetOrderedEnumValues<KeywordDepth>());

    var headerSynonymsOption = command.AddOption<int>("--header-synonyms", "Header Synonyms to add at the top of the keyword file.  Default 255.  Example 1 (Birds only) or 255 for all.", false, "-hs");
    _defineFlagsOption(headerSynonymsOption, EnumHelper.GetOrderedEnumValues<HeaderSynonym>());

    command.SetHandler(async (path, majorRegion, country, subRegion1, depth, headerSynonyms) =>
    {
      try
      {
        var criteria = new KeywordListCriteria(majorRegion)
        {
          Country = country,
          SubRegion1 = subRegion1,
          Depth = (KeywordDepth)depth,
          HeaderSynonyms = (HeaderSynonym)headerSynonyms
        };
        if (!criteria.IsValid())
          throw new ArgumentException("Invalid criteria.  At least 1 option must be provided (Major region, Country or Sub-region 1");

        var fileContent = await BirdNamesFx.DownloadKeywords(ServiceProvider!, criteria);
        await File.WriteAllBytesAsync(path, fileContent.ToArray());
      }
      catch (Exception e)
      {
        Console.WriteLine($"Failed to download keywords.  {e.Message}.\n\n{e}");
      }
    }, pathOption, majorOption, countryOption, subRegion1Option, depthOption, headerSynonymsOption);

    return command;
  }
  private static Command _tempWorkCommand()
  {
    var command = new Command("temp-work", "Debug tests")
    {
      IsHidden = true
    };

    command.SetHandler(async () =>
    {
      try
      {
        await BirdNamesFx.TempWork(ServiceProvider!);
      }
      catch (Exception e)
      {
        Console.WriteLine($"Temp work failed.  {e.Message}.\n\n{e}");
      }
    });

    return command;
  }

  private static void _defineFlagsOption(Option<int> depthOption, SortedList<int,string> completions)
  {
    depthOption.SetDefaultValue(completions.Keys.Sum());
    depthOption.AddCompletions(completions.Select(x => $"{x.Value} ({x.Key})").ToArray());
  }
 

}
