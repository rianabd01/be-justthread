using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using be_justthread.Services;
using be_justthread.Models;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using Dapper;

namespace be_justthread.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ThreadsController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly JwtValidator _jwt;

        public ThreadsController(IConfiguration config, JwtValidator jwt)
        {
            _config = config;
            _jwt = jwt;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            // var auth = Request.Headers["Authorization"].FirstOrDefault();
            // if (string.IsNullOrEmpty(auth) || !auth.StartsWith("Bearer "))
            //     return Unauthorized();

            // var token = auth.Substring("Bearer ".Length).Trim();
            // try
            // {
            //     var principal = _jwt.Validate(token);
            // }
            // catch
            // {
            //     return Unauthorized();
            // }

            var raw = _config.GetConnectionString("DefaultConnection") ?? _config["ConnectionStrings:DefaultConnection"];
            var connStr = be_justthread.Services.DbConnectionHelper.Normalize(raw ?? string.Empty);

            try
            {
                await using var conn = new NpgsqlConnection(connStr);
                var sql = @"SELECT id, title, content, created_at as CreatedAt FROM threads ORDER BY created_at DESC LIMIT 50";
                var threads = await conn.QueryAsync<be_justthread.Models.Thread>(sql);
                return Ok(threads);
            }
            catch (System.Exception ex)
            {
                // return a helpful error for debugging; in production don't return sensitive info
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }
    }
}
