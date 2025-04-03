using AutoMapper;
using AppointmentManagementAPI.DTOs;
using AppointmentManagementAPI.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AppointmentManagementAPI.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Appointment, AppointmentDto>().ReverseMap();
        }
    }
}
