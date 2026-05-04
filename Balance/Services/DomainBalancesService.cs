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
        await _balanceDao.Add(balance);
        return balance;
    }
}