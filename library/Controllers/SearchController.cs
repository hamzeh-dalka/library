using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using library.Interfaces;
using Microsoft.AspNetCore.Authorization;

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

        [Authorize(Roles = "Student")]
        [HttpGet("smart-search")]
        public async Task<IActionResult> SmartSearch([FromQuery] string query, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Please enter a search query.");
            }

            try
            {
                float[] queryVector = await _aiService.GenerateEmbeddingAsync(query);

                var books = await _dbcontext.Books
                    .Where(b => b.Embedding != null)
                    .Select(b => new { b.Id, b.Title, b.Author, b.Embedding })
                    .ToListAsync(ct);

                var rankedBooks = books
                    .Select(b => new
                    {
                        b.Id,
                        b.Title,
                        b.Author,
                        Similarity = CalculateCosineSimilarity(
                            queryVector,
                            JsonSerializer.Deserialize<float[]>(b.Embedding!)
                        )
                    })
                    .OrderByDescending(x => x.Similarity)
                    .Take(5)
                    .ToList();

                return Ok(rankedBooks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"AI Search Error: {ex.Message}");
            }
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