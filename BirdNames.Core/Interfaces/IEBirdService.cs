using BirdNames.Core.Models;
using BirdNames.Core.Settings;

namespace BirdNames.Core.Interfaces;

public interface IEBirdService
{
  Task ProcessSubRegion1(bool refresh,CancellationToken token=default);
  Task ProcessSubRegion2(bool refresh,CancellationToken token=default);
  Task ProcessMajorRegions(bool refresh,CancellationToken token=default);
  Task ProcessSubRegion1Species(bool refresh, CancellationToken token = default);
  Task ProcessSubRegion1SpeciesInfo(bool refresh, CancellationToken token = default);
  Task VerifyAllUniqueSpecies(bool refresh, CancellationToken token = default);
  Task<MemoryStream?> DownloadKeywords(KeywordListCriteria criteria, CancellationToken token = default);
  Task TempWork();
  bool IsValid();
  Task<bool> ValidateSettings(BirdNamesCoreSettings settings);
}