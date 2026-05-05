using Microsoft.AspNetCore.Mvc;

namespace Balance.Domain;

public class Operation
{
    public Guid Id { get; }
    public OperationType Type { get; }
    public double Amount  { get; }
    public int SequenceNumber  { get; }
    public DateTime Created  { get; }
    
    public Operation(Guid id, OperationType type, double amount, int sequenceNumber, DateTime created)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        if (SequenceNumber < 0)
            throw new ArgumentOutOfRangeException(nameof(sequenceNumber));
        
        if(Created == DateTime.MinValue || Created == DateTime.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(created));
        
        Type = type;
        Amount = amount;
        SequenceNumber = sequenceNumber;
        Created = created;
        Id = id;
    }

    public static Operation New(OperationType type, double amount, int sequenceNumber)
        => new Operation(Guid.NewGuid(), type, amount, sequenceNumber, DateTime.Now);
}
    