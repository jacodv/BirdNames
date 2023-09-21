using FluentValidation;

namespace BirdNames.Core.Models.Validators;

public class EBirdSubRegion1Validator : CodeAndNameValidator<EBirdSubRegion1>
{
  public EBirdSubRegion1Validator()
  {
    RuleFor(x => x.Country)
      .NotEmpty();
  }
}