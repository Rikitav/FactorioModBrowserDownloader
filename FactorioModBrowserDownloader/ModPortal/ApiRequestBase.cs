using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

namespace FactorioNexus.ModPortal
{
    public abstract class ApiRequestBase<TResponse> where TResponse : class
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public HttpMethod HttpMethod { get; private set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public string MethodName { get; private set; }

        protected ApiRequestBase(HttpMethod httpMethod, params string[] methodPath)
        {
            HttpMethod = httpMethod;
            MethodName = Path.Combine(methodPath);
        }

        protected ApiRequestBase(params string[] methodPath)
        {
            HttpMethod = HttpMethod.Get;
            MethodName = Path.Combine(methodPath);
        }

        public virtual void BuildParameters(StringBuilder builder)
        {
            IEnumerable<PropertyInfo> properties = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            if (!properties.Any())
                return;

            properties = properties.Where(property => property.GetCustomAttribute<JsonIgnoreAttribute>()?.Condition != JsonIgnoreCondition.Always);
            if (!properties.Any())
                return;

            for (int i = 0; i < properties.Count(); i++)
            {
                PropertyInfo property = properties.ElementAt(i);
                string propertyName = property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name;
                object? propertyValue = property.GetValue(this, null);

                if (propertyValue == null)
                    continue;

                if (propertyValue is string str && string.IsNullOrEmpty(str))
                    continue;

                builder.Append(i == 0 ? '?' : '&').Append(propertyName).Append('=').Append(FormatParameterValue(propertyValue));
            }
        }

        protected virtual string? FormatParameterValue(object propertyValue)
            => propertyValue.ToString();
    }
}
