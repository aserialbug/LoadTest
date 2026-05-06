using Balance.Domain;

namespace Balance.Dto;


public record OperationDto(Guid Id, OperationType Type, double Amount, int SequenceNumber, DateTime Created);