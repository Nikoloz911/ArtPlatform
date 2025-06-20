using AuthService.Data;
using AuthService.DTOs;
using AuthService.Models;
using AutoMapper;
using CommonUtils.JWT;
using CommonUtils.SMTP;
using Contracts.Core;
using Contracts.DTO;
using Contracts.Enums;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
namespace AuthService.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private readonly IValidator<RegisterUserDTO> _validator;

    public AuthController(DataContext context, IMapper mapper, IValidator<RegisterUserDTO> validator)
    {
        _context = context;
        _mapper = mapper;
        _validator = validator;
    }

    /// REGISTER A NEW USER
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDTO registerUserDto)
    {
        /// VALIDATE THE REQUEST
        ValidationResult validationResult = await _validator.ValidateAsync(registerUserDto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new ApiResponse<List<string>>
            {
                StatusCode = 400,
                Message = "Validation Failed",
                Data = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
            });
        }
        if (!Enum.TryParse<USER_ROLES>(registerUserDto.Role, true, out var parsedRole))
        {
            return BadRequest(new ApiResponse<string>
            {
                StatusCode = 400,
                Message = "Invalid role. Must be 'Artist', 'Critic', or 'Admin'.",
                Data = null
            });
        }

        bool emailExists = await _context.Users.AnyAsync(u => u.Email == registerUserDto.Email);
        if (emailExists)
        {
            return Conflict(new ApiResponse<string>
            {
                StatusCode = 409,
                Message = "Email already Exists",
                Data = null
            });
        }

        // Generate verification code
        string verificationCode = SMTP_Registration.GenerateVerificationCode();

        // Map DTO to User entity
        var user = _mapper.Map<User>(registerUserDto);
        user.Role = parsedRole;
        user.IsVerified = false;
        user.VerificationCode = verificationCode;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Send verification email
        try
        {
            SMTP_Registration.EmailSender(user.Email, user.Name, verificationCode);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse<string>
            {
                StatusCode = 500,
                Message = "User created but failed to send verification email.",
                Data = ex.Message
            });
        }

        // Send event to RabbitMQ
        var userCreatedEvent = _mapper.Map<UserCreatedEvent>(user);

        var factory = new ConnectionFactory { HostName = "localhost" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: "user_created",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(userCreatedEvent));

        channel.BasicPublish(
            exchange: "",
            routingKey: "user_created",
            basicProperties: null,
            body: body
        );

        return Ok(new ApiResponse<User>
        {
            StatusCode = 200,
            Message = "User registered, verification email sent, and event published.",
            Data = user
        });
    }
    /// LOG IN
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLogInDTO loginDto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

        if (user == null || user.Password != loginDto.Password)
        {
            return Unauthorized(new ApiResponse<string>
            {
                StatusCode = 401,
                Message = "Invalid email or password.",
                Data = null
            });
        }

        if (!user.IsVerified)
        {
            return StatusCode(403, new ApiResponse<string>
            {
                StatusCode = 403,
                Message = "User is not verified.",
                Data = null
            });
        }


        var jwtUser = new JWTUserModel
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Password = user.Password,
            Biography = user.Biography,
            ProfilePictureUrl = user.ProfilePictureUrl,
            Role = user.Role,
            IsVerified = user.IsVerified,
            VerificationCode = user.VerificationCode
        };

        var jwtService = HttpContext.RequestServices.GetRequiredService<IJWTService>();
        var token = jwtService.GetUserToken(jwtUser);

        var responseDto = new LoginResponseDTO
        {
            Token = token.Token,
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Biography = user.Biography,
            ProfilePictureUrl = user.ProfilePictureUrl,
            Role = user.Role.ToString(),
            IsVerified = user.IsVerified
        };

        return Ok(new ApiResponse<LoginResponseDTO>
        {
            StatusCode = 200,
            Message = "Login successful.",
            Data = responseDto
        });
    }

    /// VERIFY EMAIL
    [HttpPost("verify")]
    public async Task<IActionResult> VerifyUser([FromBody] VerifyUserDTO verifyDto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == verifyDto.Email);

        if (user == null)
        {
            return NotFound(new ApiResponse<string>
            {
                StatusCode = 404,
                Message = "User not found.",
                Data = null
            });
        }

        if (user.IsVerified)
        {
            return BadRequest(new ApiResponse<string>
            {
                StatusCode = 400,
                Message = "User is already verified.",
                Data = null
            });
        }

        if (user.VerificationCode != verifyDto.VerificationCode)
        {
            return BadRequest(new ApiResponse<string>
            {
                StatusCode = 400,
                Message = "Invalid verification code.",
                Data = null
            });
        }

        user.IsVerified = true;
        user.VerificationCode = null;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<string>
        {
            StatusCode = 200,
            Message = "User successfully verified.",
            Data = null
        });
    }




}