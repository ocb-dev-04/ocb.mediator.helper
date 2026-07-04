using Microsoft.AspNetCore.Mvc;
using OCB.Mediator.Helper.ResultPattern;
using OCB.Mediator.Helper.Abstractions.Sender;

namespace Web.Api.Controllers;

/// <summary>
/// Base controller for all controllers in the application.
/// </summary>
public abstract class BaseController : ControllerBase
{
    protected readonly ISender _sender;

    /// <summary>
    /// <see cref="BaseController"/> protected constructor.
    /// </summary>
    /// <param name="sender"></param>
    /// <exception cref="ArgumentNullException"></exception>
    protected BaseController(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    /// <summary>
    /// Handles error results based on the provided <see cref="Error"/> object.
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    protected IActionResult HandleErrorResults(Error error)
        => error switch
        {
            ValidationError validation
                => BadRequest(new { validation.Translation, validation.Description, validation.Errors }),
            _ => error.StatusCode switch
            {
                304 => StatusCode(error.StatusCode, new { error.Translation, error.Description }),
                400 => BadRequest(new { error.Translation, error.Description }),
                401 => Unauthorized(),
                404 => NotFound(new { error.Translation, error.Description }),
                _ => StatusCode(error.StatusCode)
            }
        };
}
