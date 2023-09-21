using BirdNames.Core.Models;
using BirdNames.Dal.Interfaces;

namespace BirdNames.Core.Services;
public class LightRoomKeywordsService: ILightRoomKeywordsService
{
  private readonly IRepository<BirdNamesOrder> _orderRepository;
  private readonly IRepository<BirdNamesFamily> _familyRepository;
  private readonly IRepository<BirdNamesGenus> _genusRepository;
  private readonly IRepository<BirdNamesSpecies> _speciesRepository;

  public LightRoomKeywordsService(
    IRepository<BirdNamesOrder> orderRepository,
    IRepository<BirdNamesFamily> familyRepository,
    IRepository<BirdNamesGenus> genusRepository,
    IRepository<BirdNamesSpecies> speciesRepository)
  {
    _orderRepository = orderRepository;
    _familyRepository = familyRepository;
    _genusRepository = genusRepository;
    _speciesRepository = speciesRepository;
  }
}

public interface ILightRoomKeywordsService
{
}
