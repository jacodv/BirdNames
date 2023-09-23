using FluentValidation;

namespace BirdNames.Core.Models.Validators;

public class EBirdSpeciesValidator : CodeAndNameValidator<EBirdSpecies>
{
  public EBirdSpeciesValidator()
  {
    RuleFor(x => x.SciName).NotEmpty();
    RuleFor(x=>x.Order).NotEmpty();
    
    When(x => x.Category != "hybrid", () =>
    {
      RuleFor(x => x.FamilyComName).NotEmpty();
      RuleFor(x=>x.FamilySciName).NotEmpty();
    });
  }
}