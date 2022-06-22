using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using TTalk.Library.Models;
using TTalk.WinUI.Models;

namespace TTalk.WinUI
{
    internal class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ChannelMessage, Message>();
            CreateMap<MessageAttachment, Attachment>();
        }
    }
}
