
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiEcommerce.Data;
using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ApiEcommerce.Repository;

public class UserRepository(ApplicationDbContext dbContext, IConfiguration configuration) : IUserRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly string? _secrectKet = configuration.GetValue<string>("ApiSettings");

    private const string USER_NAME_REQUIRED= "El username es requerido";
    private const string PASSWORD_REQUERED="La constraseña es requerida";
    private const string USER_NAME_NO_FOUND= "Username no encontrado";
    private const string CREDENCIAL_INCORRECT= "Credenciales incorrectas";
    private const string SECRECT_KEY_NOT_CONFIGURED="SecretKey No configurada";
    private const string USER_LOGIN_SUCCESS="Usuario logueado correctamente";


    public User? GetUser(int id) => _dbContext.Users.FirstOrDefault(user => user.Id == id);

    public IReadOnlyCollection<User> GetUsers() => [.. _dbContext.Users.OrderBy(user => user.Name)];

    public bool IsUniqueUser(string username) => !_dbContext.Users.Any(user => user.Username.ToLower().Trim() == username.ToLower().Trim());

    public async Task<UserLoginResponseDto> Login(UserLoginDto userLoginDto)
    {
        if (string.IsNullOrEmpty(userLoginDto.Username))
            return ErrorLogin(USER_NAME_REQUIRED);

        if (string.IsNullOrEmpty(userLoginDto.Password))
            return ErrorLogin(PASSWORD_REQUERED);

        var user = await _dbContext.Users.FirstOrDefaultAsync<User>(user => user.Username.ToLower().Trim() == userLoginDto.Username.ToLower().Trim());

        if (user == null)
            return ErrorLogin(USER_NAME_NO_FOUND);

        if (ThePasswordNotAreIquals(userLoginDto.Password, user.Password!))
            return ErrorLogin(CREDENCIAL_INCORRECT);

        if (string.IsNullOrWhiteSpace(_secrectKet))
            throw new InvalidOperationException(SECRECT_KEY_NOT_CONFIGURED);


        var token = GenerateToken(user.Id.ToString(), user.Username, user.Role?? string.Empty);

        return LoginSuccess(token, user.Username, user.Name!, user.Role, user.Password!,  USER_LOGIN_SUCCESS );
       
    }

    public async Task<User> Register(CreateUserDto createUserDto)
    {
        var encriptedPassword = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);
        var user = new User()
        {
            Username = createUserDto.Username ?? "No Username",
            Name = createUserDto.Name,
            Role = createUserDto.Role,
            Password = encriptedPassword
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        return user;

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
    
    private static bool ThePasswordNotAreIquals(string password1, string password2) => !BCrypt.Net.BCrypt.Verify(password1, password2);

    private static UserLoginResponseDto ErrorLogin(string message)
    {
        return new UserLoginResponseDto()
        {
            Token = "",
            User = null,
            Message = message
        };
    }

     private static UserLoginResponseDto LoginSuccess(string? token,string username, string name,string? role, string password, string message)
    {
        return new UserLoginResponseDto()
        {
            Token = token,
            User = new UserRegisterDto()
            {
                Username = username,
                Name = name,
                Role = role ?? "sin rol",
                Password = password
            },
            Message = message
        };
    }
}
