namespace EcommerceApi.Services.OpenaiService
{
    public interface IOpenaiService
    {
        Task<string> GetChatGPTResponse(string prompt);
    }
}
