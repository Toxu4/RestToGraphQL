using System.Linq;
using System.Text.RegularExpressions;

namespace RestToGraphQL.FileQueryStorage.Internal
{
    internal class Query        
    {
        public string RequestMethod { get; private set; }
        public Regex RequestPatternRegex { get; private set; }
        
        public string QueryText { get; private set; }
            
        public string[] ParamsToResolve{ get; private set; }
        
        public string ResultToken { get; private set; }

        private Query()
        {
            
        }

        public static Query CreateFrom(QueryDoc queryDoc)
        {
            return new Query
            {
                RequestMethod = queryDoc.RequestMethod,
                RequestPatternRegex = new Regex(queryDoc.RequestPattern, RegexOptions.Compiled),
                QueryText = queryDoc.QueryText,
                ResultToken = queryDoc.ResultToken,
                ParamsToResolve = Regex
                    .Matches(queryDoc.QueryText, "\\$(?<name>[a-z]+[0-9]?[a-z]?)", RegexOptions.Multiline)
                    .Cast<Match>()
                    .Select(m => m.Groups["name"].Value)
                    .Distinct()
                    .ToArray()
            };
        }
    }
}