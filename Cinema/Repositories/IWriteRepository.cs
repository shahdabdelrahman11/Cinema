namespace Cinema.Repositories
{
    public interface IWriteRepository<T> where T : class
    {
        void Add(T entity);
        void Update(T entity);
        void Delete(int id);
        void Save();
    }
}