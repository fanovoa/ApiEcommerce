using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace ApiEcommerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    //[EnableCors(PoliceNames.AllowSpecificOrigin)]
    public class CategoriesController(ICategoryRepository categoryRepository, IMapper mapper) : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository = categoryRepository;
        private readonly IMapper _mapper = mapper;


        private const string ID_NO_VALIDO ="El id debe ser mayor a cero";
        private const string NO_EXISTE_CATEGORIA_CON_ID = "La categoría no existe con el id ";
        private const string REQUEST_NO_VALIDO ="El cuerpo de la petición es inválido";
        private const string CATEGORIA_YA_EXISTE="La categoría ya existe";
        private const string ERROR_AL_ACTUALIZAR="Algo salió mal al actualizar el registro";
        private const string ERROR_AL_ELIMINAR ="Algo salió mal al eliminar el registro";
        private const string ERROR_AL_GUARDAR="Algo salió mal al guarda el registro";

        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        //[EnableCors("AllowSpecificOrigin")]
        public IActionResult GetCategories()
        {
            var categories = _categoryRepository.GetCategories();
            var categoriesDto = new List<CategoryDto>();
            foreach( var category in categories)
            {
                categoriesDto.Add(_mapper.Map<CategoryDto>(category));
            }

            return Ok(categoriesDto);
        }


        [AllowAnonymous]
        [HttpGet("{id:int}", Name = "GetCategory")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetCategory(int id )
        {
            if( id<= 0)
             return BadRequest(ID_NO_VALIDO);

            var category = _categoryRepository.GetCategory(id);

            if ( category == null)
               return NotFound($"{NO_EXISTE_CATEGORIA_CON_ID} {id}");

            var categoryDto = _mapper.Map<CategoryDto>(category);
            return Ok(categoryDto);

        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CreateCategory([FromBody] CreateCategoryDto createCategoryDto)
        {
            if( createCategoryDto == null)
            {
                 ModelState.AddModelError("CustomError", REQUEST_NO_VALIDO);
                return BadRequest(ModelState);
            }

            if (_categoryRepository.CategoryExists(createCategoryDto.Name))
            {
                ModelState.AddModelError("CustomError", CATEGORIA_YA_EXISTE);
                return BadRequest(ModelState);
            }

            var category = _mapper.Map<Category>(createCategoryDto);
            if (!_categoryRepository.CreateCategory(category))
            {
                ModelState.AddModelError("CustomError",$"{ERROR_AL_GUARDAR} {category.Name}");
                return StatusCode(500, ModelState);
            }


            return CreatedAtRoute("GetCategory", new { id= category.Id},category);

        }


        [HttpPatch("{id:int}", Name ="UpdateCategory")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult UpdateCategory(int id, [FromBody] CreateCategoryDto updateCategoryDto)
        {
            if( updateCategoryDto == null)
            {
                 ModelState.AddModelError("CustomError", REQUEST_NO_VALIDO);
                return BadRequest(ModelState);
            }

            if (!_categoryRepository.CategoryExists(id))
            {
                return NotFound($"{NO_EXISTE_CATEGORIA_CON_ID} {id}");
            }
            

            if (_categoryRepository.CategoryExists(updateCategoryDto.Name))
            {
                ModelState.AddModelError("CustomError", CATEGORIA_YA_EXISTE);
                return BadRequest(ModelState);
            }

            var category = _mapper.Map<Category>(updateCategoryDto);
            category.Id= id;
            if (!_categoryRepository.UpdateCategory(category))
            {
                ModelState.AddModelError("CustomError",$"{ERROR_AL_ACTUALIZAR} {category.Name}");
                return StatusCode(500, ModelState);
            }


            return NoContent();

        }


        [HttpDelete("{id:int}", Name ="DeleteCategory")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult DeleteCategory(int id)
        {
            
            if( id<= 0)
             return BadRequest(ID_NO_VALIDO);

            if (!_categoryRepository.CategoryExists(id))
                 return NotFound($"{NO_EXISTE_CATEGORIA_CON_ID} {id}");
            
            var category = _categoryRepository.GetCategory(id);

            if( category == null)
                 return NotFound($"{NO_EXISTE_CATEGORIA_CON_ID} {id}");

            if (!_categoryRepository.DeleteCategory(category))
            {
                 ModelState.AddModelError("CustomError",$"{ERROR_AL_ELIMINAR} {category.Name}");
                return StatusCode(500, ModelState);
            }

             return NoContent();
        }

    }
}
