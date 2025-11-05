using System.Linq.Expressions;
using System.Text.Json;

namespace Forge.OData.Client.Core
{
    /// <summary>
    /// Provides a queryable interface for OData entities that translates LINQ expressions to OData queries
    /// </summary>
    public class ODataQueryable<T> where T : class
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _entitySetName;
        private readonly ODataQueryBuilder _queryBuilder;

        public ODataQueryable(HttpClient httpClient, string baseUrl, string entitySetName)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
            _entitySetName = entitySetName ?? throw new ArgumentNullException(nameof(entitySetName));
            _queryBuilder = new ODataQueryBuilder();
        }

        private ODataQueryable(HttpClient httpClient, string baseUrl, string entitySetName, ODataQueryBuilder queryBuilder)
        {
            _httpClient = httpClient;
            _baseUrl = baseUrl;
            _entitySetName = entitySetName;
            _queryBuilder = queryBuilder;
        }

        /// <summary>
        /// Filters the entities based on a predicate
        /// </summary>
        public ODataQueryable<T> Where(Expression<Func<T, bool>> predicate)
        {
            var newBuilder = _queryBuilder.Clone();
            var filterExpression = new ODataExpressionVisitor().Visit(predicate);
            newBuilder.AddFilter(filterExpression);
            return new ODataQueryable<T>(_httpClient, _baseUrl, _entitySetName, newBuilder);
        }

        /// <summary>
        /// Orders the entities by a property in ascending order
        /// </summary>
        public ODataQueryable<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            var newBuilder = _queryBuilder.Clone();
            var propertyName = GetPropertyName(keySelector);
            newBuilder.AddOrderBy(propertyName, ascending: true);
            return new ODataQueryable<T>(_httpClient, _baseUrl, _entitySetName, newBuilder);
        }

        /// <summary>
        /// Orders the entities by a property in descending order
        /// </summary>
        public ODataQueryable<T> OrderByDescending<TKey>(Expression<Func<T, TKey>> keySelector)
        {
            var newBuilder = _queryBuilder.Clone();
            var propertyName = GetPropertyName(keySelector);
            newBuilder.AddOrderBy(propertyName, ascending: false);
            return new ODataQueryable<T>(_httpClient, _baseUrl, _entitySetName, newBuilder);
        }

        /// <summary>
        /// Skips a specified number of entities
        /// </summary>
        public ODataQueryable<T> Skip(int count)
        {
            var newBuilder = _queryBuilder.Clone();
            newBuilder.SetSkip(count);
            return new ODataQueryable<T>(_httpClient, _baseUrl, _entitySetName, newBuilder);
        }

        /// <summary>
        /// Takes a specified number of entities
        /// </summary>
        public ODataQueryable<T> Take(int count)
        {
            var newBuilder = _queryBuilder.Clone();
            newBuilder.SetTop(count);
            return new ODataQueryable<T>(_httpClient, _baseUrl, _entitySetName, newBuilder);
        }

        /// <summary>
        /// Selects specific properties
        /// </summary>
        public ODataQueryable<T> Select<TResult>(Expression<Func<T, TResult>> selector)
        {
            var newBuilder = _queryBuilder.Clone();
            var properties = GetSelectedProperties(selector);
            newBuilder.SetSelect(properties);
            return new ODataQueryable<T>(_httpClient, _baseUrl, _entitySetName, newBuilder);
        }

        /// <summary>
        /// Expands navigation properties
        /// </summary>
        public ODataQueryable<T> Expand<TProperty>(Expression<Func<T, TProperty>> navigationProperty)
        {
            var newBuilder = _queryBuilder.Clone();
            var propertyName = GetPropertyName(navigationProperty);
            newBuilder.AddExpand(propertyName);
            return new ODataQueryable<T>(_httpClient, _baseUrl, _entitySetName, newBuilder);
        }

        /// <summary>
        /// Executes the query and returns the results
        /// </summary>
        public async Task<List<T>> ToListAsync()
        {
            var url = BuildUrl();
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ODataResponse<T>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result?.Value ?? [];
        }

        /// <summary>
        /// Executes the query and returns the first result or default
        /// </summary>
        public async Task<T> FirstOrDefaultAsync()
        {
            var results = await Take(1).ToListAsync();
            return results.FirstOrDefault();
        }

        /// <summary>
        /// Returns the count of entities
        /// </summary>
        public async Task<int> CountAsync()
        {
            var newBuilder = _queryBuilder.Clone();
            newBuilder.SetCount(true);

            var url = BuildUrl(newBuilder);
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ODataCountResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result?.Count ?? 0;
        }

        private string BuildUrl(ODataQueryBuilder? builder = null)
        {
            var queryBuilder = builder ?? _queryBuilder;
            var baseUri = _baseUrl.TrimEnd('/');
            var query = queryBuilder.Build();

            if (string.IsNullOrEmpty(query))
            {
                return $"{baseUri}/{_entitySetName}";
            }

            return $"{baseUri}/{_entitySetName}?{query}";
        }

        private string GetPropertyName<TProperty>(Expression<Func<T, TProperty>> expression)
        {
            if (expression.Body is MemberExpression memberExpression)
            {
                return memberExpression.Member.Name;
            }

            throw new ArgumentException("Expression must be a property accessor", nameof(expression));
        }

        private List<string> GetSelectedProperties<TResult>(Expression<Func<T, TResult>> expression)
        {
            var properties = new List<string>();

            if (expression.Body is NewExpression newExpression)
            {
                foreach (var arg in newExpression.Arguments)
                {
                    if (arg is MemberExpression memberExpression)
                    {
                        properties.Add(memberExpression.Member.Name);
                    }
                }
            }
            else if (expression.Body is MemberExpression memberExpression)
            {
                properties.Add(memberExpression.Member.Name);
            }

            return properties;
        }
    }

    internal class ODataResponse<T>(List<T> value)
    {
        public List<T> Value { get; set; } = value;
    }

    internal class ODataCountResponse
    {
        public int Count { get; set; }
    }
}
