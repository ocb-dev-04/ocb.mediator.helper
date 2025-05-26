using FluentValidation;
using System.Reflection;
using OCB.Mediator.Helper.Abstractions.Sender;
using Microsoft.Extensions.DependencyInjection;
using OCB.Mediator.Helper.Abstractions.Messaging;
using OCB.Mediator.Helper.Implementations.Sender;
using OCB.Mediator.Helper.Abstractions.Pipelines;
using OCB.Mediator.Helper.Abstractions.Notification;
using OCB.Mediator.Helper.Implementations.Notification;

namespace OCB.Mediator.Helper;

/// <summary>
/// Class to add all service to use mediator helper
/// </summary>
public static class MediatorServices
{
    /// <summary>
    /// Add all query/handlers using scruptor
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static IServiceCollection AddMediatorHelperServices(
        this IServiceCollection services, 
        Assembly assembly)
    {
        services.AddScoped<ISender, Sender>();
        services.AddScoped<INotificationDispatcher, NotificationDispatcher>();

        services.Scan(scan => scan.FromAssemblies(assembly)
            .AddClasses(clases => clases.AssignableTo(typeof(IQueryHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(clases => clases.AssignableTo(typeof(ICommandHandler<>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(clases => clases.AssignableTo(typeof(ICommandHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );

        services.Scan(scan => scan.FromApplicationDependencies()
            .AddClasses(c => c.AssignableTo(typeof(INotificationHandler<>)), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }

    /// <summary>
    /// Add custom pipeline behavior
    /// </summary>
    /// <param name="services"></param>
    /// <param name="behaviorType"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static IServiceCollection AddPipelineBehavior(
        this IServiceCollection services,
        Type behaviorType)
    {
        if (!behaviorType.IsGenericTypeDefinition)
            throw new ArgumentException("Only open generic types are allowed", nameof(behaviorType));

        Type interfaceType = typeof(IPipelineBehavior<,>);
        services.AddScoped(interfaceType, behaviorType);

        return services;
    }

    /// <summary>
    /// Add fluent validators
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assembly"></param>
    /// <param name="includeInternalTypes"></param>
    /// <returns></returns>
    public static IServiceCollection AddValidators(
        this IServiceCollection services, 
        Assembly assembly, 
        bool includeInternalTypes = false)
    {
        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: includeInternalTypes);

        return services;
    }
}
