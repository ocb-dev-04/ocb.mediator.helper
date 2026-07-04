namespace OCB.Mediator.Helper.ResultPattern;

/// <summary>
/// Represents a validation failure as an <see cref="Error"/>, carrying the per-property validation messages.
/// </summary>
/// <remarks>Returned by the built-in validation pipeline behavior when one or more validators fail, instead of
/// throwing an exception. Property names are exposed in camelCase, each mapped to its distinct error
/// messages.</remarks>
public sealed record ValidationError : Error
{
    /// <summary>
    /// <see cref="ValidationError"/> public constructor
    /// </summary>
    /// <param name="errors">The validation errors, keyed by camelCase property name. Cannot be <see langword="null"/>.</param>
    public ValidationError(IReadOnlyDictionary<string, string[]> errors)
        : base(400, "validationError", "One or more validation errors occurred")
        => Errors = errors ?? throw new ArgumentNullException(nameof(errors));

    /// <summary>
    /// Gets the validation errors, keyed by camelCase property name.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
