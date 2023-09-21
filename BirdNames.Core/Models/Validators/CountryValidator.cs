using FluentValidation;

namespace BirdNames.Core.Models.Validators;

public class CountryValidator : CodeAndNameValidator<EBirdCountry>
{
  public CountryValidator()
  {
    RuleFor(x => x.Continent)
      .NotEmpty();
  }
}