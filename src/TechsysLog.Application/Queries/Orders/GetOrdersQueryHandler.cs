using System.Text;
using System.Text.Json;
using AutoMapper;
using TechsysLog.Application.Common;
using TechsysLog.Application.DTOs;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.Interfaces;

namespace TechsysLog.Application.Queries.Orders;

/// <summary>
/// Handler for GetOrdersQuery.
/// Implements cursor-based pagination for efficient large dataset handling.
/// </summary>
public sealed class GetOrdersQueryHandler : IQueryHandler<GetOrdersQuery, PagedResult<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetOrdersQueryHandler(
        IOrderRepository orderRepository,
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        // Decode cursor if provided
        Guid? cursorId = null;
        if (!string.IsNullOrEmpty(request.Cursor))
        {
            cursorId = DecodeCursor(request.Cursor);
        }

        // Build filter expression
        var orders = await GetFilteredOrdersAsync(request, cursorId, cancellationToken);

        // Get total count for the filter
        var totalCount = await GetTotalCountAsync(request, cancellationToken);

        // Take limit + 1 to check if there are more results
        var orderList = orders.ToList();
        var hasMore = orderList.Count > request.Limit;

        if (hasMore)
        {
            orderList = orderList.Take(request.Limit).ToList();
        }

        // Generate next cursor
        string? nextCursor = null;
        if (hasMore && orderList.Any())
        {
            var lastOrder = orderList.Last();
            nextCursor = EncodeCursor(lastOrder.Id);
        }

        // Map to DTOs
        var orderDtos = _mapper.Map<List<OrderDto>>(orderList);

        return PagedResult<OrderDto>.Create(
            orderDtos,
            nextCursor,
            hasMore,
            request.Limit,
            totalCount);
    }

    private async Task<IEnumerable<Order>> GetFilteredOrdersAsync(
        GetOrdersQuery request,
        Guid? cursorId,
        CancellationToken cancellationToken)
    {
        IEnumerable<Order> orders;

        if (request.UserId.HasValue)
        {
            orders = await _orderRepository.GetByUserIdAsync(request.UserId.Value, cancellationToken);
        }
        else if (request.Status.HasValue)
        {
            orders = await _orderRepository.GetByStatusAsync(request.Status.Value, cancellationToken);
        }
        else
        {
            orders = await _orderRepository.GetAllAsync(cancellationToken);
        }

        // Apply both filters if needed
        if (request.UserId.HasValue && request.Status.HasValue)
        {
            orders = orders.Where(o => o.Status == request.Status.Value);
        }

        // Order by CreatedAt descending, then by Id for consistent pagination
        orders = orders.OrderByDescending(o => o.CreatedAt).ThenByDescending(o => o.Id);

        // Apply cursor filter
        if (cursorId.HasValue)
        {
            orders = orders.SkipWhile(o => o.Id != cursorId.Value).Skip(1);
        }

        // Take limit + 1 to check for more
        return orders.Take(request.Limit + 1);
    }

    private async Task<int> GetTotalCountAsync(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId.HasValue && request.Status.HasValue)
        {
            return await _orderRepository.CountAsync(
                o => o.UserId == request.UserId.Value && o.Status == request.Status.Value,
                cancellationToken);
        }

        if (request.UserId.HasValue)
        {
            return await _orderRepository.CountAsync(
                o => o.UserId == request.UserId.Value,
                cancellationToken);
        }

        if (request.Status.HasValue)
        {
            return await _orderRepository.CountAsync(
                o => o.Status == request.Status.Value,
                cancellationToken);
        }

        return await _orderRepository.CountAsync(cancellationToken: cancellationToken);
    }

    private static string EncodeCursor(Guid id)
    {
        var json = JsonSerializer.Serialize(new { id });
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }

    private static Guid? DecodeCursor(string cursor)
    {
        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            var obj = JsonSerializer.Deserialize<JsonElement>(json);
            if (obj.TryGetProperty("id", out var idElement))
            {
                return Guid.Parse(idElement.GetString()!);
            }
        }
        catch
        {
            // Invalid cursor, ignore
        }

        return null;
    }
}