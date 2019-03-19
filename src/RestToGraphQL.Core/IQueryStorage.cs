using System;
using System.Threading.Tasks;
using Toxu4.GraphQl.Client;

namespace RestToGraphQL.Core
{
    public interface IQueryStorage
    {
        Task<(IGraphQlQuery query, string resultToken)> GetQueryFor(string method, string path, Func<string, object> paramResolver);        
    }
}