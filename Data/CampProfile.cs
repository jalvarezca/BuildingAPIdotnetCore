using AutoMapper;
using CoreCodeCamp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreCodeCamp.Data
{
    public class CampProfile : Profile
    {
        public CampProfile()
        {
            this.CreateMap<Camp, CampModel>()
                .ForMember(c => c.Venue, o => o.MapFrom(m => m.Location.VenueName))
                .ReverseMap();     //This will create an exception to the automapper rules. Indicates where to get the infomration for Venue

            this.CreateMap<Talk, TalkModel>()
                .ReverseMap()
                .ForMember(t => t.Camp, opt => opt.Ignore())            //When assigning from talk model to Talk entity, ignore Camp
                .ForMember(t => t.Speaker, opt => opt.Ignore());        //When assigning from talk model to Talk entity, ignore Speaker

            this.CreateMap<Speaker, SpeakerModel>()
                .ReverseMap();

        }
    }
}
