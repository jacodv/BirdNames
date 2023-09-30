using BirdNames.Core.Interfaces;
using BirdNames.Core.Services;
using BirdNames.Core.Settings;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Options;
using Radzen;

namespace BirdNames.Blazor.Pages;

public class AdminPageBase : ComponentBase, IDisposable
{
  private const int MaxFileSize = 1024 * 1024 * 10;//10MB;
  private const string XmlTempFile = "temp/ioc.xml";
  private const string CsvTempFile = "temp/countries.csv";

  [Inject] protected IWebHostEnvironment Environment { get; set; } = null!;
  [Inject] protected IBirdNamesXmlProcessorService BirdNamesXmlProcessorService { get; set; } = null!;
  [Inject] protected ICountryService CountryService { get; set; } = null!;
  [Inject] protected ISettingsService SettingsService { get; set; } = null!;
  [Inject] protected ILogger<AdminPageBase> Logger { get; set; } = null!;
  [Inject] protected NotificationService NotificationService { get; set; } = null!;
  [Inject] protected IOptions<BirdNamesCoreSettings> CoreSettings { get; set; } = null!;
  [Inject] protected IEBirdService EBirdService { get; set; } = null!;

  protected bool IsLoggedIn { get; private set; }
  protected string? AdminPassword { get; set; }
  protected bool RefreshMajorRegions { get; set; }
  protected bool RefreshSubRegion1 { get; set; }
  protected bool RefreshSubRegion2 { get; set; }
  protected bool RefreshSubRegion1Species { get; set; }
  protected bool RefreshUniqSpecies { get; set; }
  protected bool RefreshAllSpeciesVerify { get; set; }


  protected void Login()
  {
    IsLoggedIn = false;
    var password = SettingsService.Unprotect(CoreSettings.Value.AdminPassword!);
    if (password == AdminPassword)
    {
      IsLoggedIn = true;
      AdminPassword = string.Empty;
    }
    else
    {
      NotificationService.Notify(NotificationSeverity.Error, "Login Error", "Invalid password");
    }
  }
  protected async Task OnXmlFileUploadChanged(InputFileChangeEventArgs args)
  {
    var tempXmlFilePath = _getTempXmlFilePath();
    try
    {
      await _saveUploadedFile(args, tempXmlFilePath, "text/xml");
    }
    catch (Exception e)
    {
      Logger.LogError(e, $"Xml File Upload Error: {e.Message}");
      NotificationService.Notify(NotificationSeverity.Error, "Upload Error", e.Message);
    }
  }
  protected async Task OnCsvFileUploadChanged(InputFileChangeEventArgs args)
  {
    var tempCsvFilePath = _getTempCsvFilePath();
    try
    {
      await _saveUploadedFile(args, tempCsvFilePath, "text/csv");
    }
    catch (Exception e)
    {
      Logger.LogError(e, $"Csv File Upload Error: {e.Message}");
      NotificationService.Notify(NotificationSeverity.Error, "Upload Error", e.Message);
    }
  }
  protected async Task ProcessUploadedXml()
  {
    var tempXmlFilePath = _getTempXmlFilePath();
    try
    {
      if (!File.Exists(tempXmlFilePath))
        throw new FileNotFoundException("Xml file not found", tempXmlFilePath);

      await using var fileStream = File.OpenRead(tempXmlFilePath);
      await BirdNamesXmlProcessorService.ProcessXml(fileStream);
      fileStream.Close();
      NotificationService.Notify(NotificationSeverity.Success, "Process Complete", "IOC World Bird List uploaded and processed");
      await Task.Yield();
    }
    catch (Exception e)
    {
      Logger.LogError(e, $"Xml File Process Error: {e.Message}");
      NotificationService.Notify(NotificationSeverity.Error, "Process Error", e.Message);
      await Task.Yield();
    }
    finally
    {
      if (File.Exists(tempXmlFilePath))
        File.Delete(tempXmlFilePath);
    }
  }
  protected async Task ProcessUploadedCsv()
  {
    var tempCsvFilePath = _getTempCsvFilePath();
    try
    {
      if (!File.Exists(tempCsvFilePath))
        throw new FileNotFoundException("Csv file not found", tempCsvFilePath);

      await using var fileStream = File.OpenRead(tempCsvFilePath);
      await CountryService.PersistCountries(fileStream);
      fileStream.Close();
      NotificationService.Notify(NotificationSeverity.Success, "Process Complete", "Countries uploaded and processed");
      await Task.Yield();
    }
    catch (Exception e)
    {
      Logger.LogError(e, $"Csv File Process Error: {e.Message}");
      NotificationService.Notify(NotificationSeverity.Error, "Process Error", e.Message);
      await Task.Yield();
    }
    finally
    {
      if (File.Exists(tempCsvFilePath))
        File.Delete(tempCsvFilePath);
    }
  }
  protected async Task ProcessSubRegions(bool subRegion1, bool refresh)
  {
    try
    {
      if(subRegion1)
        await EBirdService.ProcessSubRegion1(refresh);
      else
        await EBirdService.ProcessSubRegion2(refresh);
    }
    catch (Exception e)
    {
      Logger.LogError(e, $"Sub-region error: {e.Message}");
      NotificationService.Notify(NotificationSeverity.Error, "Sub-region error", e.Message);
    }
  }
  protected async Task ProcessMajorRegions(bool refresh)
  {
    try
    {
      await EBirdService.ProcessMajorRegions(refresh);
    }
    catch (Exception e)
    {
      Logger.LogError(e, $"Major region error: {e.Message}");
      NotificationService.Notify(NotificationSeverity.Error, "Major region error", e.Message);
    }
  }

  protected async Task ProcessSubRegions1Species(bool refresh)
  {
    try
    {
      await EBirdService.ProcessSubRegion1Species(refresh);
    }
    catch (Exception e)
    {
      Logger.LogError(e, $"Sub-region 1 species error: {e.Message}");
      NotificationService.Notify(NotificationSeverity.Error, "Sub-region 1 species error", e.Message);
    }
  }
  protected async Task ProcessUniqueSpeciesInfo(bool refresh)
  {
    try
    {
      await EBirdService.ProcessSubRegion1SpeciesInfo(refresh);
    }
    catch (Exception e)
    {
      Logger.LogError(e, $"Unique species info error: {e.Message}");
      NotificationService.Notify(NotificationSeverity.Error, "Unique species info error", e.Message);
    }
  }
  protected async Task VerifyAllUniqueSpecies(bool refresh)
  {
    try
    {
      await EBirdService.VerifyAllUniqueSpecies(refresh);
    }
    catch (Exception e)
    {
      Logger.LogError(e, $"Verify all unique species error: {e.Message}");
      NotificationService.Notify(NotificationSeverity.Error, "Verify all unique species error", e.Message);
    }
  }

  #region Private
  private string _getTempXmlFilePath()
  {
    return System.IO.Path.Combine(System.IO.Path.Combine(Environment.ContentRootPath, XmlTempFile));
  }
  private string _getTempCsvFilePath()
  {
    return System.IO.Path.Combine(System.IO.Path.Combine(Environment.ContentRootPath, CsvTempFile));
  }
  private async Task _saveUploadedFile(InputFileChangeEventArgs args, string tempFilePath, string expectedContentType)
  {
    if (args.File.Size > MaxFileSize)
      throw new InvalidOperationException($"File size too large: {args.File.Size}>{MaxFileSize}");

    if (args.File.ContentType != expectedContentType)
      throw new InvalidOperationException($"Invalid file type: {args.File.ContentType}");

    await using var file = File.Create(tempFilePath);
    await args.File.OpenReadStream(int.MaxValue).CopyToAsync(file);
    file.Close();

    await InvokeAsync(StateHasChanged);
    await Task.Yield();
    NotificationService.Notify(NotificationSeverity.Info, "File Uploaded",
      $"File: {args.File.Name}\nSize: {args.File.Size}\nType: {args.File.ContentType}");
  }
  #endregion


  public void Dispose()
  {
    BirdNamesXmlProcessorService?.Dispose();
  }
}
