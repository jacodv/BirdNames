using FluentValidation;

namespace BirdNames.Core.Models.Validators;

public class SubRegion2Validator : CodeAndNameValidator<EBirdSubRegion2>
{
  public SubRegion2Validator()
  {
    RuleFor(x => x.SubRegion1)
      .NotEmpty();
    RuleFor(x => x.Country)
      .NotEmpty();
  }
}