namespace Balance.Interfaces;

public interface IBalanceDao
{
    public Task<Domain.Balance> GetById(Guid id);
    public Task Add(Domain.Balance balance);
}