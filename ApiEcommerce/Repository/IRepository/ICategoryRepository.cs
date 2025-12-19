
namespace ApiEcommerce.Repository.IRepository;

public interface ICategoryRepository
{
    IReadOnlyCollection<Category> GetCategories();
    Category? GetCategory(int id);
    bool CategoryExists(int id);
    bool CategoryExists(string name);
    bool CreateCategory(Category category);
    bool UpdateCategory(Category category);
    bool DeleteCategory(Category category);
    bool Save();


}
