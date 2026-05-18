namespace IdentityService.Application.DTOs
{
    public record RegisterRequest(
     string FullName,
     string Email,
     string Password,
     string PhoneNumber,
     UserRoleDto Role
 );

    public record LoginRequest(
        string Email,
        string Password
    );

    public record AuthResponse(
        string Token,
        string FullName,
        string Email,
        string Role,
        DateTime ExpiresAt
    );

    public enum UserRoleDto
    {
        Customer = 1,
        Company = 2,
        Driver = 3
    }
}
