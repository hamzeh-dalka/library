using library.Interfaces;
using OpenAI.Embeddings;
using Microsoft.Extensions.Configuration;

namespace library.AIService
{
    public class AIService : IAIService
    {
        private readonly EmbeddingClient _client;

        public AIService(IConfiguration configuration)
        {
            string apiKey = configuration.GetValue<string>("OpenAI:ApiKey");

            if(string.IsNullOrEmpty(apiKey))
            {
                throw new ArgumentException("OpenAI API key is not configured.");
            }

            _client = new EmbeddingClient("text-embedding-3-small",(apiKey));
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text)
        {
            OpenAIEmbedding embedding = await _client.GenerateEmbeddingAsync(text);

            return embedding.ToFloats().ToArray();
        }
    }
}
