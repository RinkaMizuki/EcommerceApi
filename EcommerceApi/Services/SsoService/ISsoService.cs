namespace EcommerceApi.Services.SsoService
{
    public interface ISsoService
    {
        public Task<string> SsoDefaultTokenVerify(string token);
        public Task<string> SsoFacebookTokenVerify(string token);
        public Task<string> SsoGoogleTokenVerify(string token);
    }
}
