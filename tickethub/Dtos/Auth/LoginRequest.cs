namespace tickethub.Dtos.Auth;

public record LoginRequest(
    string Username,
    string Email,
    string Password
);