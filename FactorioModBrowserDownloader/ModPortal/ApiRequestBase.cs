using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json.Serialization;

namespace FactorioModBrowserDownloader.ModPortal
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

        public virtual string ToUrlParameters()
        {
            IEnumerable<string> parameters = GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(property => property.GetCustomAttribute<JsonIgnoreAttribute>()?.Condition != JsonIgnoreCondition.Always)
                .Select(property =>
                {
                    string propertyName = property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name;
                    object? propertyValue = property.GetValue(this, null);
                    return propertyValue == null ? null! : string.Format("{0}={1}", propertyName, propertyValue);
                });

            return parameters.Any() ? "?" + string.Join("&", parameters.Where(p => p != null)) : string.Empty;
        }
    }
}
