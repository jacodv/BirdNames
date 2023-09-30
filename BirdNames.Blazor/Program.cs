using System.Net.Mime;
using BirdNames.Blazor.GraphQL;
using BirdNames.Core.Models;
using BirdNames.Core.Settings;
using BirdNames.Core.StartUp;
using BirdNames.Dal.Interfaces;
using FluentValidation;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using Radzen;
using Serilog;

namespace BirdNames.Blazor;
public class Program
{
  public static IServiceProvider? ServiceProvider { get; set; }
  public static IConfiguration? Configuration { get; set; }
  public static Serilog.Core.Logger? Logger { get; set; }
  public static bool ShowAdminMenu { get; set; } = false;

  public static void Main(string[] args)
  {
    var builder = WebApplication.CreateBuilder(args);
    var env = builder.Environment;

    Configuration = new ConfigurationBuilder()
      .SetBasePath(AppContext.BaseDirectory!)
      .AddJsonFile("appsettings.json", true, true)
      .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
      .AddEnvironmentVariables()
      .Build();

    Logger = new LoggerConfiguration()
      .ReadFrom.Configuration(Configuration)
      .CreateLogger();

    Logger.Information($"Created Logger: {env.EnvironmentName}");
    ShowAdminMenu = Configuration.GetValue<bool>(nameof(ShowAdminMenu));

    builder.Host.UseSerilog(Logger);


    // Add services to the container.
    builder.Services.AddRazorPages();
    builder.Services.AddServerSideBlazor();

    builder.Services
      .Configure<DatabaseSettings>(settings =>
      {
        Configuration.GetSection(nameof(DatabaseSettings)).Bind(settings);
      })
      .Configure<BirdNamesCoreSettings>(settings =>
      {
        Configuration.GetSection(nameof(BirdNamesCoreSettings)).Bind(settings);
      })
      .AddOptions();

    builder.Services.AddValidatorsFromAssemblyContaining<ModelVersionBaseValidator<BirdNamesOrder>>();

    builder.Services.AddTransient(ctx =>
      ctx.GetService<IRepository<EBirdMajorRegion>>()!.GetCollection());
    builder.Services.AddTransient(ctx =>
      ctx.GetService<IRepository<EBirdCountry>>()!.GetCollection());
    builder.Services.AddTransient(ctx =>
      ctx.GetService<IRepository<EBirdSubRegion1>>()!.GetCollection());

    builder.Services.SetupBirdNamesCore();
    builder.Services.AddRadzenComponents();
    builder.Services.ConfigureGraphQl();
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
      c.SwaggerDoc("v1", new OpenApiInfo { Title = "BirdNames API", Version = "v1" });
    });

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
    
    app.MapControllers();
    
    app.MapGraphQL();
    
    app.MapSwagger();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
      c.SwaggerEndpoint("/swagger/v1/swagger.json", "BirdNames API V1");
    });

    app.MapBlazorHub();
    app.MapFallbackToPage("/{param?}", "/_Host");
    app.MapFallbackToPage("/_Host");
    app.Run();
  }
}
