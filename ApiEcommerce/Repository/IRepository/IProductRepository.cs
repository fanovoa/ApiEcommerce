
using ApiEcommerce.Models;

namespace ApiEcommerce.Repository.IRepository;

public interface IProductRepository
{
  public IReadOnlyCollection<Product> GetProducts();
  public IReadOnlyCollection<Product> GetProductsForCategory(int categoryId );
  public IReadOnlyCollection<Product> SearchProduct( string name);
  public Product? GetProduct( int id);
  public bool BuyProduct(string name, int quantity);
  public bool ProductExists(int id);
  public bool ProductExists(string name);
  public bool CreateProduct(Product product);
  public bool UpdateProduct(Product product);
  public bool DeleteProduct(Product product);
  public bool Save();
}

