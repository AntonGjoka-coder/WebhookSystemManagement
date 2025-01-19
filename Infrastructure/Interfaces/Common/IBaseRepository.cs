namespace Infrastructure.Interfaces.Common
{
    public interface IBaseRepository<T>
    {
        Task<Guid> Create(T product);
        Task Update(T product);
        Task Delete(Guid Id);
        Task<T> GetById(Guid Id);
        Task<List<T>> GetAll();
    }
}