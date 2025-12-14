using System.Net;
using System.Reflection;

namespace astratech_apps_backend.Helpers
{
    public static class SanitizerHelper
    {
        public static T? EncodeObject<T>(T obj) where T : class
        {
            if (obj == null)
            {
                return null;
            }

            var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (property.CanWrite && property.PropertyType == typeof(string))
                {
                    var value = (string?)property.GetValue(obj);
                    if (!string.IsNullOrEmpty(value) && !property.Name.Equals("Password"))
                    {
                        var encodedValue = WebUtility.HtmlEncode(value);
                        property.SetValue(obj, encodedValue);
                    }
                }
            }
            return obj;
        }

        public static string EncodeString(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }
            return WebUtility.HtmlEncode(input);
        }
    }
}
