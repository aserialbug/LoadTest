using Microsoft.OpenApi.Models;

namespace Balance.Domain;

public class Balance
{
    private readonly List<Operation> _operations = new();
    private List<Operation>? _trimmedOperations;
    public Guid Id { get; }

    public double Amount { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; private set; }
    public int Version { get; private set; }
    public IEnumerable<Operation> Operations => _operations;

    public Balance(Guid id, IEnumerable<Operation> operations, DateTime createdAt, DateTime updatedAt)
    {
        if (id == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(id));

        if (operations == null)
            throw new ArgumentNullException(nameof(operations));
        
        if(createdAt == DateTime.MinValue)
            throw new ArgumentOutOfRangeException(nameof(createdAt));
        
        if(updatedAt == DateTime.MinValue)
             throw new ArgumentOutOfRangeException(nameof(updatedAt));
        
        Id = id;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;

        _operations.AddRange(operations);
        if (_operations.Count == 0)
            throw new ArgumentOutOfRangeException(nameof(operations));

        var comparer = Comparer<Operation>.Create((op1, op2) => op1.SequenceNumber.CompareTo(op2.SequenceNumber));
        _operations.Sort(comparer);
        
        if (_operations[0].Type != OperationType.Initial)
            throw new ArgumentOutOfRangeException(nameof(operations));

        foreach (var operation in _operations)
        {
            UpdateAmount(operation);
        }

        Version = _operations[^1].SequenceNumber;
    }

    public Operation Deposit(double depositAmount) => CreateOperation(depositAmount, OperationType.Deposit);

    public Operation Expense(double expenseAmount) =>  CreateOperation(expenseAmount, OperationType.Expense);

    public Operation TrimHistory()
    {
        if (_operations.Count == 1)
            return _operations[0];
        
        var operation = Operation.New(OperationType.Initial, Amount, Version + 1);
        _trimmedOperations ??= new List<Operation>();
        _trimmedOperations.AddRange(_operations);
        _operations.Clear();
        _operations.Add(operation);
        Version = operation.SequenceNumber;
        return operation;
    }

    public IEnumerable<Operation> GetTrimmedOperations()
    {
        if (_trimmedOperations == null)
            return Enumerable.Empty<Operation>();

        return _trimmedOperations;
    }

    private Operation CreateOperation(double operationAmount, OperationType type)
    {
        if (operationAmount <= 0d)
            throw new ArgumentOutOfRangeException(nameof(operationAmount));
        
        var operation = Operation.New(type, operationAmount, Version + 1);
        UpdateAmount(operation);
        _operations.Add(operation);
        UpdatedAt = DateTime.Now;
        Version = operation.SequenceNumber;
        return operation;
    }

    private void UpdateAmount(Operation operation)
    {
        switch (operation.Type)
        {
            case OperationType.Initial:
                Amount = operation.Amount;
                break;
            case OperationType.Deposit:
                Amount += operation.Amount;
                break;
            case OperationType.Expense:
                if (Amount - operation.Amount < 0)
                    throw new InvalidOperationException($"{operation.Amount} is lager than balance amount");
                Amount -= operation.Amount;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public static Balance New(Guid id, double amount = 0)
    {
        var sequenceNumber = 0;
        var initOperation = new List<Operation>
        {
            Operation.New(OperationType.Initial, amount, sequenceNumber)
        };
        return new Balance(id, initOperation, initOperation[0].Created, initOperation[0].Created);
    }
}