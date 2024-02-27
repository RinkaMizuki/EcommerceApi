using OpenAI_API;

namespace EcommerceApi.Services.OpenaiService
{
    public class OpenaiService : IOpenaiService
    {
        private readonly string _apiKey;

        public OpenaiService(IConfiguration configuration)
        {
            _apiKey = configuration.GetSection("OpenaiApiKey").Value!;
        }

        public async Task<string> GetChatGPTResponse(string prompt)
        {
            try
            {
                var openai = new OpenAIAPI(_apiKey);

                // Gửi yêu cầu đến OpenAI API và nhận câu trả lời
                var response = await openai.Completions.GetCompletion(prompt);

                return response ?? "No response";
            }
            catch(Exception ex) { 
                throw new Exception(ex.Message);
            }
        }
    }
}
