namespace MediatR.Cqrs.Helper.Models;

public sealed record ValidationError(string PropertyName, string ErrorMessage);