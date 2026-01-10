using AutoMapper;
using TechsysLog.Application.Common;
using TechsysLog.Application.DTOs;
using TechsysLog.Application.Interfaces;
using TechsysLog.Domain.Common;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Interfaces;
using TechsysLog.Domain.ValueObjects;

namespace TechsysLog.Application.Commands.Users;

/// <summary>
/// Handler for CreateUserCommand.
/// Creates a new user with hashed password.
/// </summary>
public sealed class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMapper _mapper;

    public CreateUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
    }

    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Create email value object
        var emailResult = Email.Create(request.Email);
        if (emailResult.IsFailure)
            return Result.Failure<UserDto>(emailResult.Error!);

        // Check if email already exists
        if (await _userRepository.EmailExistsAsync(emailResult.Value, cancellationToken))
            return Result.Failure<UserDto>("Email is already registered.");

        // Create password value object with hash
        var passwordResult = Password.Create(request.Password, _passwordHasher.Hash);
        if (passwordResult.IsFailure)
            return Result.Failure<UserDto>(passwordResult.Error!);

        // Create user entity
        var userResult = User.Create(
            request.Name,
            emailResult.Value,
            passwordResult.Value,
            request.Role);

        if (userResult.IsFailure)
            return Result.Failure<UserDto>(userResult.Error!);

        // Persist user
        await _userRepository.AddAsync(userResult.Value, cancellationToken);

        // Map to DTO and return
        var userDto = _mapper.Map<UserDto>(userResult.Value);
        return Result.Success(userDto);
    }
}