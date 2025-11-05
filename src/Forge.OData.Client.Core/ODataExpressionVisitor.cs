using System.Collections.Generic;
using System.Linq.Expressions;

namespace Forge.OData.Client.Core
{
    /// <summary>
    /// Visitor that converts LINQ expressions to OData filter syntax
    /// </summary>
    public class ODataExpressionVisitor
    {
        public string Visit(Expression expression)
        {
            if (expression == null)
                return string.Empty;

            return expression switch
            {
                LambdaExpression lambda => Visit(lambda.Body),
                BinaryExpression binary => VisitBinary(binary),
                MemberExpression member => VisitMember(member),
                ConstantExpression constant => VisitConstant(constant),
                MethodCallExpression methodCall => VisitMethodCall(methodCall),
                UnaryExpression unary => VisitUnary(unary),
                _ => throw new NotSupportedException($"Expression type {expression.GetType().Name} is not supported"),
            };
        }

        private string VisitBinary(BinaryExpression binary)
        {
            var left = Visit(binary.Left);
            var right = Visit(binary.Right);

            var op = binary.NodeType switch
            {
                ExpressionType.Equal => "eq",
                ExpressionType.NotEqual => "ne",
                ExpressionType.GreaterThan => "gt",
                ExpressionType.GreaterThanOrEqual => "ge",
                ExpressionType.LessThan => "lt",
                ExpressionType.LessThanOrEqual => "le",
                ExpressionType.AndAlso => "and",
                ExpressionType.OrElse => "or",
                ExpressionType.Add => "add",
                ExpressionType.Subtract => "sub",
                ExpressionType.Multiply => "mul",
                ExpressionType.Divide => "div",
                ExpressionType.Modulo => "mod",
                _ => throw new NotSupportedException($"Binary operator {binary.NodeType} is not supported")
            };

            return $"{left} {op} {right}";
        }

        private string VisitMember(MemberExpression member)
        {
            // If it's a constant member access (like a captured variable), evaluate it
            if (member.Expression is ConstantExpression constantExpression)
            {
                var container = constantExpression.Value;
                var value = member.Member switch
                {
                    System.Reflection.FieldInfo field => field.GetValue(container),
                    System.Reflection.PropertyInfo property => property.GetValue(container),
                    _ => throw new NotSupportedException($"Member type {member.Member.GetType().Name} is not supported")
                };
                return FormatValue(value);
            }

            // Build the property path for nested properties
            var path = BuildPropertyPath(member);
            
            // Handle collection Count property - translate to OData $count
            const string countSuffix = "/Count";
            if (path.EndsWith(countSuffix) && IsCollectionProperty(member.Expression))
            {
                // Remove /Count and add /$count
                path = path.Substring(0, path.Length - countSuffix.Length) + "/$count";
            }
            
            return path;
        }
        
        private bool IsCollectionProperty(Expression? expression)
        {
            if (expression is not MemberExpression memberExpr)
                return false;
                
            var propertyType = memberExpr.Type;
            
            // Check if it's a collection type (implements IEnumerable but is not a string)
            return propertyType != typeof(string) && 
                   typeof(System.Collections.IEnumerable).IsAssignableFrom(propertyType);
        }

        private string VisitConstant(ConstantExpression constant)
        {
            return FormatValue(constant.Value);
        }

        private string VisitMethodCall(MethodCallExpression methodCall)
        {
            var methodName = methodCall.Method.Name;

            // String methods
            if (methodCall.Method.DeclaringType == typeof(string))
            {
                return methodName switch
                {
                    "StartsWith" => VisitStartsWith(methodCall),
                    "EndsWith" => VisitEndsWith(methodCall),
                    "Contains" => VisitContains(methodCall),
                    "ToLower" when methodCall.Object is { } => $"tolower({Visit(methodCall.Object)})",
                    "ToUpper" when methodCall.Object is { } => $"toupper({Visit(methodCall.Object)})",
                    "Trim" when methodCall.Object is { } => $"trim({Visit(methodCall.Object)})",
                    "Length" when methodCall.Object is { } => $"length({Visit(methodCall.Object)})",
                    _ => throw new NotSupportedException($"Method {methodName} is not supported")
                };
            }

            // Static string methods
            if (methodCall.Method.DeclaringType == typeof(string) && methodCall.Method.IsStatic)
            {
                if (methodName == "Concat")
                {
                    return VisitConcat(methodCall);
                }
            }

            throw new NotSupportedException($"Method {methodName} is not supported");
        }

        private string VisitUnary(UnaryExpression unary)
            => unary.NodeType switch
            {
                ExpressionType.Not => $"not ({Visit(unary.Operand)})",
                ExpressionType.Convert => Visit(unary.Operand),
                _ => throw new NotSupportedException($"Unary operator {unary.NodeType} is not supported")
            };


        private string VisitStartsWith(MethodCallExpression methodCall)
        {
            if (methodCall.Object is not { }) throw new NotSupportedException($"Static call {methodCall} is not supported.");
            var obj = Visit(methodCall.Object);
            var arg = Visit(methodCall.Arguments[0]);
            return $"startswith({obj}, {arg})";
        }

        private string VisitEndsWith(MethodCallExpression methodCall)
        {
            if (methodCall.Object is not { }) throw new NotSupportedException($"Static call {methodCall} is not supported.");
            var obj = Visit(methodCall.Object);
            var arg = Visit(methodCall.Arguments[0]);
            return $"endswith({obj}, {arg})";
        }

        private string VisitContains(MethodCallExpression methodCall)
        {
            if (methodCall.Object is not { }) throw new NotSupportedException($"Static call {methodCall} is not supported.");
            var obj = Visit(methodCall.Object);
            var arg = Visit(methodCall.Arguments[0]);
            return $"contains({obj}, {arg})";
        }

        private string VisitConcat(MethodCallExpression methodCall)
        {
            var parts = methodCall.Arguments.Select(Visit);
            return $"concat({string.Join(", ", parts)})";
        }

        private string FormatValue(object? value)
        {
            if (value == null)
                return "null";

            if (value is string str)
                return $"'{str.Replace("'", "''")}'";

            if (value is bool b)
                return b.ToString().ToLowerInvariant();

            if (value is DateTime dt)
                return dt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            if (value is DateTimeOffset dto)
                return dto.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            if (value is Guid guid)
                return guid.ToString();

            // For other types, try to convert to string
            return value.ToString() ?? "";
        }

        private string BuildPropertyPath(MemberExpression member)
        {
            var parts = new List<string>();
            Expression? current = member;

            while (current is MemberExpression memberExpr)
            {
                parts.Insert(0, memberExpr.Member.Name);
                current = memberExpr.Expression;
            }

            // If we stopped at a ParameterExpression, we have the full path
            // If we stopped at a ConstantExpression, it's a captured variable - evaluate it
            if (current is ConstantExpression)
            {
                // This should have been caught earlier, but handle it just in case
                return member.Member.Name;
            }

            // Return the path, joining with '/' for OData navigation
            return string.Join("/", parts);
        }

    }
}
