using OCB.Mediator.Helper.ResultPattern;

namespace OCB.Mediator.Helper.Extensions;

/// <summary>
/// Provides extension methods for handling <see cref="Result"/> and <see cref="Result{TValue}"/> instances in a fluent
/// and functional manner.
/// </summary>
/// <remarks>The <see cref="FluentResultsExtensions"/> class includes methods that simplify the evaluation of
/// result states (success or error) and allow developers to execute specific logic based on the state of the result.
/// These methods are designed to improve readability and reduce boilerplate code when working with result
/// objects.</remarks>
public static class FluentResultsExtensions
{
    /// <summary>
    /// Evaluates the result and executes the appropriate function based on its state.
    /// </summary>
    /// <remarks>This method provides a convenient way to handle both success and error states of a <see
    /// cref="Result{TValue}"/> by delegating the logic to the provided functions. The caller is responsible for
    /// ensuring that the functions handle the respective states appropriately.</remarks>
    /// <typeparam name="TReturnType">The type of the value returned by the <paramref name="success"/> or <paramref name="error"/> function.</typeparam>
    /// <typeparam name="TValue">The type of the value contained in the <see cref="Result{TValue}"/> if the operation is successful.</typeparam>
    /// <param name="result">The result to evaluate. Must not be null.</param>
    /// <param name="success">A function to execute if the result represents a successful operation. Receives the value of the result as
    /// input.</param>
    /// <param name="error">A function to execute if the result represents a failed operation. Receives the error associated with the result
    /// as input.</param>
    /// <returns>The value returned by either the <paramref name="success"/> or <paramref name="error"/> function, depending on
    /// the state of the result.</returns>
    public static TReturnType Match<TReturnType, TValue>(
        this Result<TValue> result,
        Func<object, TReturnType> success,
        Func<Error, TReturnType> error)
            where TValue : notnull
            where TReturnType : notnull
                => result.IsSuccess
                    ? success(result.GetType().Equals(typeof(Unit)) ? string.Empty : result.Value)
                    : error(result.Error);

    /// <summary>
    /// Evaluates the current state of the <see cref="Result"/> and executes the appropriate function  based on whether
    /// the operation was successful or encountered an error.
    /// </summary>
    /// <typeparam name="TReturnType">The type of the value returned by the <paramref name="success"/> or <paramref name="error"/> function. Must be
    /// non-nullable.</typeparam>
    /// <param name="result">The <see cref="Result"/> instance to evaluate.</param>
    /// <param name="success">A function to execute if the <paramref name="result"/> represents a successful operation.</param>
    /// <param name="error">A function to execute if the <paramref name="result"/> represents a failed operation.  The <see cref="Error"/>
    /// associated with the failure is passed as an argument.</param>
    /// <returns>The value returned by either the <paramref name="success"/> or <paramref name="error"/> function,  depending on
    /// the state of the <paramref name="result"/>.</returns>
    public static TReturnType Match<TReturnType>(
        this Result result,
        Func<TReturnType> success,
        Func<Error, TReturnType> error)
            where TReturnType : notnull
                => result.IsSuccess
                    ? success()
                    : error(result.Error);
}