using System.Diagnostics.CodeAnalysis;

namespace FastORM;

public partial class FastDbContext
{
    #region Schema Stubs (Intercepted)

    public void CreateTable<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>() 
        => throw new InvalidOperationException("This method should be intercepted by the compiler.");

    public Task CreateTableAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(CancellationToken cancellationToken = default) 
        => throw new InvalidOperationException("This method should be intercepted by the compiler.");

    public void DropTable<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>() 
        => throw new InvalidOperationException("This method should be intercepted by the compiler.");

    public Task DropTableAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(CancellationToken cancellationToken = default) 
        => throw new InvalidOperationException("This method should be intercepted by the compiler.");

    #endregion
}
