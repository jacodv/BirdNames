using FluentValidation;

namespace BirdNames.Core.Models.Validators;

public class GenusValidator : ModelVersionBaseValidator<BirdNamesGenus>
{
  public GenusValidator()
  {
    RuleFor(x => x.Latin)
      .NotEmpty();
    RuleFor(x => x.FamilyName)
      .NotEmpty();
  }
}