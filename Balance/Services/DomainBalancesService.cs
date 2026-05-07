using Balance.Domain;
using Balance.Interfaces;

namespace Balance.Services;

public class DomainBalancesService : IBalanceService
{
    private readonly IBalanceDao _balanceDao;

    public DomainBalancesService(IBalanceDao balanceDao)
    {
        _balanceDao = balanceDao;
    }

    public Task<Domain.Balance> GetById(Guid balanceId)
    {
        return _balanceDao.GetById(balanceId);
    }

    public async Task<Domain.Balance> Create(double? initialAmount)
    {
        var balance = Domain.Balance.New(Guid.NewGuid(), initialAmount ?? 0);
        await _balanceDao.Upsert(balance);
        return balance;
    }

    public async Task<Operation> Deposit(Guid balanceId, double amount)
    {
        var balance = await _balanceDao.GetById(balanceId);
        var operation = balance.Deposit(amount);
        await _balanceDao.Upsert(balance);
        return operation;
    }

    public async Task<Operation> Expense(Guid balanceId, double amount)
    {
        var balance = await _balanceDao.GetById(balanceId);
        var operation = balance.Expense(amount);
        await _balanceDao.Upsert(balance);
        return operation;
    }

    public async Task<Operation> Trim(Guid balanceId)
    {
        var balance = await _balanceDao.GetById(balanceId);
        var operation = balance.TrimHistory();
        await _balanceDao.Upsert(balance);
        return operation;
    }
}