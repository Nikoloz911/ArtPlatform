using AutoMapper;
using SubscriptionService.DTOs;
using SubscriptionService.Models;

namespace SubscriptionService.Helpers;
public class SubscriptionMappingProfile : Profile
{
    public SubscriptionMappingProfile()
    {
        CreateMap<Subscription, SubscriptionDTO>();
        CreateMap<Subscription, FollowerDTO>();
    }
}
