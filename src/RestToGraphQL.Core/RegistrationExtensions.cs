using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace RestToGraphQL.Core
{
    public static class RegistrationExtensions
    {
        public static void UseRestToGraphQL(this IApplicationBuilder app)
        {
            app.Use(async (context, func) =>
            {
                var handler = context.RequestServices.GetRequiredService<Handler>();                
                
                var success = await handler.Handle(context);
                if (!success)
                {
                    await func();
                }                
            });            
        }

        public static void AddRestToGraphQLCore(this IServiceCollection services)
        {
            services.AddSingleton<Handler>();
        }
    }
}