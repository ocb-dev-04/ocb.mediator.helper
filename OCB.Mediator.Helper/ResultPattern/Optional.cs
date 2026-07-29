namespace OCB.Mediator.Helper.ResultPattern;

/// <summary>
/// Marker type returned by <see cref="Optional.None"/>. Implicitly convertible to <see
/// cref="Optional{T}"/> for any <c>T</c>, so a call site never needs to specify a type argument
/// (e.g. <c>Employment = Optional.None()</c>).
/// </summary>
public readonly struct OptionalNone
{
}

/// <summary>
/// Non-generic factory helpers for <see cref="Optional{T}"/>, mirroring the ergonomic
/// <see cref="Result.Success{TValue}(TValue)"/>/<see cref="Result.Failure{TValue}(Error)"/> API
/// already established by the <see cref="Result"/> pattern.
/// </summary>
public static class Optional
{
    /// <summary>
    /// Creates a marker representing the absence of a value. Implicitly converts to <see
    /// cref="Optional{T}"/> for whatever <c>T</c> the call site requires.
    /// </summary>
    /// <returns>An <see cref="OptionalNone"/> marker.</returns>
    public static OptionalNone None() => default;

    /// <summary>
    /// Creates an <see cref="Optional{T}"/> instance wrapping the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the value to wrap.</typeparam>
    /// <param name="value">The value to wrap. Cannot be <see langword="null"/>.</param>
    /// <returns>An <see cref="Optional{T}"/> instance representing a present value.</returns>
    public static Optional<T> Some<T>(T value) => Optional<T>.Some(value);
}

/// <summary>
/// Represents whether a response section carries a value, replacing a semantic <see
/// langword="null"/> with an explicit contract.
/// </summary>
/// <remarks>The <see cref="Optional{T}"/> class mirrors the <see cref="Result"/> pattern already used across the
/// solution: <see cref="HasValue"/> is always the source of truth, and instances are only created through <see
/// cref="Some(T)"/> or <see cref="None"/> — never via a public constructor.</remarks>
/// <typeparam name="T">The type of the wrapped value.</typeparam>
public sealed class Optional<T>
{
    /// <summary>
    /// Gets a value indicating whether this instance carries a value.
    /// </summary>
    public bool HasValue { get; }

    /// <summary>
    /// Gets the wrapped value. Only meaningful when <see cref="HasValue"/> is <see langword="true"/>; otherwise
    /// <see langword="default"/>.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Optional{T}"/> class.
    /// </summary>
    /// <param name="hasValue">Whether the instance carries a value.</param>
    /// <param name="value">The wrapped value, or <see langword="default"/> when <paramref name="hasValue"/> is <see
    /// langword="false"/>.</param>
    private Optional(bool hasValue, T? value)
    {
        HasValue = hasValue;
        Value = value;
    }

    /// <summary>
    /// Creates an <see cref="Optional{T}"/> instance representing the absence of a value.
    /// </summary>
    /// <returns>An <see cref="Optional{T}"/> instance with <see cref="HasValue"/> set to <see langword="false"/>.</returns>
    public static Optional<T> None() => new(false, default);

    /// <summary>
    /// Creates an <see cref="Optional{T}"/> instance wrapping the specified value.
    /// </summary>
    /// <param name="value">The value to wrap. Cannot be <see langword="null"/>.</param>
    /// <returns>An <see cref="Optional{T}"/> instance with <see cref="HasValue"/> set to <see langword="true"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static Optional<T> Some(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return new(true, value);
    }

    /// <summary>
    /// Implicitly converts a value of type <typeparamref name="T"/> to an <see cref="Optional{T}"/> instance.
    /// </summary>
    /// <param name="value">The value to be converted. Cannot be <see langword="null"/>.</param>
    public static implicit operator Optional<T>(T value) => Some(value);

    /// <summary>
    /// Implicitly converts the non-generic <see cref="OptionalNone"/> marker to an empty <see
    /// cref="Optional{T}"/> instance.
    /// </summary>
    /// <param name="_">The <see cref="OptionalNone"/> marker (ignored).</param>
    public static implicit operator Optional<T>(OptionalNone _) => None();
}
