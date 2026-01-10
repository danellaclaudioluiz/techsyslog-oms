using TechsysLog.Application.Common;
using TechsysLog.Application.DTOs;

namespace TechsysLog.Application.Queries.Users;

/// <summary>
/// Query to get a user by ID.
/// </summary>
public sealed record GetUserByIdQuery : IQuery<UserDto?>
{
    public Guid UserId { get; init; }
}