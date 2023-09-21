using FluentValidation;

namespace BirdNames.Core.Models.Validators;

public class SpeciesValidator : SubSpeciesValidator<BirdNamesSpecies>
{
  public SpeciesValidator()
  {
    RuleFor(x => x.Name)
      .NotEmpty();
    RuleFor(x => x.BreedingRegions)
      .NotEmpty();
    RuleFor(x => x.GenusLatin)
      .NotEmpty();
  }
}