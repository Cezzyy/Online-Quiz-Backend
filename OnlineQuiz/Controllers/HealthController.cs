using Microsoft.AspNetCore.Mvc;
using OnlineQuiz.Data;

namespace OnlineQuiz.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Tags("Health")]
    public class HealthController : ControllerBase
    {
        private readonly OnlineQuizDbContext _context;

        public HealthController(OnlineQuizDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Health check endpoint to verify API availability and database connectivity
        /// </summary>
        /// <returns>API health status including database connection</returns>
        [HttpGet]
        public async Task<IActionResult> HealthCheck()
        {
            var healthResponse = new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                service = "OnlineQuiz API",
                version = "1.0.0",
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                database = new { status = "healthy", connected = true }
            };

            try
            {
                // Check database connectivity
                var canConnect = await _context.Database.CanConnectAsync();
                
                if (!canConnect)
                {
                    return StatusCode(503, new
                    {
                        status = "unhealthy",
                        timestamp = DateTime.UtcNow,
                        service = "OnlineQuiz API",
                        version = "1.0.0",
                        environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                        database = new { status = "unhealthy", connected = false, error = "Database connection failed" }
                    });
                }

                return Ok(healthResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(503, new
                {
                    status = "unhealthy",
                    timestamp = DateTime.UtcNow,
                    service = "OnlineQuiz API",
                    version = "1.0.0",
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                    database = new { status = "unhealthy", connected = false, error = ex.Message }
                });
            }
        }
    }
}