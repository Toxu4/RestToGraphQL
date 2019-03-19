using System;
using Microsoft.Extensions.DependencyInjection;
using RestToGraphQL.FileQueryStorage;

namespace RestToGraphQL.Core
{
    public static class RegistrationExtensions
    {
        public static void AddFileQueryStorage(this IServiceCollection services, Action<FileQueryStorageSettings> settings)
        {
            services.Configure(settings);
            services.AddSingleton<IQueryStorage, FileQueryStorage.Internal.FileQueryStorage>();
        }
    }
}