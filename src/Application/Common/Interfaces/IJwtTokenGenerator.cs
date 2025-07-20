namespace VerticalSliceArchitecture.Application.Common.Interfaces;

using VerticalSliceArchitecture.Application.Domain.Users;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
