using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Forge.OData.Client
{
    /// <summary>
    /// Builds OData query strings from query parameters
    /// </summary>
    public class ODataQueryBuilder
    {
        private readonly List<string> _filters = new List<string>();
        private readonly List<string> _orderBy = new List<string>();
        private readonly List<string> _expand = new List<string>();
        private readonly List<string> _select = new List<string>();
        private int? _top;
        private int? _skip;
        private bool _count;

        public ODataQueryBuilder Clone()
        {
            var clone = new ODataQueryBuilder();
            clone._filters.AddRange(_filters);
            clone._orderBy.AddRange(_orderBy);
            clone._expand.AddRange(_expand);
            clone._select.AddRange(_select);
            clone._top = _top;
            clone._skip = _skip;
            clone._count = _count;
            return clone;
        }

        public void AddFilter(string filter)
        {
            if (!string.IsNullOrWhiteSpace(filter))
            {
                _filters.Add(filter);
            }
        }

        public void AddOrderBy(string property, bool ascending)
        {
            var direction = ascending ? "asc" : "desc";
            _orderBy.Add($"{property} {direction}");
        }

        public void AddExpand(string navigationProperty)
        {
            if (!string.IsNullOrWhiteSpace(navigationProperty))
            {
                _expand.Add(navigationProperty);
            }
        }

        public void SetSelect(List<string> properties)
        {
            _select.Clear();
            _select.AddRange(properties.Where(p => !string.IsNullOrWhiteSpace(p)));
        }

        public void SetTop(int count)
        {
            _top = count;
        }

        public void SetSkip(int count)
        {
            _skip = count;
        }

        public void SetCount(bool includeCount)
        {
            _count = includeCount;
        }

        public string Build()
        {
            var parts = new List<string>();

            // $filter
            if (_filters.Any())
            {
                var filterExpression = string.Join(" and ", _filters.Select(f => $"({f})"));
                parts.Add($"$filter={Uri.EscapeDataString(filterExpression)}");
            }

            // $orderby
            if (_orderBy.Any())
            {
                var orderByExpression = string.Join(", ", _orderBy);
                parts.Add($"$orderby={Uri.EscapeDataString(orderByExpression)}");
            }

            // $expand
            if (_expand.Any())
            {
                var expandExpression = string.Join(",", _expand);
                parts.Add($"$expand={Uri.EscapeDataString(expandExpression)}");
            }

            // $select
            if (_select.Any())
            {
                var selectExpression = string.Join(",", _select);
                parts.Add($"$select={Uri.EscapeDataString(selectExpression)}");
            }

            // $top
            if (_top.HasValue)
            {
                parts.Add($"$top={_top.Value}");
            }

            // $skip
            if (_skip.HasValue)
            {
                parts.Add($"$skip={_skip.Value}");
            }

            // $count
            if (_count)
            {
                parts.Add("$count=true");
            }

            return string.Join("&", parts);
        }
    }
}
