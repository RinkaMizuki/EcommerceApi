using System.IdentityModel.Tokens.Jwt;

namespace EcommerceApi
{
    public static class Helpers
    {
        public static List<T> ParseString<T>(string list)
        {
            if ((list.StartsWith("[") && list.EndsWith("]")) || (list.StartsWith("{") && list.EndsWith("}")))
            {
                if (list == "{}" || list == "[]") return new List<T>();
                list = list.Substring(1, list.Length - 2);
                List<string> listValues = new List<string>();
                char[] splitOperator = new char[] { ':', ',' };
                listValues = list.Split(splitOperator).ToList();
                for (int i = 0; i < listValues.Count; i++)
                {
                    if (listValues[i].StartsWith("\"") && listValues[i].EndsWith("\""))
                    {
                        listValues[i] = listValues[i].Trim('"');
                    }
                }

                return listValues.Select(elm => (T)Convert.ChangeType(elm, typeof(T))).ToList();
            }

            return new List<T>(); // Return an empty array if parsing fails
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
            return userName;
        }
    }
}
