using OnlineStore.Domain.Entities;

namespace OnlineStore.Application.Statistics;

public sealed record CustomerSpending(Customer Customer, decimal TotalSpent);
