using AutoMapper;
using TechsysLog.Application.Common;
using TechsysLog.Application.DTOs;
using TechsysLog.Domain.Interfaces;

namespace TechsysLog.Application.Queries.Deliveries;

/// <summary>
/// Handler for GetDeliveryByOrderIdQuery.
/// </summary>
public sealed class GetDeliveryByOrderIdQueryHandler : IQueryHandler<GetDeliveryByOrderIdQuery, DeliveryDto?>
{
    private readonly IDeliveryRepository _deliveryRepository;
    private readonly IMapper _mapper;

    public GetDeliveryByOrderIdQueryHandler(
        IDeliveryRepository deliveryRepository,
        IMapper mapper)
    {
        _deliveryRepository = deliveryRepository;
        _mapper = mapper;
    }

    public async Task<DeliveryDto?> Handle(GetDeliveryByOrderIdQuery request, CancellationToken cancellationToken)
    {
        var delivery = await _deliveryRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);

        if (delivery is null)
            return null;

        return _mapper.Map<DeliveryDto>(delivery);
    }
}