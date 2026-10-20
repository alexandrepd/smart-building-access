using SmartBuilding.Application.Users;

namespace SmartBuilding.Application.Authentication;

public interface IAccessTokenService
{
    AccessTokenDto CreateToken(UserDto user);
}
