using System.Linq.Expressions;
using CommunityEvents.Data;
using CommunityEvents.Interfaces;
using CommunityEvents.Models;
using Microsoft.EntityFrameworkCore;

namespace CommunityEvents.Repositories;

/// <summary>
/// Generic base repository providing default CRUD operations via EF Core.
/// Concrete repositories inherit this and extend with entity-specific queries.
/// Demonstrates: Inheritance, Generic programming, Repository pattern.
/// </summary>
public abstract class BaseRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    protected BaseRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id)
        => await _dbSet.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

    public virtual async Task<IEnumerable<T>> GetAllAsync()
        => await _dbSet.Where(e => !e.IsDeleted).ToListAsync();

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.Where(predicate).Where(e => !e.IsDeleted).ToListAsync();

    public virtual async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task UpdateAsync(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id)
            ?? throw new Exceptions.NotFoundException(typeof(T).Name, id);
        entity.SoftDelete();
        await _context.SaveChangesAsync();
    }

    public virtual async Task<bool> ExistsAsync(int id)
        => await _dbSet.AnyAsync(e => e.Id == id && !e.IsDeleted);

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        var query = _dbSet.Where(e => !e.IsDeleted);
        if (predicate is not null)
            query = query.Where(predicate);
        return await query.CountAsync();
    }
}
