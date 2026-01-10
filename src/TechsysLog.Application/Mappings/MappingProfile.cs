using AutoMapper;
using TechsysLog.Application.DTOs;
using TechsysLog.Domain.Entities;
using TechsysLog.Domain.ValueObjects;

namespace TechsysLog.Application.Mappings;

/// <summary>
/// AutoMapper profile for mapping domain entities to DTOs.
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value));

        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.OrderNumber, opt => opt.MapFrom(src => src.OrderNumber.Value))
            .ForMember(dest => dest.DeliveryAddress, opt => opt.MapFrom(src => src.DeliveryAddress));

        CreateMap<Address, AddressDto>()
            .ForMember(dest => dest.Cep, opt => opt.MapFrom(src => src.Cep.Value))
            .ForMember(dest => dest.CepFormatted, opt => opt.MapFrom(src => src.Cep.Formatted))
            .ForMember(dest => dest.FullAddress, opt => opt.MapFrom(src => src.ToString()));

        CreateMap<Delivery, DeliveryDto>();

        CreateMap<Notification, NotificationDto>();
    }
}