using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiEcommerce.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(IUserRepository userRepository, IMapper mapper) : ControllerBase
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IMapper _mapper = mapper;

        private const string ID_MAYOR_A_CERO = "El id debe ser mayor a cero";
        private const string USUARIO_CON_ID_NO_EXISTE = "El usuario no existe con el id";
        private const string USERNAME_REQUIRED = "El username es requerido";
        private const string USUARIO_EXIST ="El usuario ya existe";
        private const string ERROR_REGISTER_USER ="Error al registar usuario";



        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetUsers()
        {
            var users = _userRepository.GetUsers();
            return Ok(_mapper.Map<List<UserDto>>(users));
        }

        [HttpGet("{userId:int}", Name = "GetUser")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetUser(int userId)
        {
            if (userId <= 0)
                return BadRequest(ID_MAYOR_A_CERO);

            var user = _userRepository.GetUser(userId);
            if (user == null)
                return NotFound($"{USUARIO_CON_ID_NO_EXISTE} {userId}");

            return Ok(_mapper.Map<UserDto>(user));
        }

        [AllowAnonymous]
        [HttpPost(Name ="RegisterUser")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegisterUser([FromBody] CreateUserDto createUserDto)
        {
            if( createUserDto == null || !ModelState.IsValid)
                return BadRequest(ModelState);
            
            if(string.IsNullOrWhiteSpace(createUserDto.Username))
                return BadRequest(USERNAME_REQUIRED);

            if(!_userRepository.IsUniqueUser(createUserDto.Username))
                return BadRequest(USUARIO_EXIST);

            var result = await _userRepository.Register(createUserDto);

            if( result == null)
                return StatusCode(StatusCodes.Status500InternalServerError,ERROR_REGISTER_USER);

            return CreatedAtRoute("GetUser", new{userId = result.Id}, result);
            
        }

        [AllowAnonymous]
        [HttpPost("Login",Name ="LoginUser")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegisterUser([FromBody] UserLoginDto userLoginDto)
        {
            if( userLoginDto == null || !ModelState.IsValid)
                return BadRequest(ModelState);
            
            var user = await _userRepository.Login(userLoginDto);

            if( user == null)
                return Unauthorized();

            return Ok(user);
            
        }

    }
}
