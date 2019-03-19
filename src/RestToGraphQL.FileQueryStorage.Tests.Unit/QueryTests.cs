using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using RestToGraphQL.FileQueryStorage.Internal;

namespace RestToGraphQL.FileQueryStorage.Tests.Unit
{
    [TestFixture]
    public class QueryTests
    {
        private static Fixture Fixture = new Fixture();
        
        [Test]
        [TestCase(
            @"query getCities($offset: Int $limit: Int!){
                cities{
                    list(offset: $offset limit: $limit){
                        ...cityFields
                    }
                    count        
                }
                anotherCities{
                    list{
                        ...mainCityFields    
                    }
                }
            }",
            new [] { "offset", "limit"})]
        public void CreateFrom_should_create_from_querydoc(string queryText, string[] expectedParamsToResolve)
        {
            // given
            var queryDoc = Fixture
                .Build<QueryDoc>()
                .With(d => d.QueryText, queryText)
                .Create();

            // when
            var query = Query.CreateFrom(queryDoc);

            // then
            query.RequestMethod.Should().Be(queryDoc.RequestMethod);
            query.RequestPatternRegex.ToString().Should().Be(queryDoc.RequestPattern);
            query.QueryText.Should().Be(queryDoc.QueryText);
            query.ResultToken.Should().Be(queryDoc.ResultToken);
            query.ParamsToResolve.Should().BeEquivalentTo(expectedParamsToResolve);
        }
    }
}