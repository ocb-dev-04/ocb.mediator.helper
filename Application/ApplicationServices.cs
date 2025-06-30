using OCB.Mediator.Helper;
using Application.Behaviors.EventPipelines;
using Application.Behaviors.HandlerPipelines;
using OCB.Mediator.Helper.Behaviors.Pipelines;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

/// <summary>
/// Configures and registers application-specific services and behaviors into the provided service collection.
/// </summary>
/// <remarks>This method adds mediator helper services, pipeline behaviors, notification pipeline behaviors, and
/// validators to the service collection. It is designed to streamline the setup of application-level dependencies and
/// behaviors.  The following components are registered: <list type="bullet"> <item><description>Mediator helper
/// services from the assembly containing <see cref="ApplicationServices"/>.</description></item>
/// <item><description>Pipeline behaviors for logging and validation.</description></item>
/// <item><description>Notification pipeline behavior for exception handling.</description></item>
/// <item><description>Validators from the assembly containing <see cref="ApplicationServices"/>.</description></item>
/// </list></remarks>
public static class ApplicationServices
{
    /// <summary>
    /// Configures and registers application-specific services and behaviors into the provided service collection.
    /// </summary>
    /// <remarks>This method adds mediator helper services, pipeline behaviors, notification pipeline
    /// behaviors, and validators from the specified assembly to the service collection. It is intended to streamline
    /// the setup of application-level dependencies and middleware for handling requests, validations, logging, and
    /// exception handling.</remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the application services will be added.</param>
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatorHelperServices(typeof(ApplicationServices).Assembly)
            .AddValidators(typeof(ApplicationServices).Assembly, true);

        services
            .AddPipelineBehavior(typeof(LoggerPipelineBehavior<,>))
            .AddPipelineBehavior(typeof(ValidationPipelineBehavior<,>))
            .AddNotificationPipelineBehavior(typeof(ExceptionHandlingNotificationPipelineBehavior<>));
    }
}
