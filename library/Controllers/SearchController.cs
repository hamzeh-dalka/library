using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using library.Interfaces;

namespace library.Controllers
{

    [Route("api/Search")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly LibraryDbContext _dbcontext;
        private readonly IAIService _aiService;

        public SearchController(LibraryDbContext context, IAIService aiService)
        {
            _dbcontext = context;
            _aiService = aiService;
        }

        [HttpGet("smart-search")]
        public async Task<IActionResult> SmartSearch([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Please enter a search query.");
            }

            float[] queryVector = await _aiService.GenerateEmbeddingAsync(query);

            var books = await _dbcontext.Books
                .Where(b => b.Embedding != null)
                .ToListAsync();

            var rankedBooks = books
                .Select(b => new
                {
                    Book = b,
                    Similarity = CalculateCosineSimilarity(
                        queryVector,
                        JsonSerializer.Deserialize<float[]>(b.Embedding!)
                    )
                })
                .OrderByDescending(x => x.Similarity)
                .Take(5)
                .Select(x => new
                {
                    x.Book.Id,
                    x.Book.Title,
                    x.Book.Author,
                    x.Similarity
                })
                .ToList();

            return Ok(rankedBooks);
        }

        private double CalculateCosineSimilarity(float[] vector1, float[] vector2)
        {
            if (vector1 == null || vector2 == null || vector1.Length != vector2.Length)
                return 0;

            double dotProduct = 0;
            double magnitude1 = 0;
            double magnitude2 = 0;

            for (int i = 0; i < vector1.Length; i++)
            {
                dotProduct += vector1[i] * vector2[i];
                magnitude1 += Math.Pow(vector1[i], 2);
                magnitude2 += Math.Pow(vector2[i], 2);
            }

            if (magnitude1 == 0 || magnitude2 == 0)
                return 0;

            return dotProduct / (Math.Sqrt(magnitude1) * Math.Sqrt(magnitude2));
        }
    }
}