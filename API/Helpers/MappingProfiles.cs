﻿using API.Dtos;
using AutoMapper;
using Core.Entities;
using Core.Entities.Identity;

namespace API.Helpers
{
    public class MappingProfiles: Profile
    {
        public MappingProfiles()
        {
            CreateMap<Address, AddressDto>().ReverseMap();
            CreateMap<AppUser, UserWithAddressDto>().ReverseMap();          
            CreateMap<Group, GroupDto>().ReverseMap();
        }
    }
}
