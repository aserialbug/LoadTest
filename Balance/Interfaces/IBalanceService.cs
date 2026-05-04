namespace Balance.Interfaces;

public interface IBalanceService
{
    public Task<Domain.Balance> GetById(Guid balanceId);
    public Task<Domain.Balance> Create(double? initialAmount = 0);
}