using OnlineStore.Domain.Common;

namespace OnlineStore.Application.Repositories;

public interface IRepository<T>
    where T : Entity
{
    void Add(T entity);

    bool Remove(T entity);

    T? GetById(Guid id);

    IReadOnlyCollection<T> GetAll();
}
