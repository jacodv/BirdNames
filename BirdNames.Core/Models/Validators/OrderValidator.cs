using FluentValidation;

namespace BirdNames.Core.Models.Validators;

public class OrderValidator : ModelVersionBaseValidator<BirdNamesOrder>
{
  public OrderValidator()
  {
    RuleFor(x => x.Code)
      .NotEmpty();
    RuleFor(x => x.Latin)
      .NotEmpty();
  }
}