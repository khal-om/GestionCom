using GestionCom.DTOs.Auth;
using GestionCom.Entities;
using GestionCom.Services.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;

namespace GestionCom.Controllers
{ 
    [ApiController]
    [Route("api/auth")]
    public class AuthController : Controller
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [Consumes("application/json")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _authService.LoginAsync(dto.Username, dto.Password);

            if (user == null)
                return Unauthorized();

            await SignInUserAsync(user);

            return Ok();
        }

        [HttpPost("login")]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public async Task<IActionResult> LoginForm([FromForm] LoginDto dto, [FromQuery] string? returnUrl)
        {
            var user = await _authService.LoginAsync(dto.Username, dto.Password);

            if (user == null)
                return LocalRedirect(BuildLoginRedirectUrl(returnUrl, hasError: true));

            await SignInUserAsync(user);

            return LocalRedirect(GetSafeReturnUrl(returnUrl));
        }

        private async Task SignInUserAsync(User user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Role, user.Role.Name)
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14),
                AllowRefresh = true
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProperties);
        }

        private string GetSafeReturnUrl(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return returnUrl;

            return "/";
        }

        private string BuildLoginRedirectUrl(string? returnUrl, bool hasError)
        {
            var query = new Dictionary<string, string?>();

            if (hasError)
                query["error"] = "1";

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                query["returnUrl"] = returnUrl;

            return query.Count == 0
                ? "/login"
                : QueryHelpers.AddQueryString("/login", query);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Ok();
        }

        [HttpGet("logout")]
        public async Task<IActionResult> LogoutGet()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return LocalRedirect("/login");
        }
    }

}
