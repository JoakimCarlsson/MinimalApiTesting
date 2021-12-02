using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MinimalApiTesting.Business.Endpoints;

namespace MinimalApiTesting.Endpoints
{
    public class HealthCheck : IEndpointDefinition
    {
        public void DefineEndpoints(WebApplication app)
        {
            app.MapGet("/api/health", CheckHealth).WithTags("Health")
                .Produces(200)
                .ProducesProblem(503)
                .ProducesProblem(401);
        }

        private async Task<IResult> CheckHealth([FromServices] HealthCheckService healthCheckService)
        {
            var report = await healthCheckService.CheckHealthAsync();
            return report.Status == HealthStatus.Healthy ? Results.Ok(report) : Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
        }
    }
}
