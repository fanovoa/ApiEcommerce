using System;
using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;

namespace ApiEcommerce.Repository.IRepository;

public interface IUserRepository
{
    public IReadOnlyCollection<ApplicationUser> GetUsers();
    public ApplicationUser? GetUser(string id);
    public bool IsUniqueUser(string username);
    public Task<UserLoginResponseDto> Login( UserLoginDto userLoginDto);
    public Task<UserDataDto> Register(CreateUserDto createUserDto);
}
