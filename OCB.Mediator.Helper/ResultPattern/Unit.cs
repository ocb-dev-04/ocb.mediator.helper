namespace OCB.Mediator.Helper.ResultPattern;

/// <summary>
/// Represents a type that has a single value and no meaningful data.
/// </summary>
/// <remarks>The <see cref="Unit"/> type is commonly used in scenarios where a method or operation does not return
/// a meaningful result but still needs to indicate completion or success. It is similar to the concept of "void" but
/// can be used as a value type.</remarks>
public readonly struct Unit
{
    /// <summary>
    /// Represents a singleton instance of the <see cref="Unit"/> type.
    /// </summary>
    /// <remarks>The <see cref="Value"/> field provides a single, immutable instance of the <see cref="Unit"/>
    /// type,  commonly used to represent a void or no-result value in functional programming contexts.</remarks>
    public static readonly Unit Value = new Unit();

    /// <summary>
    /// Returns a string representation of the current instance.
    /// </summary>
    /// <returns>A string containing "()".</returns>
    public override string ToString() => "()";
}
