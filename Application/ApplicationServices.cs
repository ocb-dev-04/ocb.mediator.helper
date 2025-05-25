using OCB.Mediator.Helper;
using OCB.Mediator.Helper.Behaviors;
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
            .AddValidators(typeof(ApplicationServices).Assembly, true);
    }
}
