using OnlineStore.Domain.Entities;
using OnlineStore.Infrastructure.Repositories;

namespace OnlineStore.Infrastructure.Tests.Repositories;

public class InMemoryRepositoryTests
{
    private readonly InMemoryRepository<Product> _repository = new();
    private readonly Product _laptop = new("Laptop", 45000m);
    private readonly Product _mouse = new("Mouse", 800m);

    [Fact]
    public void Add_StoresEntity()
    {
        _repository.Add(_laptop);
        _repository.Add(_mouse);

        Assert.Equal([_laptop, _mouse], _repository.GetAll());
    }

    [Fact]
    public void Add_WithSameEntityTwice_ThrowsInvalidOperationException()
    {
        _repository.Add(_laptop);

        Assert.Throws<InvalidOperationException>(() => _repository.Add(_laptop));
        Assert.Single(_repository.GetAll());
    }

    [Fact]
    public void Remove_WithExistingEntity_RemovesItAndReturnsTrue()
    {
        _repository.Add(_laptop);
        _repository.Add(_mouse);

        var removed = _repository.Remove(_laptop);

        Assert.True(removed);
        Assert.Equal([_mouse], _repository.GetAll());
    }

    [Fact]
    public void Remove_WithMissingEntity_ReturnsFalse()
    {
        _repository.Add(_laptop);

        Assert.False(_repository.Remove(_mouse));
        Assert.Single(_repository.GetAll());
    }

    [Fact]
    public void GetById_ReturnsEntityOrNull()
    {
        _repository.Add(_laptop);

        Assert.Same(_laptop, _repository.GetById(_laptop.Id));
        Assert.Null(_repository.GetById(_mouse.Id));
    }

    [Fact]
    public void GetAll_CannotBeModifiedDirectly()
    {
        _repository.Add(_laptop);

        var items = Assert.IsAssignableFrom<ICollection<Product>>(_repository.GetAll());

        Assert.Throws<NotSupportedException>(() => items.Add(_mouse));
    }
}
