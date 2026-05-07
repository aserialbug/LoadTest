using Balance.Domain;

namespace Balance.Interfaces;

public interface IBalanceService
{
    public Task<Domain.Balance> GetById(Guid balanceId);
    public Task<Domain.Balance> Create(double? initialAmount = 0);
    public Task<Operation> Deposit(Guid balanceId, double amount);
    public Task<Operation> Expense(Guid balanceId, double amount);
    public Task<Operation> Trim(Guid balanceId);
}