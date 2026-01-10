using AutoMapper;
using TechsysLog.Application.Common;
using TechsysLog.Application.DTOs;
using TechsysLog.Application.Interfaces;
using TechsysLog.Domain.Common;
using TechsysLog.Domain.Interfaces;
using TechsysLog.Domain.ValueObjects;

namespace TechsysLog.Application.Commands.Users;

/// <summary>
/// Handler for LoginCommand.
/// Authenticates user and returns JWT token.
/// </summary>
public sealed class LoginCommandHandler : ICommandHandler<LoginCommand, AuthDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IMapper _mapper;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _mapper = mapper;
    }

    public async Task<Result<AuthDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Create email value object
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
            return Result.Failure<AuthDto>("Invalid credentials.");

        // Find user by email
        var user = await _userRepository.GetByEmailAsync(emailResult.Value, cancellationToken);
        if (user is null)
            return Result.Failure<AuthDto>("Invalid credentials.");

        // Verify password
        if (!_passwordHasher.Verify(request.Password, user.Password.Hash))
            return Result.Failure<AuthDto>("Invalid credentials.");

        // Generate JWT token
        var token = _jwtService.GenerateToken(user);
        var expiresIn = _jwtService.GetExpirationInSeconds();

        // Map user to DTO
        var userDto = _mapper.Map<UserDto>(user);

        return Result.Success(new AuthDto
        {
            AccessToken = token,
            TokenType = "Bearer",
            ExpiresIn = expiresIn,
            User = userDto
        });
    }
}