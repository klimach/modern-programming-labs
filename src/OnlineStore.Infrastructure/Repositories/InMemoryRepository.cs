using OnlineStore.Application.Repositories;
using OnlineStore.Domain.Common;

namespace OnlineStore.Infrastructure.Repositories;

public sealed class InMemoryRepository<T> : IRepository<T>
    where T : Entity
{
    private readonly List<T> _items = [];

    public void Add(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (_items.Contains(entity))
        {
            throw new InvalidOperationException($"{typeof(T).Name} with id {entity.Id} already exists.");
        }

        _items.Add(entity);
    }

    public bool Remove(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return _items.Remove(entity);
    }

    public T? GetById(Guid id) => _items.FirstOrDefault(item => item.Id == id);

    public IReadOnlyCollection<T> GetAll() => _items.AsReadOnly();
}
