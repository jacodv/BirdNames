using FluentValidation;

namespace BirdNames.Core.Models.Validators;

public class EBirdCountryValidator : CodeAndNameValidator<EBirdCountry>
{
  public EBirdCountryValidator()
  {
    RuleFor(x => x.Continent)
      .NotEmpty();
  }
}