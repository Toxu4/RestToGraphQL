namespace RestToGraphQL.FileQueryStorage.Internal
{
    internal class QueryDoc
    {
        public string RequestMethod { get; set; }
        public string RequestPattern { get; set; }
        
        public string QueryText { get; set; }
        
        public string ResultToken { get; set; }        
    }
}