using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SmartTicket.API.Helpers
{
    public static class JwtHelper
    {
        public static int GetUserId(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) 
                           ?? user.FindFirst(JwtRegisteredClaimNames.Sub);
            
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid user token");
            }

            return userId;
        }

        public static string GetUserRole(ClaimsPrincipal user)
        {
            var roleClaim = user.FindFirst(ClaimTypes.Role);
            return roleClaim?.Value ?? string.Empty;
        }

        public static string GetUserEmail(ClaimsPrincipal user)
        {
            var emailClaim = user.FindFirst(ClaimTypes.Email) 
                          ?? user.FindFirst(JwtRegisteredClaimNames.Email);
            return emailClaim?.Value ?? string.Empty;
        }
    }
}