using EcommerceApi.Attributes;
using EcommerceApi.Dtos.User;

namespace EcommerceApi.Services.SsoService
{
    public interface ISsoService
    {
        public Task<string> SsoTokenVerify(string token);
    }
}
