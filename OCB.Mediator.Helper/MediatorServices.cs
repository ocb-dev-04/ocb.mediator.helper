using FluentValidation;
using System.Reflection;
using OCB.Mediator.Helper.Abstractions.Sender;
using Microsoft.Extensions.DependencyInjection;
using OCB.Mediator.Helper.Implementations.Sender;
using OCB.Mediator.Helper.Abstractions.Messaging;
using OCB.Mediator.Helper.Abstractions.Pipelines;
using OCB.Mediator.Helper.Abstractions.Notification;
using OCB.Mediator.Helper.Implementations.Notification;

namespace OCB.Mediator.Helper;

/// <summary>
/// Provides extension methods for configuring mediator-related services, including query and command handlers, 
/// pipeline behaviors, notification handlers, and validators.
/// </summary>
/// <remarks>This static class is designed to simplify the registration of mediator components in an application's
/// dependency injection container. It includes methods for adding handlers, pipeline behaviors, and validators  using
/// conventions and assemblies.</remarks>
public static class MediatorServices
{
    /// <summary>
    /// Registers mediator-related services and handlers into the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>This method scans the provided assembly for implementations of mediator interfaces, such as 
    /// <see cref="IQueryHandler{TQuery, TResult}"/>, <see cref="ICommandHandler{TCommand}"/>,  <see
    /// cref="ICommandHandler{TCommand, TResult}"/>, and <see cref="INotificationHandler{TNotification}"/>,  and
    /// registers them with a scoped lifetime. Additionally, it registers core mediator services,  including <see
    /// cref="ISender"/> and <see cref="INotificationDispatcher"/>.</remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the services will be added.</param>
    /// <param name="assembly">The assembly containing the handler implementations to be registered.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddMediatorHelperServices(
        this IServiceCollection services, 
        Assembly assembly)
    {
        services.AddScoped<Sender>();
        services.AddScoped<ISender>(sp => sp.GetRequiredService<Sender>());

        services.AddScoped<INotificationDispatcher, NotificationDispatcher>();

        services.Scan(scan => scan.FromAssemblies(new[] { assembly })
            .AddClasses(clases => clases.AssignableTo(typeof(IQueryHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithTransientLifetime()
            .AddClasses(clases => clases.AssignableTo(typeof(ICommandHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithTransientLifetime()
        );

        services.Scan(scan => scan.FromAssemblies(new[] { assembly })
            .AddClasses(c => c.AssignableTo(typeof(INotificationHandler<>)), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }

    /// <summary>
    /// Registers mediator-related services and handlers into the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <remarks>This method adds the following services to the dependency injection container: <list
    /// type="bullet"> <item><description><see cref="Sender"/> and its interface <see cref="ISender"/> for sending
    /// queries and commands.</description></item> <item><description><see cref="INotificationDispatcher"/> for
    /// dispatching notifications.</description></item> <item><description>All implementations of <see
    /// cref="IQueryHandler{TQuery, TResult}"/>, <see cref="ICommandHandler{TCommand}"/>, <see
    /// cref="ICommandHandler{TCommand, TResult}"/>, and <see cref="INotificationHandler{TNotification}"/> found in the
    /// specified assemblies.</description></item> </list> Handlers are registered with a scoped lifetime.</remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the services will be added.</param>
    /// <param name="assemblies">An array of <see cref="Assembly"/> instances to scan for handler implementations.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddMediatorHelperServices(
        this IServiceCollection services,
        Assembly[] assemblies)
    {
        services.AddScoped<Sender>();
        services.AddScoped<ISender>(sp => sp.GetRequiredService<Sender>());

        services.AddScoped<INotificationDispatcher, NotificationDispatcher>();

        services.Scan(scan => scan.FromAssemblies(assemblies)
            .AddClasses(clases => clases.AssignableTo(typeof(IQueryHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithTransientLifetime()
            .AddClasses(clases => clases.AssignableTo(typeof(ICommandHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithTransientLifetime()
        );

        services.Scan(scan => scan.FromAssemblies(assemblies)
            .AddClasses(c => c.AssignableTo(typeof(INotificationHandler<>)), publicOnly: false)
            .AsImplementedInterfaces()
            .WithTransientLifetime());

        return services;
    }

    /// <summary>
    /// Registers a pipeline behavior type in the dependency injection container.
    /// </summary>
    /// <remarks>This method is typically used to register custom pipeline behaviors for MediatR. Pipeline
    /// behaviors allow you to define cross-cutting concerns, such as logging, validation, or performance monitoring, 
    /// that are executed during the processing of requests and responses.</remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the pipeline behavior will be added.</param>
    /// <param name="behaviorType">The type of the pipeline behavior to register. Must be an open generic type that implements <see
    /// cref="IRequestPipelineBehavior{TRequest, TResponse}"/>.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="behaviorType"/> is not an open generic type or does not implement <see
    /// cref="IRequestPipelineBehavior{TRequest, TResponse}"/>.</exception>
    public static IServiceCollection AddRequestPipelineBehavior(
        this IServiceCollection services,
        Type behaviorType)
    {
        if (!behaviorType.IsGenericTypeDefinition)
            throw new ArgumentException("Only open generic types are allowed", nameof(behaviorType));

        if (!behaviorType.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestPipelineBehavior<,>)))
            throw new ArgumentException($"{behaviorType.Name} must implement IRequestPipelineBehavior<,>", nameof(behaviorType));

        Type interfaceType = typeof(IRequestPipelineBehavior<,>);
        services.AddScoped(interfaceType, behaviorType);

        return services;
    }

    /// <summary>
    /// Registers a notification pipeline behavior in the dependency injection container.
    /// </summary>
    /// <remarks>This method is used to register custom pipeline behaviors for notifications in a
    /// Mediator-like pattern. The <paramref name="behaviorType"/> must be an open generic type definition that conforms
    /// to the <see cref="INotificationPipelineBehavior{TNotification}"/> interface.</remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the behavior will be added.</param>
    /// <param name="behaviorType">The type of the notification pipeline behavior to register. Must be an open generic type that implements <see
    /// cref="INotificationPipelineBehavior{TNotification}"/>.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="behaviorType"/> is not an open generic type or does not implement <see
    /// cref="INotificationPipelineBehavior{TNotification}"/>.</exception>
    public static IServiceCollection AddNotificationPipelineBehavior(
        this IServiceCollection services,
        Type behaviorType)
    {
        if (!behaviorType.IsGenericTypeDefinition)
            throw new ArgumentException("Only open generic types are allowed", nameof(behaviorType));

        if (!behaviorType.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationPipelineBehavior<>)))
            throw new ArgumentException($"{behaviorType.Name} must implement INotificationPipelineBehavior<>", nameof(behaviorType));

        Type interfaceType = typeof(INotificationPipelineBehavior<>);
        services.AddScoped(interfaceType, behaviorType);

        return services;
    }

    /// <summary>
    /// Registers validators from the specified assembly into the service collection.
    /// </summary>
    /// <remarks>This method scans the specified assembly for validator types and registers them with the
    /// service collection. Validators are typically used for input validation in applications.</remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the validators will be added.</param>
    /// <param name="assembly">The assembly containing the validators to register.</param>
    /// <param name="includeInternalTypes">A value indicating whether internal types in the specified assembly should be included. <see langword="true"/>
    /// to include internal types; otherwise, <see langword="false"/>.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddValidators(
        this IServiceCollection services, 
        Assembly assembly, 
        bool includeInternalTypes = false)
    {
        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: includeInternalTypes);

        return services;
    }

    /// <summary>
    /// Registers validators from the specified assemblies into the service collection.
    /// </summary>
    /// <remarks>This method scans the provided assemblies for validator implementations and registers them in
    /// the service collection. Validators are typically used for input validation in applications.</remarks>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the validators will be added.</param>
    /// <param name="assemblies">An array of <see cref="Assembly"/> objects representing the assemblies to scan for validators.</param>
    /// <param name="includeInternalTypes">A value indicating whether internal types should be included when scanning for validators. <see
    /// langword="true"/> to include internal types; otherwise, <see langword="false"/>.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddValidators(
        this IServiceCollection services,
        Assembly[] assemblies,
        bool includeInternalTypes = false)
    {
        services.AddValidatorsFromAssemblies(assemblies, includeInternalTypes: includeInternalTypes);

        return services;
    }
}
