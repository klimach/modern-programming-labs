# Modern Programming Labs

- **ПІБ:** Клімач Олександр Павлович
- **Група:** 301-пТК
- **Варіант:** 1
- **Предметна область:** Інтернет-магазин (OnlineStore)
- **Версія .NET:** 10

## Опис предметної області

Проєкт моделює інтернет-магазин електроніки та аксесуарів «TechNest». Покупець (`Customer`) обирає товари (`Product`) з каталогу, кожен з яких має назву та ціну, і формує замовлення (`Order`), що складається з позицій (`OrderItem`) з кількістю та зафіксованою на момент додавання ціною. Поки замовлення має статус «Створено», у ньому можна додавати й видаляти товари та змінювати їх кількість. Замовлення оплачується карткою або банківським переказом і переходить у статус «Оплачено», а неоплачене замовлення можна скасувати. Оплачене чи скасоване замовлення не можна змінювати або оплачувати повторно.

## Структура проєкту

```
modern-programming-labs/
├── src/
│   ├── OnlineStore.Domain/           # Сутності, правила та доменні винятки; не залежить від інших проєктів
│   ├── OnlineStore.Application/      # Сценарії використання та абстракції (IPaymentMethod) → Domain
│   ├── OnlineStore.Infrastructure/   # Реалізації абстракцій (CardPayment, BankTransferPayment) → Application
│   └── OnlineStore.App/              # Console Application, composition root → усі проєкти
├── tests/
│   └── OnlineStore.Domain.Tests/     # Unit-тести доменних правил (xUnit) → Domain
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
| `Customer` | `FullName`, `Email` | — |
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
