using BirdNames.Core.Models;
using BirdNames.Core.Settings;
using BirdNames.Core.StartUp;
using FluentValidation;
using Radzen;

namespace BirdNames.Blazor;
public class Program
{
  public static IServiceProvider? ServiceProvider { get; set; }
  public static void Main(string[] args)
  {
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddRazorPages();
    builder.Services.AddServerSideBlazor();

    builder.Services
      .Configure<DatabaseSettings>(settings =>
      {
        builder.Configuration.GetSection(nameof(DatabaseSettings)).Bind(settings);
      })
      .Configure<BirdNamesCoreSettings>(settings =>
      {
        builder.Configuration.GetSection(nameof(BirdNamesCoreSettings)).Bind(settings);
      })
      .AddOptions();

    builder.Services.AddValidatorsFromAssemblyContaining<ModelVersionBaseValidator<BirdNamesOrder>>();

    builder.Services.SetupBirdNamesCore();
    builder.Services.AddRadzenComponents();

    var app = builder.Build();

    ServiceProvider = app.Services;

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
      app.UseExceptionHandler("/Error");
      // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
      app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.MapBlazorHub();
    app.MapFallbackToPage("/_Host");

    app.Run();
  }
}
