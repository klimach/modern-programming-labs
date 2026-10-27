using OnlineStore.Domain.Entities;

namespace OnlineStore.Application.Statistics;

public sealed record CustomerOrderCount(Customer Customer, int OrderCount);
