using FluentValidation;

namespace BirdNames.Core.Models.Validators;

public class EBirdSubRegion2Validator : CodeAndNameValidator<EBirdSubRegion2>
{
  public EBirdSubRegion2Validator()
  {
    RuleFor(x => x.SubRegion1)
      .NotEmpty();
    RuleFor(x => x.Country)
      .NotEmpty();
  }
}