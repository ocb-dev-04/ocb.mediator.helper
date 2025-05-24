using FluentValidation;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using CQRS.MediatR.Helper.Abstractions.Messaging;
using CQRS.MediatR.Helper.Implementations.Sender;
using CQRS.MediatR.Helper.Abstractions.Sender;

namespace CQRS.MediatR.Helper;

public static class CqrsServices
{
    public static void AddCqrsServices(this IServiceCollection services, Assembly assembly)
    {
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

        services.AddScoped<ISender, Sender>();

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);
    }
}
