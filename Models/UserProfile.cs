using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace UserManagement.Models
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // نگاشت IdentityUser به UserDto
            CreateMap<IdentityUser, UserDto>();
        }
    }
}