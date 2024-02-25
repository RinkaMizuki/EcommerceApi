using System.IdentityModel.Tokens.Jwt;

namespace EcommerceApi
{
    public static class Helpers
    {
        public static List<T> ParseString<T>(string list)
        {
            if (string.IsNullOrEmpty(list)) return new List<T>();
            if ((list.StartsWith("[") && list.EndsWith("]")) || (list.StartsWith("{") && list.EndsWith("}")) ||
                list.StartsWith("#"))
            {
                if (list == "{}" || list == "[]") return new List<T>();
                if (list.StartsWith("['") && list.EndsWith("']"))
                {
                    list = list.Substring(1, list.Length - 2);
                }
                else
                {
                    list = list.Substring(1, list.Length - 2);
                }

                var splitOperator = new[] { ':', ',' };
                var listValues = list.Split(splitOperator).ToList();

                for (var i = 0; i < listValues.Count; i++)
                {
                    if (listValues[i].StartsWith("[") || listValues[i].EndsWith("]"))
                    {
                        listValues[i] = listValues[i].Trim('[');
                        listValues[i] = listValues[i].Trim(']');
                        listValues[i] = listValues[i].Trim('"');
                    }
                    else if (listValues[i].StartsWith("[\"") && listValues[i].EndsWith("\"]"))
                    {
                        listValues[i] = listValues[i].Substring(2, listValues[i].Length - 4);
                    }
                    else if (listValues[i].StartsWith("\"") && listValues[i].EndsWith("\""))
                    {
                        listValues[i] = listValues[i].Trim('"');
                    }
                    else
                    {
                        listValues[i] = listValues[i].Trim('\'');
                    }
                }

                return listValues.Select(elm => (T)Convert.ChangeType(elm, typeof(T))).ToList();
            }

            return new List<T>(); // Return an empty List if parsing fails
        }

        public static string GetUserNameLogin(HttpContext httpContext)
        {
            var jwt = httpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var userName = string.Empty;
            if (jwt != null)
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(jwt) as JwtSecurityToken;
                userName = jsonToken?.Claims
                    .FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.UniqueName)?.Value;
            }

            return userName!;
        }
        public static List<T> GetRandomElements<T>(List<T> list, int count)
        {
            Random random = new ();
            return list.OrderBy(item => random.Next()).Take(count).ToList();
        }
    }
}