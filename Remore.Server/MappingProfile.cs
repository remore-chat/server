using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Remore.Server.Models.Dto;
using Remore.Server.Services;

namespace Remore.Server
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
