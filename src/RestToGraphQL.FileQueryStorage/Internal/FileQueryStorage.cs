using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestToGraphQL.Core;
using Toxu4.GraphQl.Client;

namespace RestToGraphQL.FileQueryStorage.Internal
{
    internal class FileQueryStorage : IQueryStorage, IDisposable
    {
        private readonly ConcurrentDictionary<string,Query> _queries = new ConcurrentDictionary<string, Query>();
        private readonly FileSystemWatcher _watcher = new FileSystemWatcher();
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);        

        public FileQueryStorage(IOptions<FileQueryStorageSettings> settings)
        {
            foreach (var fileName in Directory.EnumerateFiles(settings.Value.Path))
            {
                TryAddOrUpdateQuery(fileName);
            }
            
            StartWatching();

            return;

            // local functions
            
            void StartWatching()
            {
                _watcher.Path = settings.Value.Path;
                _watcher.Filter = "*.json";
                _watcher.NotifyFilter =
                    NotifyFilters.FileName |
                    NotifyFilters.LastWrite |
                    NotifyFilters.Size |
                    NotifyFilters.Attributes;

                _watcher.Changed += (sender, args) => ChangeHandler(args);
                _watcher.Created += (sender, args) => ChangeHandler(args);
                _watcher.Deleted += (sender, args) => ChangeHandler(args);
                _watcher.Renamed += (sender, args) => RenameHandler(args);

                _watcher.EnableRaisingEvents = true;
            }

            void ChangeHandler(FileSystemEventArgs args)
            {
                if (args.ChangeType == WatcherChangeTypes.Deleted)
                {
                    _queries.TryRemove(args.FullPath, out _);
                }
                else
                {
                    TryAddOrUpdateQuery(args.FullPath);   
                }
            }
            
            void RenameHandler(RenamedEventArgs args)
            {
                _queries.TryRemove(args.OldFullPath, out _);
                TryAddOrUpdateQuery(args.FullPath);   
            }
            
            void TryAddOrUpdateQuery(string fileName)
            {
                if (!fileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
                
                _semaphore.Wait();
                
                try
                {                   
                    var fileContent = RunWithRetries(
                        () => File.Exists(fileName) ? File.ReadAllText(fileName) : null, 
                        5, 
                        TimeSpan.FromMilliseconds(100).Milliseconds);
                    
                    if (fileContent == null)
                    {
                        return;
                    }

                    var queryDoc = JsonConvert.DeserializeObject<QueryDoc>(fileContent);

                    var query = Query.CreateFrom(queryDoc);

                    _queries.AddOrUpdate(fileName, query, (key,value) => query);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
                finally
                {
                    _semaphore.Release();
                }
            } 
            
            string RunWithRetries(Func<string> func, int retryCount, int delayMilliseconds)
            {
                for (var i = 1; i <= retryCount; i++)
                {
                    try
                    {
                        return func();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Try {i}. Error: {e.Message}");
                        Task.Delay(delayMilliseconds*i).GetAwaiter().GetResult();
                    }
                }
                throw new Exception("Exceeded number of attempts");
            }            
        }

        public Task<(IGraphQlQuery query, string resultToken)> GetQueryFor(string method, string path, Func<string, object> paramResolver)
        {
            var (query, match) = _queries
                .Select(q =>
                {
                    if (q.Value.RequestMethod != method)
                    {
                        return (Query: (Query) null, Match: (Match) null);
                    }

                    var innerMatch = q.Value.RequestPatternRegex.Match(path);
                    
                    return !innerMatch.Success 
                        ? (Query: (Query) null, Match: (Match) null) 
                        : (Query: q.Value, Match: innerMatch);
                })
                .FirstOrDefault(tuple => tuple.Query != null);

            if (query == null)
            {
                return Task.FromResult(((IGraphQlQuery)null, (string)null));
            }

            var variables = query
                .ParamsToResolve
                .Select( paramName => (
                    Key: paramName,
                    Value: 
                        match.Groups[paramName].Success ? 
                            match.Groups[paramName] : 
                            paramResolver(paramName)
                ))
                .Where(tuple => tuple.Value != null)
                .ToDictionary(
                    tuple => tuple.Key,
                    tuple => tuple.Value);            
            
            var graphQlQuery =  new GraphQlQuery
                {
                    QueryText = query.QueryText,
                    Variables = variables
                };

            return Task.FromResult(((IGraphQlQuery)graphQlQuery, query.ResultToken));
        }

        public void Dispose()
        {
            _watcher.Dispose();
        }
    }
}