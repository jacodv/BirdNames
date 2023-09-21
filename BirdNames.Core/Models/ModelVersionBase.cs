using BirdNames.Dal.Interfaces;
using FluentValidation;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BirdNames.Core.Models;

public abstract class ModelBase : IDocument
{
  [BsonId()]
  public ObjectId Id { get; set; }
  public DateTime CreatedAt { get; } = DateTime.Now;
}

public abstract class ModelVersionBase : ModelBase
{
  protected ModelVersionBase(string version, int year)
  {

    Version = version;
    Year = year;
  }
  
  #region Implementation of IDocument

  public string Version { get; set; }
  public int Year { get; set; }

  #endregion
}

public abstract class ModelVersionBaseValidator<T> : AbstractValidator<T>
where T : ModelVersionBase
{
  protected ModelVersionBaseValidator()
  {
    RuleFor(x => x.Version)
      .NotEmpty();
    RuleFor(x => x.Year)
      .NotEmpty();
  }
}

public abstract class CodeAndNameBase : ModelBase
{
  protected CodeAndNameBase(string code, string name){ 
    Code = code;
    Name = name;
  }

  public string Code { get; set; }
  public string Name { get; set; }

  public override string ToString()
  {
    return $"{Code}|{Name}";
  }
}

public abstract class CodeAndNameValidator<T> : AbstractValidator<T>
where T : CodeAndNameBase
{
  protected CodeAndNameValidator()
  {
    RuleFor(x => x.Code)
      .NotEmpty();
    RuleFor(x => x.Name)
      .NotEmpty();

  }
}
