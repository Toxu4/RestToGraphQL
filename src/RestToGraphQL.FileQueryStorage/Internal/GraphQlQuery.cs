using System.Collections.Generic;
using Toxu4.GraphQl.Client;

namespace RestToGraphQL.FileQueryStorage.Internal
{
    internal class GraphQlQuery : IGraphQlQuery
    {
        public string QueryText { get; set; }
        public IDictionary<string, object> Variables { get; set; }
    }
}