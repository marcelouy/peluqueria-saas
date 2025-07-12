using AutoMapper;
using PeluqueriaSaaS.Application.DTOs;
using PeluqueriaSaaS.Domain.Entities;

namespace PeluqueriaSaaS.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Cliente, ClienteDto>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email != null ? src.Email.Valor : null))
                .ForMember(dest => dest.Telefono, opt => opt.MapFrom(src => src.Telefono != null ? src.Telefono.NumeroCompleto : null));
        }
    }
}
