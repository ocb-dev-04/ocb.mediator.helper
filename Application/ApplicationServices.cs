using CQRS.MediatR.Helper;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class ApplicationServices
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddCqrsServices(typeof(ApplicationServices).Assembly);
        //services.AddScoped<ICommandHandler<CreateCommand, Guid>, CreateCommandHandler>();
        //services.AddScoped<IQueryHandler<GetByIdQuery, GetByIdResponse>, GetByIdQueryHandler>();
    }
}
