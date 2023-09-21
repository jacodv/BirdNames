using System.CommandLine;

namespace BirdNames.Tool.Helpers;
public static class CommandHelpers
{
  public static Option<T> AddOption<T>(this Command command, string name, string description, bool isRequired = false, string? alias = null, Action<T?>? validate = null, IList<string>? suggestions = null)
  {
    var option = _createOption<T>(name, description, isRequired, alias);
    if (validate != null)
    {
      option.AddValidator(result =>
      {
        validate(result.GetValueForOption(option));
      });
    }

    if (suggestions != null)
      option.AddCompletions(suggestions.ToArray());

    command.AddOption(option);
    return option;
  }
  public static Option<T> AddGlobalOption<T>(this RootCommand command, string name, string description, bool isRequired = false, string? alias = null, Action<T?>? validate = null, IList<string>? suggestions = null)
  {
    var option = _createOption<T>(name, description, isRequired, alias);
    if (validate != null)
    {
      option.AddValidator(result =>
      {
        validate(result.GetValueForOption(option));
      });
    }
    if (suggestions != null)
      option.AddCompletions(suggestions.ToArray());

    command.AddGlobalOption(option);
    return option;
  }

  public static Option<string> AddSourceFileOption(this Command command, string? description=null, bool isRequired=true)
  {
    return command.AddOption<string>("--source-file", description??"The source file that will be processed", isRequired, "-s");
  }

  #region Private
  private static Option<T> _createOption<T>(string name, string description, bool isRequired, string? alias)
  {
    var option = new Option<T>(name, description);
    if (isRequired)
    {
      option.IsRequired = true;
    }

    if (alias != null)
    {
      option.AddAlias(alias);
    }

    return option;
  }
  #endregion
}
