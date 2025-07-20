using VerticalSliceArchitecture.Application.Domain.Users;

namespace VerticalSliceArchitecture.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
