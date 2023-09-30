using System.Net;
using BirdNames.Core.Helpers;
using BirdNames.Core.Interfaces;
using BirdNames.Core.Models;
using BirdNames.Dal.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BirdNames.Blazor.Controllers;
[Route("api/[controller]")]
[ApiController]
public class BirdNamesController : ControllerBase
{
  private readonly IEBirdService _eBirdService;
  private readonly ILogger<BirdNamesController> _logger;
  private readonly IRepository<EBirdMajorRegion> _majorRegionsRepository;
  private readonly IRepository<EBirdCountry> _countriesRepository;
  private readonly IRepository<EBirdSubRegion1> _subregion1Repository;

  public BirdNamesController(
    IEBirdService eBirdService, 
    ILogger<BirdNamesController> logger,
    IRepository<EBirdMajorRegion> majorRegionsRepository,
    IRepository<EBirdCountry> countriesRepository,
    IRepository<EBirdSubRegion1> subregion1Repository)
  {
    _eBirdService = eBirdService;
    _logger = logger;
    _majorRegionsRepository = majorRegionsRepository;
    _countriesRepository = countriesRepository;
    _subregion1Repository = subregion1Repository;
  }

  [HttpPost]
  [Route("download/{asZip?}")]
  public async Task<FileResult> DownloadKeywordList([FromBody] KeywordListCriteria criteria, bool asZip=true)
  {
    var fileName = FileHelpers.GetDownloadFileName(isZip: asZip);
    var stream =  await _eBirdService.DownloadKeywords(criteria, CancellationToken.None);
    
    if (!asZip) 
      return File(stream!, "text/plain", fileName);

    var zippedFileData = FileHelpers.CreateZip(stream!, fileName);
    return File(zippedFileData, "application/zip", fileName);
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="nameContains"></param>
  /// <returns></returns>
  [HttpGet]
  [Route("majorregions/{nameContains?}")]
  public IEnumerable<CodeAndName> GetMajorRegions(string? nameContains=null)
  {
    return string.IsNullOrWhiteSpace(nameContains) ? 
      _majorRegionsRepository.AsQueryable().Select(x=>new CodeAndName(x.Code,x.Name)) : 
      _majorRegionsRepository.AsQueryable().Where(x => x.Name.Contains(nameContains)).Select(x=>new CodeAndName(x.Code,x.Name));
  }

  [HttpGet]
  [Route("countries/{nameContains?}")]
  public IEnumerable<CodeAndName> GetCountries(string? nameContains = null)
  {
    return string.IsNullOrWhiteSpace(nameContains) ? 
      _countriesRepository.AsQueryable().Select(x=>new CodeAndName(x.Code,x.Name)) : 
      _countriesRepository.AsQueryable().Where(x => x.Name.Contains(nameContains)).Select(x=>new CodeAndName(x.Code,x.Name));
  }

  [HttpGet]
  [Route("subregion1/{nameContains?}")]
  public IEnumerable<CodeAndName> GetSubRegion1(string? nameContains = null)
  {
    return string.IsNullOrWhiteSpace(nameContains) ? 
      _subregion1Repository.AsQueryable().Select(x=>new CodeAndName(x.Code,x.Name)) : 
      _subregion1Repository.AsQueryable().Where(x => x.Name.Contains(nameContains)).Select(x=>new CodeAndName(x.Code,x.Name));
  }
}
