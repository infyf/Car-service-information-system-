using System.Security.Claims;

namespace AutoServ.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var claim = user.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 0;
        }

        public static string GetRole(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Role)?.Value ?? "client";
        }

        public static int? GetMasterId(this ClaimsPrincipal user)
        {
            var claim = user.FindFirst("MasterId");
            return claim != null ? int.Parse(claim.Value) : null;
        }

        public static string GetEmail(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Email)?.Value ?? "";
        }
    }
}