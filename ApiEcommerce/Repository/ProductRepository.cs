
using System.Collections.Immutable;
using ApiEcommerce.Data;
using ApiEcommerce.Models;
using ApiEcommerce.Repository.IRepository;

namespace ApiEcommerce.Repository;

public class ProductRepository(ApplicationDbContext dbContext) : IProductRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public bool BuyProduct(string name, int quantity)
    {
        if (string.IsNullOrWhiteSpace(name) || quantity < 0) return false;

        var product = _dbContext.Products.FirstOrDefault(product => product.Name.ToLower().Trim() == name.ToLower().Trim());

        if (IsNull(product) || IsLessThan(quantity, product!.Stock)) return false;

        product.Stock -= quantity;
        UpdateProductDto(product);
        return Save();
    }

    public bool CreateProduct(Product product)
    {
        if (IsNull(product)) return false;

        product.CreationDate = DateTime.Now;
        product.UpdateDate = DateTime.Now;
        _dbContext.Products.Add(product);
        return Save();
    }

    public bool DeleteProduct(Product product)
    {
        if (IsNull(product)) return false;
        _dbContext.Products.Remove(product);
        return Save();
    }

    public Product? GetProduct(int id)
    {
        if (id <= 0) return null;

        return _dbContext.Products.FirstOrDefault(product => product.ProductId == id);
    }

    public IReadOnlyCollection<Product> GetProductsForCategory(int categoryId)
    {
        if (categoryId <= 0) return [];

        return [.. _dbContext.Products.Where(product => product.CategoryId == categoryId).OrderBy(product => product.Name)];
    }

    public bool ProductExists(int id)
    {
        if (id <= 0) return false;

        return _dbContext.Products.Any(product => product.ProductId == id);
    }

    public bool ProductExists(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;

        return _dbContext.Products.Any(product => product.Name.ToLower().Trim() == name.ToLower().Trim());
    }

    public IReadOnlyCollection<Product> SearchProduct(string name)
    {
        IQueryable<Product> query = _dbContext.Products;

        if (!string.IsNullOrEmpty(name))
            query = query.Where(product => product.Name.ToLower().Trim() == name.ToLower().Trim());


        return [.. query.OrderBy(product => product.Name)];
    }

    public bool UpdateProduct(Product product)
    {
        if (IsNull(product)) return false;

        product.UpdateDate = DateTime.Now;
        UpdateProductDto(product);
        return Save();

    }

    public IReadOnlyCollection<Product> GetProducts() => [.. _dbContext.Products.OrderBy(product => product.Name)];

    public bool Save() => _dbContext.SaveChanges() >= 0;

    private void UpdateProductDto(Product product) => _dbContext.Products.Update(product);

    private static bool IsLessThan(int quantity1, int quantity2) => quantity2 < quantity1;

    private static bool IsNull(Product? product) => product == null;

}
