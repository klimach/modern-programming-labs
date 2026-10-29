# Modern Programming Labs

- **ПІБ:** Клімач Олександр Павлович
- **Група:** 301-пТК
- **Варіант:** 1
- **Предметна область:** Інтернет-магазин (OnlineStore)
- **Версія .NET:** 10

## Опис предметної області

Проєкт моделює інтернет-магазин електроніки та аксесуарів «TechNest». Покупець (`Customer`) обирає товари (`Product`) з каталогу, кожен з яких має назву та ціну, і формує замовлення (`Order`), що складається з позицій (`OrderItem`) з кількістю та зафіксованою на момент додавання ціною. Поки замовлення має статус «Створено», у ньому можна додавати й видаляти товари та змінювати їх кількість. Замовлення оплачується карткою або банківським переказом і переходить у статус «Оплачено», після чого відправляється на адресу доставки покупця та отримується ним. Неоплачене замовлення можна скасувати. Оплачене чи скасоване замовлення не можна змінювати або оплачувати повторно.

## Структура проєкту

```
modern-programming-labs/
├── src/
│   ├── OnlineStore.Domain/           # Сутності, правила та доменні винятки; не залежить від інших проєктів
│   ├── OnlineStore.Application/      # Сценарії використання, статистика та абстракції (IPaymentMethod, IRepository<T>) → Domain
│   ├── OnlineStore.Infrastructure/   # Реалізації абстракцій (CardPayment, BankTransferPayment, InMemoryRepository<T>) → Application
│   └── OnlineStore.App/              # Console Application, composition root, тестові дані → усі проєкти
├── tests/
│   ├── OnlineStore.Domain.Tests/          # Unit-тести доменних правил (xUnit) → Domain
│   ├── OnlineStore.Application.Tests/     # Тести LINQ-статистики → Infrastructure
│   └── OnlineStore.Infrastructure.Tests/  # Тести репозиторію → Infrastructure
├── README.md
├── .gitignore
└── ModernProgramming.slnx
```

## Запуск

```bash
dotnet run --project src/OnlineStore.App
dotnet test
```

## Lab 2 — ООП та предметна модель

### Модель

| Клас | Стан | Поведінка |
|---|---|---|
| `Entity` | `Id` | рівність за `Id` |
| `Product` | `Name`, `Price` | `ChangePrice()` |
| `Customer` | `Name`, `Email` | — |
| `Order` | `Customer`, `Status`, `Items`, `TotalAmount`, `Payment` | `AddItem()`, `RemoveItem()`, `ChangeItemQuantity()`, `Pay()`, `Cancel()` |
| `OrderItem` | `Product`, `Quantity`, `UnitPrice`, `Total` | змінюється лише через `Order` |
| `PaymentReceipt` | `TransactionId`, `PaymentMethod`, `Amount` | — |

Статуси замовлення: `Created` → `Paid` або `Created` → `Cancelled`.

### Принципи ООП у коді

| Принцип | Де реалізовано |
|---|---|
| Encapsulation | `Order` зберігає позиції в `private readonly List<OrderItem> _items`, а назовні віддає `IReadOnlyCollection<OrderItem> Items`; `Status`, `Price`, `Quantity` мають `private set`; конструктор `OrderItem` — `internal` |
| Abstraction | `IPaymentMethod` описує контракт оплати без прив'язки до конкретного способу |
| Polymorphism | `OrderPaymentService` викликає `paymentMethod.Pay()` через інтерфейс; у `Program.cs` одна змінна `IPaymentMethod` отримує `CardPayment` та `BankTransferPayment` |
| Inheritance | `Product`, `Customer`, `Order` наслідують абстрактний клас `Entity` з `protected` конструкторами |
| Composition | `Order` містить `OrderItem`, а `OrderItem` посилається на `Product` |

### Правила

- ціна товару > 0;
- quantity > 0;
- оплачене замовлення не можна оплатити повторно або скасувати;
- скасоване замовлення не можна оплатити;
- порожнє замовлення не можна оплатити;
- сума оплати повинна дорівнювати сумі замовлення;
- оплачене чи скасоване замовлення не можна змінювати;
- список `Items` не можна змінювати напряму.

Порушення правил призводить до `DomainException`; кожне правило покрито тестами в `OnlineStore.Domain.Tests`.

## Lab 3 — Сучасне моделювання даних у C#

### Class vs record

| Тип | Вид | Чому |
|---|---|---|
| `Product`, `Customer`, `Order` | `class` (entity) | мають identity (`Id`) та змінюють стан |
| `Address` | `record` (value object) | `City`, `Street`, `Building`, `PostalCode`; рівність за значеннями, зміна лише через `with` |
| `PaymentReceipt` | `record` (value object) | незмінний результат оплати |

Record-и валідують значення в ініціалізаторах властивостей і кидають `DomainException` (поштовий індекс — 5 цифр).

### Можливості C#

| Можливість | Де використано |
|---|---|
| `required` + `init` | `Customer.Name`, `Customer.Email` — обов'язкові під час створення, валідація в `init`-аксесорі (ключове слово `field`) |
| Nullable reference types | `Customer.Address` (`Address?`), `Customer.PhoneNumber` (`string?`), `Order.Payment` (`PaymentReceipt?`); `Order.Ship()` перевіряє `Customer.Address is null` |
| `enum` | `OrderStatus`: `Created`, `Paid`, `Shipped`, `Delivered`, `Cancelled`; `OrderCategory`: `Small`, `Medium`, `Large` |
| Switch expression | `Order.GetStatusDescription()`, `Order.GetOrderCategory()`, `Order.Cancel()`, `Order.EnsureCanBePaid()` |
| Relational pattern | `GetOrderCategory()`: `< 500` → Small, `< 5000` → Medium, інакше Large |
| Property pattern | `Order.HasFreeDelivery`: `this is { Status: OrderStatus.Paid, TotalAmount: >= 2000 }`; `Address.PostalCode is { Length: 5 }` |
| Logical pattern | `EnsureCanBePaid()`: `Paid or Shipped or Delivered` |
| List pattern | `Customer.PhoneNumber`: `['+', .. var digits]` |

### Життєвий цикл замовлення

```
Created ──Pay()──▶ Paid ──Ship()──▶ Shipped ──Deliver()──▶ Delivered
   │
   └──Cancel()──▶ Cancelled
```

- відправити можна лише оплачене замовлення, якщо в покупця вказана адреса;
- доставити можна лише відправлене замовлення;
- скасувати можна лише замовлення зі статусом `Created`.

## Lab 4 — Collections, Generics та LINQ

### Generic repository

`IRepository<T>` (Application) та `InMemoryRepository<T>` (Infrastructure) з constraint `where T : Entity`, що дає доступ до `Id` та рівності сутностей:

| Метод | Опис |
|---|---|
| `Add(T)` | додає сутність; повторне додавання тієї ж сутності — `InvalidOperationException` |
| `Remove(T)` | видаляє сутність, повертає `false`, якщо її немає |
| `GetById(Guid)` | повертає сутність або `null` |
| `GetAll()` | `IReadOnlyCollection<T>` — колекцію не можна змінити ззовні |

### Тестові дані

`SampleData.Seed()` у `OnlineStore.App/Data` заповнює репозиторії: 6 покупців, 10 товарів, 20 замовлень у всіх статусах (`Created`, `Paid`, `Shipped`, `Delivered`, `Cancelled`).

### LINQ-запити (`OrderStatisticsService`)

Продажі (сума, середнє, витрати покупців, топ товарів) рахуються лише за оплаченими замовленнями (`Payment is not null`); створені та скасовані замовлення виручки не дають.

| Завдання | Метод | LINQ |
|---|---|---|
| Orders з Total > 1000 | `GetOrdersWithTotalAbove(1000)` | `Where` |
| Топ-5 найдорожчих orders | `GetMostExpensiveOrders(5)` | `OrderByDescending`, `Take` |
| Загальна сума продажів | `GetTotalSales()` | `Where`, `Sum` |
| Середня вартість order | `GetAverageOrderValue()` | `Select`, `DefaultIfEmpty`, `Average` |
| Кількість orders кожного customer | `GetOrderCountByCustomer()` | `GroupBy`, `Select`, `Count`, `ThenBy` |
| Усі products з усіх orders | `GetOrderedProducts()` | `SelectMany`, `Select`, `Distinct`, `OrderBy` |
| Топ-3 products за quantity | `GetBestSellingProducts(3)` | `SelectMany`, `GroupBy`, `Sum`, `Take` |
| Customers, що витратили > 5000 | `GetCustomersWithSpendingAbove(5000)` | `GroupBy`, `Sum`, `Where` |
| Чи існує order > 10000 | `HasOrderWithTotalAbove(10000)` | `Any` |
| `Dictionary<CustomerId, decimal>` | `GetSpendingByCustomerId()` | `GroupBy`, `ToDictionary` |

Результати групувань повертаються як records: `CustomerOrderCount`, `ProductSales`, `CustomerSpending`. Методи матеріалізують результат через `ToList()`, тому запит виконується один раз під час виклику, а не при кожному переборі (deferred execution).
