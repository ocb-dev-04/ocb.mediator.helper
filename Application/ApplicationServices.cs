using OCB.Mediator.Helper;
using Application.Behaviors.EventPipelines;
using Application.Behaviors.HandlerPipelines;
using OCB.Mediator.Helper.Behaviors.Pipelines;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

/// <summary>
/// Class to add all service into application layer
/// </summary>
public static class ApplicationServices
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatorHelperServices(typeof(ApplicationServices).Assembly)
            .AddPipelineBehavior(typeof(LoggerPipelineBehavior<,>))
            .AddPipelineBehavior(typeof(ValidationPipelineBehavior<,>))
            .AddNotificationPipelineBehavior(typeof(ExceptionHandlingNotificationPipelineBehavior<>))
            .AddValidators(typeof(ApplicationServices).Assembly, true);
    }
}
