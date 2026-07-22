namespace library.Interfaces
{
    public interface IAIService
    {
        Task<float[]> GenerateEmbeddingAsync(string text);
    }
}
