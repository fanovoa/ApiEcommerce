using System;

namespace ApiEcommerce.Models.Dtos;

public interface IProductDto
{
    public string Name { get; set; } 
    public string Description { get; set; } 
    public decimal Price { get; set; }
    public string? ImgUrl { get; set; }
    public IFormFile? Image { get; set; }
    public string SKU { get; set; }  
    public int Stock { get; set; }
    public DateTime? UpdateDate { get; set; }
    public int CategoryId { get; set; }
}
