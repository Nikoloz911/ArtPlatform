using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SubscriptionService.Data;

namespace SubscriptionService.Controllers;

[Route("api/subscription")]
[ApiController]
public class SubscriptionController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public SubscriptionController(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    //    GET /api/subscriptions/followers/{artistId
    //} - ხელოვანის გამომწერები
    //GET /api/subscriptions/following - ვის გამოვწერთ(მოქმედი მომხმარებელი)
    //POST /api/subscriptions/{artistId} -ხელოვანის გამოწერა
    //DELETE /api/subscriptions/{artistId} -გამოწერის გაუქმება

}
