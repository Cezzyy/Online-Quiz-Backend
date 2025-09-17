using Microsoft.AspNetCore.Mvc;
using OnlineQuiz.Data;
using Microsoft.EntityFrameworkCore;

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
        /// Basic health check endpoint to verify API availability
        /// </summary>
        /// <returns>API health status</returns>
        [HttpGet]
        public IActionResult HealthCheck()
        {
            return Ok(new 
            { 
                status = "healthy",
                timestamp = DateTime.UtcNow,
                service = "OnlineQuiz API",
                version = "1.0.0",
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
            });
        }

        /// <summary>
        /// Detailed health check including database connectivity
        /// </summary>
        /// <returns>Detailed health status including dependencies</returns>
        [HttpGet("detailed")]
        public async Task<IActionResult> DetailedHealthCheck()
        {
            var healthStatus = new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                service = "OnlineQuiz API",
                version = "1.0.0",
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                checks = new Dictionary<string, object>()
            };

            // Check database connectivity
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                healthStatus.checks.Add("database", new 
                { 
                    status = canConnect ? "healthy" : "unhealthy",
                    responseTime = DateTime.UtcNow,
                    details = canConnect ? "Database connection successful" : "Database connection failed"
                });
            }
            catch (Exception ex)
            {
                healthStatus.checks.Add("database", new 
                { 
                    status = "unhealthy",
                    responseTime = DateTime.UtcNow,
                    details = $"Database connection error: {ex.Message}"
                });
            }

            // Check memory usage
            var workingSet = Environment.WorkingSet;
            healthStatus.checks.Add("memory", new 
            { 
                status = workingSet < 500_000_000 ? "healthy" : "warning", // 500MB threshold
                workingSetBytes = workingSet,
                workingSetMB = Math.Round(workingSet / 1024.0 / 1024.0, 2)
            });

            // Check disk space (if needed)
            try
            {
                var drives = DriveInfo.GetDrives().Where(d => d.IsReady);
                var diskInfo = drives.Select(d => new 
                {
                    name = d.Name,
                    availableSpaceGB = Math.Round(d.AvailableFreeSpace / 1024.0 / 1024.0 / 1024.0, 2),
                    totalSpaceGB = Math.Round(d.TotalSize / 1024.0 / 1024.0 / 1024.0, 2),
                    percentFree = Math.Round((double)d.AvailableFreeSpace / d.TotalSize * 100, 2)
                }).ToList();

                healthStatus.checks.Add("diskSpace", new 
                { 
                    status = "healthy",
                    drives = diskInfo
                });
            }
            catch (Exception ex)
            {
                healthStatus.checks.Add("diskSpace", new 
                { 
                    status = "warning",
                    details = $"Could not retrieve disk information: {ex.Message}"
                });
            }

            // Determine overall status
            var hasUnhealthy = healthStatus.checks.Values.Any(check => 
                check.GetType().GetProperty("status")?.GetValue(check)?.ToString() == "unhealthy");
            
            if (hasUnhealthy)
            {
                return StatusCode(503, new { 
                    status = "unhealthy", 
                    timestamp = healthStatus.timestamp,
                    service = healthStatus.service,
                    checks = healthStatus.checks 
                });
            }

            return Ok(healthStatus);
        }

        /// <summary>
        /// Readiness probe for Kubernetes/container orchestration
        /// </summary>
        /// <returns>Service readiness status</returns>
        [HttpGet("ready")]
        public async Task<IActionResult> ReadinessCheck()
        {
            try
            {
                // Check if database is accessible
                var canConnect = await _context.Database.CanConnectAsync();
                
                if (!canConnect)
                {
                    return StatusCode(503, new { 
                        status = "not ready", 
                        reason = "Database not accessible",
                        timestamp = DateTime.UtcNow 
                    });
                }

                return Ok(new { 
                    status = "ready", 
                    timestamp = DateTime.UtcNow,
                    service = "OnlineQuiz API"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(503, new { 
                    status = "not ready", 
                    reason = ex.Message,
                    timestamp = DateTime.UtcNow 
                });
            }
        }

        /// <summary>
        /// Liveness probe for Kubernetes/container orchestration
        /// </summary>
        /// <returns>Service liveness status</returns>
        [HttpGet("live")]
        public IActionResult LivenessCheck()
        {
            // Simple liveness check - if this endpoint responds, the service is alive
            return Ok(new { 
                status = "alive", 
                timestamp = DateTime.UtcNow,
                service = "OnlineQuiz API"
            });
        }
    }
}