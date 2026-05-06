namespace Balance.Dto;

public record BalanceDto(Guid Id, double Amount, List<OperationDto> Operations);