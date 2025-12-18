using ApiEcommerce.Data;
using ApiEcommerce.Repository.IRepository;

namespace ApiEcommerce.Repository;

public class CateegoryRepository(ApplicationDbContext dbContext) : ICategoryRepository
{

    private readonly ApplicationDbContext _dbContext = dbContext;

    public bool CategoryExists(int id) =>  _dbContext.Categories.Any(category => category.Id == id);
    

    public bool CategoryExists(string name) =>  _dbContext.Categories.Any(category => category.Name.ToLower().Trim() == name.ToLower().Trim());
    
    public bool CreateCategory(Category category)
    {
       category.CreationDate = DateTime.Now;
       _dbContext.Categories.Add(category);
       return Save();
    }

    public bool DeleteCategory(Category category)
    {
        _dbContext.Categories.Remove(category);
        return Save();
    }

    public IReadOnlyCollection<Category> GetCategories() => [.. _dbContext.Categories.OrderBy( category => category.Name)];
    

    public Category GetCategory(int id)
    {
        return _dbContext.Categories.FirstOrDefault(category => category.Id == id) ??
        throw new InvalidOperationException($"La categoría con el id {id} no existe");
    }

    public bool Save() => _dbContext.SaveChanges() >= 0;
    

    public bool UpdateCategory(Category category)
    {
        category.CreationDate = DateTime.Now;
        _dbContext.Categories.Update(category);
        return Save();
    }
}
