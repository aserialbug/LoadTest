namespace Balance.Domain;

public class Balance
{
    public Guid Id { get; }
    public double Amount { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; private set; }
    public int Version { get; private set; }

    public Balance(Guid id, double amount, DateTime createdAt, DateTime updatedAt, int version)
    {
        if (id == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(id));
        
        if(amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));
        
        Id = id;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        Version = version;
        Amount = amount;
    }

    public void Deposit(double depositAmount)
    {
        if (depositAmount <= 0d)
            throw new ArgumentOutOfRangeException(nameof(depositAmount));
        
        Amount += depositAmount;
        UpdatedAt = DateTime.Now;
        Version++;
    }

    public void Expense(double expenseAmount)
    {
        if (expenseAmount <= 0d)
            throw new ArgumentOutOfRangeException(nameof(expenseAmount));
        
        if (Amount - expenseAmount < 0)
            throw new InvalidOperationException($"{expenseAmount} is lager than balance amount");

        Amount -= expenseAmount;
        UpdatedAt = DateTime.Now;
        Version++;
    }

    public static Balance New(Guid id, double amount = 0) 
        => new Balance(id, amount, DateTime.Now, DateTime.Now, 0);
}