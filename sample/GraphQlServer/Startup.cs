using GraphQlServer.Schema;
using GraphQlServer.Schema.Types;
using GraphQL;
using GraphQL.Server;
using GraphQL.Server.Ui.Playground;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using RestToGraphQL.Core;
using Toxu4.GraphQl.Client;

namespace GraphQlServer
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddGraphQL(options =>
                    {
                        options.EnableMetrics = true;
                        options.ExposeExceptions = true;
                    })
                .AddDataLoader();
            
            services.AddSingleton<IDependencyResolver>(
                c => new FuncDependencyResolver(c.GetService));
            
            //            
            services.AddSingleton<ComputerSchema>();
            services.AddSingleton<ComputerQuery>();
            services.AddSingleton<DriveQuery>();
            services.AddSingleton<DriveType>();
            services.AddSingleton<FolderOrFileType>();
            services.AddSingleton<FolderType>();
            services.AddSingleton<FileType>(); 
            
            // Add RestToGraphQL proxy     
            services.AddRestToGraphQLCore();
            services.AddFileQueryStorage(settings => settings.Path = "./Queries");
            services.AddGraphQlClient(settings => settings.Endpoint = "http://localhost:5000/graphql");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseWebSockets();

            app.UseGraphQL<ComputerSchema>();

            app.UseGraphQLPlayground(new GraphQLPlaygroundOptions());
            
            app.UseRestToGraphQL();
        }
    }
}