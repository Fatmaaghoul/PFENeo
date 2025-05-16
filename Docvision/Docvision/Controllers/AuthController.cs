using Docvision.Dtos;
using Docvision.Models;
using Docvision.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace Docvision.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager; 

        public AuthController(IAuthService authService,
            UserManager<ApplicationUser> userManager)
        {
            _authService = authService;
            _userManager = userManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Dtos.RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Dtos.LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            return Ok(result);
        }


        [HttpGet("confirm-email")] 
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var result = await _authService.ConfirmEmailAsync(userId, token);
            return Ok(result);
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("AuthToken");
            return Ok("Déconnexion réussie");
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] Dtos.ForgotPasswordRequest request)
        {
            var result = await _authService.ForgotPasswordAsync(request.Email);
            return Ok(result);
        }



        [HttpGet("reset-password")]
        public IActionResult ResetPasswordPage([FromQuery] string token, [FromQuery] string email)
        {
            try
            {
                // Validation basique
                if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(email))
                    return BadRequest("Token and email are required");

                // Décodage spécial pour les tokens Identity
                var cleanedToken = Uri.UnescapeDataString(token)
                                    .Replace(" ", "+"); // Correction critique pour les tokens Identity

                // Redirection vers le frontend avec token nettoyé
                return Redirect($"http://localhost:5173/reset-password?token={Uri.EscapeDataString(cleanedToken)}&email={Uri.EscapeDataString(email)}");
            }
            catch (Exception ex)
            {
                return BadRequest($"Invalid token: {ex.Message}");
            }
        }
        [HttpGet("validate-reset-token")]
        public async Task<IActionResult> ValidateResetToken([FromQuery] string token, [FromQuery] string email)
        {
            try
            {
                Console.WriteLine($"Token reçu: {token}");
                Console.WriteLine($"Email reçu: {email}");

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    Console.WriteLine("Utilisateur non trouvé");
                    return Ok(new { valid = false, message = "Utilisateur non trouvé" });
                }

                var cleanedToken = Uri.UnescapeDataString(token).Replace(" ", "+");
                Console.WriteLine($"Token nettoyé: {cleanedToken}");

                var isValid = await _userManager.VerifyUserTokenAsync(
                    user,
                    _userManager.Options.Tokens.PasswordResetTokenProvider,
                    "ResetPassword",
                    cleanedToken);

                Console.WriteLine($"Token valide: {isValid}");

                return Ok(new { valid = isValid, message = isValid ? "Token valide" : "Token invalide ou expiré" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur de validation: {ex}");
                return StatusCode(500, new { valid = false, message = $"Erreur de validation: {ex.Message}" });
            }
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] Dtos.ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Nettoyage du token avant utilisation
                var cleanedToken = Uri.UnescapeDataString(request.Token)
                                    .Replace(" ", "+");

                var result = await _authService.ResetPasswordAsync(new Dtos.ResetPasswordRequest
                {
                    Email = request.Email,
                    Token = cleanedToken,
                    NewPassword = request.NewPassword
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var result = await _authService.RefreshTokenAsync(request.RefreshToken);
            return Ok(result);
        }
    }


}
