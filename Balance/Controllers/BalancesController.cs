using Balance.Dto;
using Balance.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Balance.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BalancesController : ControllerBase
{

    private readonly ILogger<BalancesController> _logger;
    private readonly IBalanceService _balanceService;
    private readonly IBalanceDao _balanceDao;

    public BalancesController(ILogger<BalancesController> logger, 
        IBalanceService balanceService, IBalanceDao balanceDao)
    {
        _logger = logger;
        _balanceService = balanceService;
        _balanceDao = balanceDao;
    }

    [HttpGet]
    public Task<IEnumerable<BalanceListDto>> Get()
    {
        return _balanceDao.GetBalanceList();
    }
    
    [HttpPost]
    public async Task<Guid> Post([FromBody]CreateBalanceDto dto)
    {
        var balance = await _balanceService.Create(dto.InitialAmount);
        return balance.Id;
    }

    [HttpGet("{id}")]
    public async Task<BalanceDto> GetBalance([FromRoute]Guid id)
    {
        var balance = await _balanceService.GetById(id);
        var operations =
            balance.Operations.Select(
                o => new OperationDto(o.Id, o.Type, o.Amount, o.SequenceNumber, o.Created))
                .ToList();
        
        return new BalanceDto(balance.Id, balance.Amount, operations);
    }

    [HttpPost("{id}/deposit")]
    public async Task<Guid> Deposit([FromRoute] Guid id, [FromBody] DepositParameterDto parameter)
    {
        var operation = await _balanceService.Deposit(id, parameter.Amount);
        return operation.Id;
    }
    
    [HttpPost("{id}/expense")]
    public async Task<Guid> Expense([FromRoute] Guid id, [FromBody] ExpenseParameterDto parameter)
    {
        var operation = await _balanceService.Expense(id, parameter.Amount);
        return operation.Id;
    }
    
    [HttpPost("{id}/trim")]
    public async Task<Guid> Trim([FromRoute] Guid id)
    {
        var operation = await _balanceService.Trim(id);
        return operation.Id;
    }
}