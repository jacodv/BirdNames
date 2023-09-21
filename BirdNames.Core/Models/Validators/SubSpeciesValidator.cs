using FluentValidation;

namespace BirdNames.Core.Models.Validators;

public class SubSpeciesValidator<T> : ModelVersionBaseValidator<T>
where T : BirdNamesSubSpecies
{
  public SubSpeciesValidator()
  {
    RuleFor(x => x.Latin)
      .NotEmpty();
  }
}