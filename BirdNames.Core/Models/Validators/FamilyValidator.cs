using FluentValidation;

namespace BirdNames.Core.Models.Validators;

public class FamilyValidator : ModelVersionBaseValidator<BirdNamesFamily>
{
  public FamilyValidator()
  {
    RuleFor(x => x.Latin)
      .NotEmpty();
    RuleFor(x => x.Name)
      .NotEmpty();
    RuleFor(x => x.OrderCode)
      .NotEmpty();
  }
}