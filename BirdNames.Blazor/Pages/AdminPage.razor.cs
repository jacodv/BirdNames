using BirdNames.Core.Interfaces;
using BirdNames.Core.Services;
using BirdNames.Core.Settings;
using BirdNames.Core.Xml;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Options;
using Radzen;

namespace BirdNames.Blazor.Pages;

public class AdminPageBase : ComponentBase, IDisposable
{
  private const int MaxFileSize = 1024 * 1024 * 10;//10MB;
  private const string XmlTempFile = "temp/ioc.xml";
  
  [Inject] protected IWebHostEnvironment Environment { get; set; } = null!;
  [Inject] protected IBirdNamesXmlProcessorService BirdNamesXmlProcessorService { get; set; } = null!;
  [Inject] protected ICountryService CountryService { get; set; } = null!;
  [Inject] protected ISettingsService SettingsService { get; set; } = null!;
  [Inject] protected ILogger<AdminPageBase> Logger { get; set; } = null!;
  [Inject] protected NotificationService NotificationService { get; set; } = null!;
  [Inject] protected IOptions<BirdNamesCoreSettings> CoreSettings { get; set; } = null!;

  protected bool AllowXmlUpload { get; private set; }
  protected bool IsLoggedIn { get; private set; }
  protected string AdminPassword { get; set; } 

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
  protected async Task OnFileUploadChanged(InputFileChangeEventArgs args)
  {
    AllowXmlUpload = false;
    var tempXmlFilePath = _getTempXmlFilePath();

    try
    {
      if (args.File.Size > MaxFileSize)
        throw new InvalidOperationException($"File size too large: {args.File.Size}>{MaxFileSize}");

      if (args.File.ContentType != "text/xml")
        throw new InvalidOperationException($"Invalid file type: {args.File.ContentType}");

      await using var file = File.Create(tempXmlFilePath);
      await args.File.OpenReadStream(int.MaxValue).CopyToAsync(file);
      file.Close();

      AllowXmlUpload = true;
      await InvokeAsync(StateHasChanged);
      await Task.Yield();
      NotificationService.Notify(NotificationSeverity.Info, "File Uploaded", $"File: {args.File.Name}\nSize: {args.File.Size}\nType: {args.File.ContentType}");
    }
    catch (Exception e)
    {
      Logger.LogError(e, $"Xml File Upload Error: {e.Message}");
      AllowXmlUpload = false;
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
  
  #region Private
  private string _getTempXmlFilePath()
  {
    return Path.Combine(Path.Combine(Environment.ContentRootPath, XmlTempFile));
  }
  #endregion


  public void Dispose()
  {
    BirdNamesXmlProcessorService?.Dispose();
  }
}
