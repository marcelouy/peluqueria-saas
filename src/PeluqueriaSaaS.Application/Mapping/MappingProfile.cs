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
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email ?? ""))           // ✅ FIXED: Direct string mapping
                .ForMember(dest => dest.Telefono, opt => opt.MapFrom(src => src.Telefono ?? ""))     // ✅ FIXED: Direct string mapping
                .ForMember(dest => dest.FechaRegistro, opt => opt.MapFrom(src => src.FechaCreacion));
        }
    }
}