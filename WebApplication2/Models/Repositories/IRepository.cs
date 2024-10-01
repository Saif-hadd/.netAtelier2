namespace WebApplication2.Models.Repositories
{
    public interface IRepository<T>
    {
        T Get(int Id);
        IEnumerable<T> GetAll();
        T Add(T t);
        T Update(T t);
        List<T> Search(string term);
        T Delete(int Id);
        string? GetById(int id);
    }
}
