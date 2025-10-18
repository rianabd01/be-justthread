using System;
using System.Web;

namespace be_justthread.Services
{
    public static class DbConnectionHelper
    {
        // Accepts either a key=value connection string or a postgres:// URI and returns
        // a key=value string that Npgsql can understand.
        public static string Normalize(string conn)
        {
            if (string.IsNullOrWhiteSpace(conn)) return conn;

            var trimmed = conn.Trim();
            if (trimmed.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
            {
                // parse as URI
                var uri = new Uri(trimmed);

                var userInfo = uri.UserInfo.Split(':');
                var username = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : "";
                var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";

                var host = uri.Host;
                var port = uri.IsDefaultPort ? 5432 : uri.Port;
                // path starts with /{database}
                var database = uri.AbsolutePath.Length > 1 ? Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')) : "postgres";

                // parse query parameters
                var query = uri.Query;
                var qp = HttpUtility.ParseQueryString(query);

                // map sslmode if present
                var sslmode = qp["sslmode"] ?? qp["ssl-mode"];

                var parts = $"Host={host};Port={port};Database={database};Username={username};Password={password}";
                if (!string.IsNullOrEmpty(sslmode))
                {
                    // Npgsql uses "Ssl Mode" or "SSL Mode"
                    parts += $";SSL Mode={sslmode}";
                    // for convenience in dev allow trusting server certs
                    parts += ";Trust Server Certificate=true";
                }

                return parts;
            }

            return conn;
        }
    }
}
