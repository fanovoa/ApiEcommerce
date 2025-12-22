
using System.Net.Mail;
using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace ApiEcommerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController(IProductRepository productRepository, ICategoryRepository categoryRepository, IMapper mapper) : ControllerBase
    {
        private readonly IProductRepository _productRepository = productRepository;
        private readonly ICategoryRepository _categoryRepository = categoryRepository;
        private readonly IMapper _mapper = mapper;


        private const string  ID_MAYOR_A_CERO = "El id debe ser mayor a cero";
        private const string  REQUEST_INVALIDO ="El cuerpo de la petición es inválido";
        private const string CATEGORIA_NO_EXISTE = "La categoría no existe con el id";
        private const string PRODUCTO_CON_ID_NO_EXISTE="El producto no existe con el id";
        private const string PRODUCTO_CON_NOMBRE_NO_EXISTE="El producto no existe con el nombre";
        private const string PRODUCTO_CON_NOMBRE_YA_EXISTE="El producto ya existe con el nombre";
        private const string NOMBRE_ES_OBLIGATORIO ="El nombre del producto es obligatorio";
        private const string CANTIDAD_MENOR_A_CERO ="La cantidad debe ser mayor a cero";
        private const string PRODUCTO_NO_EXISTE ="El producto no existe";
        private const string NO_EXISTEN_PRODUCTOS_PARA_CATEGORIA="No existen productos para esa la categoría";
        private const string NO_EXISTEN_PRODUCTOS_CON_NOMBRE_DECRIPCION="No existen productos con el nombre o descripción";
        private const string ERROR_GUARDAR_REGISTRO="Algo salió mal al guarda el registro";
        private const string ERROR_ACTUALIZAR_REGISTRO ="Algo salió mal al actualizar el registro";
        private const string ERROR_ELIMINAR_REGISTRO ="Algo salió mal al eliminar el registro";
        private const string ERROR_AL_COMPRAR="No se pudo comprar el producto o la cantidad solicitada es mayor al stock disponible";


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProducts()
        {
            var products = _productRepository.GetProducts();
            var productsDto = _mapper.Map<List<ProductDto>>(products);
            return Ok(productsDto);
        }

        [HttpGet("{producId:int}", Name = "GetProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProduct(int producId)
        {
            if (producId <= 0)
                return BadRequest(ID_MAYOR_A_CERO);

            var product = _productRepository.GetProduct(producId);
            if (product == null)
                return NotFound($"{PRODUCTO_CON_ID_NO_EXISTE} {producId}");

            return Ok(_mapper.Map<List<ProductDto>>(product));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            if (createProductDto == null)
            {
                ModelState.AddModelError("CustomError", REQUEST_INVALIDO);
                return BadRequest(ModelState);
            }

            string nameProduct = createProductDto.Name;
            int idCategory = createProductDto.CategoryId;

            if (_productRepository.ProductExists(nameProduct))
            {
                ModelState.AddModelError("CustomError", $"{PRODUCTO_CON_NOMBRE_YA_EXISTE} '{nameProduct}'");
                return BadRequest(ModelState);
            }

            if (!_categoryRepository.CategoryExists(idCategory))
            {
                ModelState.AddModelError("CustomError", $"{CATEGORIA_NO_EXISTE} '{idCategory}'");
                return BadRequest(ModelState);
            }

            var product = _mapper.Map<Product>(createProductDto);
            if (!_productRepository.CreateProduct(product))
            {
                ModelState.AddModelError("CustomError", $"{ERROR_GUARDAR_REGISTRO} '{nameProduct}'");
                return StatusCode(500, ModelState);
            }

            var createdProduct = _productRepository.GetProduct(product.ProductId);
            var productDto = _mapper.Map<ProductDto>(createdProduct);
            return CreatedAtRoute("GetProduct", new { producId = product.ProductId }, productDto);
        }


        [HttpGet("searchProductByCategory/{categoryId:int}", Name = "GetProductsForCategory")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProductsForCategory(int categoryId)
        {
            if (categoryId <= 0)
                return BadRequest(ID_MAYOR_A_CERO);

            if (!_categoryRepository.CategoryExists(categoryId))
            {
                ModelState.AddModelError("CustomError", $"{CATEGORIA_NO_EXISTE} '{categoryId}'");
                return BadRequest(ModelState);
            }

            var products = _productRepository.GetProductsForCategory(categoryId);


            if (products.Count == 0)
                return NotFound($"{NO_EXISTEN_PRODUCTOS_PARA_CATEGORIA} '{categoryId}'");

            return Ok(_mapper.Map<List<ProductDto>>(products));
        }


        [HttpGet("searchProductByNameDescription/{searchTerm}", Name = "SearchProducts")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult SearchProducts(string searchTerm)
        {

            var products = _productRepository.SearchProducts(searchTerm);

            if (products.Count == 0)
                return NotFound($"{NO_EXISTEN_PRODUCTOS_CON_NOMBRE_DECRIPCION} '{searchTerm}'");

            return Ok(_mapper.Map<List<ProductDto>>(products));
        }


        [HttpPatch("buyProduct/{name}/{quantity:int}", Name = "BuyProduct")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult BuyProduct(string name, int quantity)
        {
            if (string.IsNullOrEmpty(name))
                return BadRequest(NOMBRE_ES_OBLIGATORIO);

            if (quantity <= 0)
                return BadRequest(CANTIDAD_MENOR_A_CERO);

            var foundProduct = _productRepository.ProductExists(name);

            if (!foundProduct)
                return NotFound($"{PRODUCTO_CON_NOMBRE_NO_EXISTE} {name}");

            if (!_productRepository.BuyProduct(name, quantity))
            {
                ModelState.AddModelError("CustomError", $"{ERROR_AL_COMPRAR} '{name}'");
                return BadRequest(ModelState);
            }

           

            return Ok(CustomMessageBuy(name, quantity));


        }

        [HttpPut("{productId:int}", Name = "UpdateProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult UpdateProduct(int productId, [FromBody] UpdateProductDto updateProductDto)
        {
            if (updateProductDto == null)
            {
                ModelState.AddModelError("CustomError", REQUEST_INVALIDO);
                return BadRequest(ModelState);
            }

            if (!_productRepository.ProductExists(productId))
            {
                ModelState.AddModelError("CustomError", PRODUCTO_NO_EXISTE);
                return BadRequest(ModelState);
            }

            int idCategory = updateProductDto.CategoryId;
            string nameProduct = updateProductDto.Name;

            if (!_categoryRepository.CategoryExists(idCategory))
            {
                ModelState.AddModelError("CustomError", $"{CATEGORIA_NO_EXISTE} {idCategory}");
                return BadRequest(ModelState);
            }



            var product = _mapper.Map<Product>(updateProductDto);
            product.ProductId = productId;

            if (!_productRepository.UpdateProduct(product))
            {
                ModelState.AddModelError("CustomError", $"{ERROR_ACTUALIZAR_REGISTRO} '{nameProduct}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }



        [HttpDelete("{producId:int}", Name = "DeleteProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult DeleteProduct(int producId)
        {
            if (producId <= 0)
                return BadRequest(ID_MAYOR_A_CERO);

            var product = _productRepository.GetProduct(producId);

            if (product == null)
                return NotFound($"{PRODUCTO_CON_ID_NO_EXISTE} {producId}");

            if (!_productRepository.DeleteProduct(product))
            {
                ModelState.AddModelError("CustomError", $"{ERROR_ELIMINAR_REGISTRO} {product.Name}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }


        private static string CustomMessageBuy(string name, int quantity)
        {
            
             var units = quantity == 1 ? "unidad" : "unidades";
             return $"Se compro la {quantity} {units} del producto '{name}'";
        } 


    }
}
