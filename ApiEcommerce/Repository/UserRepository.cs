
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiEcommerce.Data;
using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ApiEcommerce.Repository;

public class UserRepository
    (ApplicationDbContext dbContext, IConfiguration configuration, UserManager<ApplicationUser> userManager,RoleManager<IdentityRole> roleManager)
     : IUserRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly string? _secrectKet = configuration.GetValue<string>("ApiSettings:SecretKey");

    private readonly UserManager<ApplicationUser> _userManager =userManager;
    private readonly RoleManager<IdentityRole> _roleManager =roleManager;

    private const string USER_NAME_REQUIRED= "El username es requerido";
    private const string PASSWORD_REQUERED="La constraseña es requerida";
    private const string USER_NAME_NO_FOUND= "Username no encontrado";
    private const string CREDENCIAL_INCORRECT= "Credenciales incorrectas";
    private const string SECRECT_KEY_NOT_CONFIGURED="SecretKey No configurada";
    private const string USER_LOGIN_SUCCESS="Usuario logueado correctamente";
    private const string REGISTER_NOT_COMPLETED ="No se pudo completar el registro";


    public ApplicationUser? GetUser(string id) => _dbContext.ApplicationUsers.FirstOrDefault(user => user.Id == id);

    public IReadOnlyCollection<ApplicationUser> GetUsers() => [.. _dbContext.ApplicationUsers.OrderBy(user => user.Name)];

    public bool IsUniqueUser(string username) => !_dbContext.Users.Any(user => user.Username.ToLower().Trim() == username.ToLower().Trim());

    public async Task<UserLoginResponseDto> Login(UserLoginDto userLoginDto)
    {
        string? Password  =userLoginDto.Password;
        string? Username  =userLoginDto.Username;

        ValidateSecretKeyConfigurated();

        ValidateUserName(Username);

        ValidatePassword(Password);

        var user = await _dbContext.ApplicationUsers.FirstOrDefaultAsync<ApplicationUser>(user => user.UserName != null && user.UserName.ToLower().Trim() == Username!.ToLower().Trim());

        if (user == null)
            return ErrorLogin(USER_NAME_NO_FOUND);

        if(await ThePasswordNotAreEquals( user, Password!))
            return ErrorLogin(CREDENCIAL_INCORRECT);

        var roles = await _userManager.GetRolesAsync(user);
      
        var token = GenerateToken(user.Id.ToString(), user.UserName ?? string.Empty, roles.FirstOrDefault() ?? string.Empty);

        return LoginSuccess(token, user, USER_LOGIN_SUCCESS);

    }

    public async Task<UserDataDto> Register(CreateUserDto createUserDto)
    {
        ValidateSecretKeyConfigurated();
        ValidateUserName(createUserDto.Username);
        ValidatePassword(createUserDto.Password);
        ApplicationUser user = CreateEntityUser(createUserDto);

        var result = await _userManager.CreateAsync(user, createUserDto.Password!);

        if(!result.Succeeded)
            throw new ApplicationException(REGISTER_NOT_COMPLETED);
     
        await AssigUserRole(user, createUserDto.Role);

        return user.Adapt<UserDataDto>();
        
    }

    private async Task<bool> ThePasswordNotAreEquals(ApplicationUser user, string password) => !await _userManager.CheckPasswordAsync(user, password);

    private async Task AssigUserRole(ApplicationUser user, string? roleName)
    {
         var userRole = roleName ?? "User";
         if(!await _roleManager.RoleExistsAsync(userRole))
         {
            var identityRole = new IdentityRole(userRole);
            await _roleManager.CreateAsync(identityRole);
         }
          await _userManager.AddToRoleAsync(user, userRole);
    }
   
    private void ValidateSecretKeyConfigurated()
    {
        if (string.IsNullOrWhiteSpace(_secrectKet))
            throw new InvalidOperationException(SECRECT_KEY_NOT_CONFIGURED);
    }

    private string? GenerateToken(string id, string username, string role)
    {
        var handlerToken = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_secrectKet!);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
          [
                new Claim("id", id),
                new Claim("username", username),
                new Claim(ClaimTypes.Role, role ),
            ]),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = handlerToken.CreateToken(tokenDescriptor);
        var tokenWrite= handlerToken.WriteToken(token);
        return tokenWrite;
    }
    
    private UserLoginResponseDto LoginSuccess(string? token, ApplicationUser? user, string message)
    {
        return new UserLoginResponseDto()
        {
            Token = token,
            User = user.Adapt<UserDataDto>(),
            Message = message
        };
    }

    private static UserLoginResponseDto ErrorLogin(string message)
    {
        return new UserLoginResponseDto()
        {
            Token = "",
            User = null,
            Message = message
        };
    }

    private static ApplicationUser CreateEntityUser(CreateUserDto createUserDto)
    {
        return new ApplicationUser()
        {
            UserName = createUserDto.Username,
            Email = createUserDto.Username,
            NormalizedEmail = createUserDto.Username!.ToUpper(),
            Name = createUserDto.Name
        };
    }

    private static void ValidatePassword(string? password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException(PASSWORD_REQUERED);
    }

    private static void ValidateUserName(string? userName)
    {
        if (string.IsNullOrEmpty(userName))
            throw new ArgumentException(USER_NAME_REQUIRED);
    }

}
