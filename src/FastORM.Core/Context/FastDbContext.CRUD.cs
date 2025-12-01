using System.Diagnostics.CodeAnalysis;

namespace FastORM;

public partial class FastDbContext
{
    #region Insert Stubs (Intercepted)

    public int Insert<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(T entity) 
        => throw new InvalidOperationException("This method should be intercepted by the compiler.");

    public int Insert<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(IEnumerable<T> entities) 
        => throw new InvalidOperationException("This method should be intercepted by the compiler.");

    public Task<int> InsertAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(T entity, CancellationToken cancellationToken = default) 
        => throw new InvalidOperationException("This method should be intercepted by the compiler.");

    public Task<int> InsertAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(IEnumerable<T> entities, CancellationToken cancellationToken = default) 
        => throw new InvalidOperationException("This method should be intercepted by the compiler.");

    #endregion

    #region Update Stubs (Intercepted)

    public int Update<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(T entity) 
        => throw new InvalidOperationException("This method should be intercepted by the compiler.");

    public int Update<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(IEnumerable<T> entities) 
        => throw new InvalidOperationException("This method should be intercepted by the compiler.");

    public Task<int> UpdateAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(T entity, CancellationToken cancellationToken = default) 
        => throw new InvalidOperationException("This method should be intercepted by the compiler.");

    public Task<int> UpdateAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(IEnumerable<T> entities, CancellationToken cancellationToken = default) 
        => throw new InvalidOperationException("This method should be intercepted by the compiler.");

    #endregion

    #region Delete Stubs (Intercepted)

    public int Delete<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(T entity) 
        => throw new InvalidOperationException("This method should be intercepted by the compiler.");

    public int Delete<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(IEnumerable<T> entities) 
        => throw new InvalidOperationException("This method should be intercepted by the compiler.");

    public Task<int> DeleteAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(T entity, CancellationToken cancellationToken = default) 
        => throw new InvalidOperationException("This method should be intercepted by the compiler.");

    public Task<int> DeleteAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(IEnumerable<T> entities, CancellationToken cancellationToken = default) 
        => throw new InvalidOperationException("This method should be intercepted by the compiler.");

    #endregion
}
