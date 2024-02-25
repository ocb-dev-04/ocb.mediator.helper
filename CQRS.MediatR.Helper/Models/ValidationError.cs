namespace CQRS.MediatR.Helper.Models;

/// <summary>
/// Model to create validation errors objects
/// </summary>
/// <param name="PropertyName"></param>
/// <param name="ErrorMessage"></param>
/// <returns></returns>
public sealed record ValidationError(string PropertyName, string ErrorMessage);