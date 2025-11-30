using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace EcoRecyclersGreenTech.Services
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Generate nonce for each request
            var nonce = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            context.Items["CSPNonce"] = nonce;

            // Setting up CSP with inline scripts allowed using nonce
            context.Response.Headers["Content-Security-Policy"] =
                $"default-src 'self'; " +
                $"script-src 'self' 'nonce-{nonce}'; " +
                $"style-src 'self' 'nonce-{nonce}'; " +
                $"img-src 'self' data:; " +
                $"font-src 'self'; " +
                $"connect-src 'self'; " +
                $"frame-ancestors 'none'; " +
                $"base-uri 'self';";

            // Additional security headers
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["X-Frame-Options"] = "DENY";
            context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=()";

            // Redirecting authentication pages after successful authentication
            var path = context.Request.Path.Value?.ToLower();
            if (context.User?.Identity != null && context.User.Identity.IsAuthenticated)
            {
                if (path != null &&
                   (path.StartsWith("/auth/login") || path.StartsWith("/auth/signup")))
                {
                    context.Response.Redirect("/home/index");
                    return;
                }
            }

            await _next(context);
        }
    }
}
