using Contracts.Core;
namespace CommonUtils.JWT;
public interface IJWTService
{
    UserToken GetUserToken(JWTUserModel user);
}
