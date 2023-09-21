using FluentValidation;

namespace BirdNames.Core.Models.Validators;

public class SubRegion1Validator : CodeAndNameValidator<EBirdSubRegion1>
{
  public SubRegion1Validator()
  {
    RuleFor(x => x.Country)
      .NotEmpty();
  }
}