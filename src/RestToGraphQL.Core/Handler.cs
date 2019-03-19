using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Toxu4.GraphQl.Client;

namespace RestToGraphQL.Core
{
    internal class Handler
    {
        private readonly IQueryStorage _queryStorage;
        private readonly IGraphQlQueryExecutor _graphQlQueryExecutor;

        public Handler(IQueryStorage queryStorage, IGraphQlQueryExecutor graphQlQueryExecutor)
        {
            _queryStorage = queryStorage;
            _graphQlQueryExecutor = graphQlQueryExecutor;
        }

        public async Task<bool> Handle(HttpContext context)
        {
            var (query, resultToken) = await _queryStorage
                .GetQueryFor(
                    context.Request.Method, 
                    context.Request.Path.Value,
                    paramName => ResolveParam(context, paramName));
            
            if (query == null)
            {
                return false;
            }
            
            var (data, errors) = await _graphQlQueryExecutor.Run<IGraphQlQuery, QueryResult<JObject>>(query);
            
            context.Response.Headers.Add("Content-Type", "application/json");
            
            if (errors?.Length > 0)
            {
                context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(errors));
            }
            else
            {
                var result = data.SelectToken(resultToken);
                if (result != null)
                {
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(result));
                }
                else
                {
                    context.Response.StatusCode = (int) HttpStatusCode.NotFound;
                }            
            }
                        
            return true;            
        }

        private static object ResolveParam(HttpContext context, string paramName)
        {
            return 
                context.Request.Query[paramName].FirstOrDefault() 
                ??
                (
                    context.Request.HasFormContentType
                        ? context.Request.Form[paramName].FirstOrDefault()
                        : null
                );
        }
    }
}