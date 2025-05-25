using FluentValidation;
using System.Reflection;
using CQRS.MediatR.Helper.Abstractions.Sender;
using Microsoft.Extensions.DependencyInjection;
using CQRS.MediatR.Helper.Abstractions.Messaging;
using CQRS.MediatR.Helper.Implementations.Sender;
using CQRS.MediatR.Helper.Abstractions.Pipelines;

namespace CQRS.MediatR.Helper;

/// <summary>
/// Class to add all service to use CQRS
/// </summary>
public static class CqrsServices
{
    /// <summary>
    /// Add all queriy/handlers using scruptor
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assembly"></param>
    /// <returns></returns>
    public static IServiceCollection AddCqrsServices(
        this IServiceCollection services, 
        Assembly assembly)
    {
        services.AddScoped<ISender, Sender>();

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
            //.AddClasses(classes => classes.AssignableTo(typeof(IPipelineBehavior<,>)), publicOnly: false)
            //    .AsImplementedInterfaces()
            //    .WithScopedLifetime()
        );

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
