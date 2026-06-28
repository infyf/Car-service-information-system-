using AutoServ.Core.Entities;

namespace AutoServ.Core.Interfaces
{
    public interface ITokenService
    {
        string GenerateJwtToken(User user);
    }
}