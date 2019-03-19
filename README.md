# RestToGraphQL

[![Build status](https://ci.appveyor.com/api/projects/status/gang5b621tu4skwf?svg=true)](https://ci.appveyor.com/project/Toxu4/resttographql)
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/RestToGraphQL.Core.svg)](https://www.nuget.org/packages/RestToGraphQL.Core)
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/RestToGraphQL.FileQueryStorage.svg)](https://www.nuget.org/packages/RestToGraphQL.FileQueryStorage)

## Getting started

RestAPI proxy to GraphQL api.

Add package (or implement your own Query storage)

```
dotnet add package RestToGraphQL.FileQueryStorage --version 1.0.1-beta
```

Change Startup.cs

```
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            //...            

            // Add RestToGraphQL proxy     
            services.AddRestToGraphQLCore();
            services.AddFileQueryStorage(settings => settings.Path = "./Queries");
            services.AddGraphQlClient(settings => settings.Endpoint = "http://localhost:5000/graphql");

            //...
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //...

            app.UseRestToGraphQL();

            //...
        }
    }
```

Place your queries into "./Queries" directory.

Query example:

```
{
  "RequestMethod" : "GET",
  "RequestPattern" : "api/folders/?$",
  "QueryText" : "query getFolder($name: String){ folder(name: $name){ fullName content{ __typename ... on FolderType{ fullName } ... on FileType{ name }}}}",
  "ResultToken" : "folder"
}
```

Open

```
http://localhost:5000/api/folders?name=C:\
```





