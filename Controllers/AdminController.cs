using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace be_justthread.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        public AdminController(IConfiguration config, IWebHostEnvironment env)
        {
            _config = config;
            _env = env;
        }

        // Development-only endpoint to initialize the DB (creates threads table and seeds sample rows)
        [HttpPost("init-db")]
        public IActionResult InitDb()
        {
            if (!_env.IsDevelopment() && _config["Admin:AllowInit"] != "true")
                return Forbid();

            var raw = _config.GetConnectionString("DefaultConnection") ?? _config["ConnectionStrings:DefaultConnection"];
            var connStr = be_justthread.Services.DbConnectionHelper.Normalize(raw ?? string.Empty);
            if (string.IsNullOrWhiteSpace(connStr)) return BadRequest("Connection string not configured.");

            using var conn = new NpgsqlConnection(connStr);
            conn.Open();

            var createSql = @"
CREATE TABLE IF NOT EXISTS threads (
    id serial PRIMARY KEY,
    title text NOT NULL,
    content text NOT NULL,
    created_at timestamptz NOT NULL DEFAULT now()
);
";

            using (var cmd = new NpgsqlCommand(createSql, conn)) cmd.ExecuteNonQuery();

            // seed sample rows if table empty
            using (var check = new NpgsqlCommand("SELECT COUNT(1) FROM threads", conn))
            {
                var cnt = Convert.ToInt64(check.ExecuteScalar() ?? 0L);
                if (cnt == 0)
                {
                    using var ins = new NpgsqlCommand("INSERT INTO threads (title, content) VALUES (@t, @c)", conn);
                    ins.Parameters.AddWithValue("@t", "Welcome to JustThread");
                    ins.Parameters.AddWithValue("@c", "This is the first seeded thread.");
                    ins.ExecuteNonQuery();

                    ins.Parameters.Clear();
                    ins.Parameters.AddWithValue("@t", "Second post");
                    ins.Parameters.AddWithValue("@c", "Another seeded thread to show listing.");
                    ins.ExecuteNonQuery();
                }
            }

            return Ok(new { status = "initialized" });
        }
    }
}
