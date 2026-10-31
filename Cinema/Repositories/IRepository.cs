namespace Cinema.Repositories
{
    public interface IRepository<T> : IReadOnlyRepository<T>, IWriteRepository<T>
        where T : class
    {
    }
}