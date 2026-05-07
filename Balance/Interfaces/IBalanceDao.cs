using Balance.Dto;

namespace Balance.Interfaces;

public interface IBalanceDao
{
    public Task<Domain.Balance> GetById(Guid id);
    public Task<IEnumerable<BalanceListDto>> GetBalanceList();
    public Task Upsert(Domain.Balance balance);
}