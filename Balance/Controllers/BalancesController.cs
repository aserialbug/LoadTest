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

    public BalancesController(ILogger<BalancesController> logger, 
        IBalanceService balanceService)
    {
        _logger = logger;
        _balanceService = balanceService;
    }

    [HttpGet]
    public async Task<BalanceDto> Get([FromQuery]Guid balanceId)
    {
        var balance = await _balanceService.GetById(balanceId);
        return new BalanceDto(balance.Id, balance.Amount);
    }

    [HttpPost]
    public async Task<Guid> Post([FromBody]CreateBalanceDto dto)
    {
        var balance = await _balanceService.Create(dto.InitialAmount);
        return balance.Id;
    }
}