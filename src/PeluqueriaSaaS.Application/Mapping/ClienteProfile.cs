using AutoMapper;
using PeluqueriaSaaS.Application.DTOs;
using PeluqueriaSaaS.Domain.Entities;

namespace PeluqueriaSaaS.Application.Mapping;

public class ClienteProfile : Profile
{
    public ClienteProfile()
    {
        CreateMap<Cliente, ClienteDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Valor))
            .ForMember(dest => dest.Telefono, opt => opt.MapFrom(src => src.Telefono.Numero))
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => src.Activo))
            .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => src.FechaCreacion));
    }
}
