using ElectronicLibrary.Application.Interfaces;
using ElectronicLibrary.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace ElectronicLibrary.Persistence.Repositories;

public class Repository<TEntity>(ApplicationDbContext context) : IRepository<TEntity> where TEntity : BaseEntity
{
    protected readonly ApplicationDbContext Context = context;
    protected readonly DbSet<TEntity> Set = context.Set<TEntity>();

    public async Task<TEntity?> GetByIdAsync(object id, CancellationToken ct = default) =>
        await Set.FindAsync([id], ct);

    public async Task<List<TEntity>> GetAllAsync(CancellationToken ct = default) =>
        await Set.ToListAsync(ct);

    public async Task AddAsync(TEntity entity, CancellationToken ct = default)
    {
        await Set.AddAsync(entity, ct);
        await Context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(TEntity entity, CancellationToken ct = default)
    {
        Set.Update(entity);
        await Context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(TEntity entity, CancellationToken ct = default)
    {
        if (entity is ISoftDeletable softDeletable)
        {
            softDeletable.IsActive = false;
            Set.Update(entity);
        }
        else
        {
            Set.Remove(entity);
        }

        await Context.SaveChangesAsync(ct);
    }
}
