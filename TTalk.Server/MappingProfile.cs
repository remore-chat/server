using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTalk.Server.Models.Dto;
using TTalk.Server.Services;

namespace TTalk.Server
{
    internal class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ServerConfigurationUpdateDto, ServerConfiguration>();
            CreateMap<ServerConfiguration, ServerConfigurationUpdateDto>();
        }
    }
}
