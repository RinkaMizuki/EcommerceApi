using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace EcommerceApi
{
    public static class Helpers
    {
        public static Lazy<Dictionary<string, X509Certificate2>> Certificates = new Lazy<Dictionary<string, X509Certificate2>>(FetchGoogleCertificates);
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
            if(string.IsNullOrEmpty(jwt))
            {
                jwt = httpContext.Request.Cookies["accessToken"]!;
            }
            var userName = string.Empty;
            if (jwt != null)
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(jwt) as JwtSecurityToken;
                userName = jsonToken?.Claims
                    .FirstOrDefault(claim => claim.Type == "username")?.Value;
            }

            return userName!;
        }
        public static string GetUserRoleLogin(HttpContext httpContext)
        {
            var jwt = httpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var role = string.Empty;
            if (jwt != null)
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(jwt) as JwtSecurityToken;
                role = jsonToken?.Claims
                    .FirstOrDefault(claim => claim.Type == "Role")?.Value;
            }

            return role!;
        }
        public static List<T> CreatePaging<T>(List<T> list, List<int> rangeValues, int currentPage, int perPage, string type, HttpResponse response)
        {
            var totalCount = list.Count;
            list = list
                .Skip((currentPage - 1) * perPage)
                .Take(perPage)
                .ToList();
            response.Headers.Append("Access-Control-Expose-Headers", "Content-Range");
            response.Headers.Append("Content-Range", $"{type} {rangeValues[0]}-{rangeValues[1]}/{totalCount}");
            return list;
        }
        public static List<T> GetRandomElements<T>(List<T> list, int count)
        {
            Random random = new ();
            return list.OrderBy(item => random.Next()).Take(count).ToList();
        }

        public static int CalcPriceSale(decimal price, int percent)
        {
            return Convert.ToInt32(Convert.ToInt32(price) * (1 - (float)percent / 100));
        }
        public static Dictionary<string, X509Certificate2> FetchGoogleCertificates()
        {
            using (var http = new HttpClient())
            {
                var json = http.GetStringAsync("https://www.googleapis.com/oauth2/v1/certs").Result;

                var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                return dictionary.ToDictionary(x => x.Key, x => new X509Certificate2(Encoding.UTF8.GetBytes(x.Value)));
            }
        }
    }
}