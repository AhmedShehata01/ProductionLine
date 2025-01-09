using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StartUp.BLL.Services;
using StartUp.DAL.Database;

namespace StartUp.BLL.Repository
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {


        private readonly ApplicationContext _context;
        private readonly IAuditLogService _auditLogService;
        private DbSet<TEntity> dbSet;

        public GenericRepository(ApplicationContext context, IAuditLogService auditLogService)
        {
            _context = context;
            _auditLogService = auditLogService;
            dbSet = context.Set<TEntity>();
        }



        public async Task<IEnumerable<TEntity>> GetAsync(
                                                            Expression<Func<TEntity, bool>>? filter = null,
                                                            int? page = null,
                                                            int pageSize = 0,
                                                            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
                                                            List<Expression<Func<TEntity, object>>>? includeProperties = null,
                                                            bool noTrack = false)
        {
            IQueryable<TEntity> query = dbSet; // _context.employee;

            if (filter != null)
            {
                query = query.Where(filter); // _context.employee.where(a => a.id == 10);
            }

            if (includeProperties != null)
            {
                foreach (var includeProperty in includeProperties)
                {
                    query = query.Include(includeProperty);
                    // _context.employee.where(a => a.id == 10).include(department);
                }
            }

            if (noTrack)
            {
                query = query.AsNoTracking();
                // _context.employee.where(a => a.id == 10).include(department).AsNoTracking();
            }

            if (orderBy != null)
            {
                query = orderBy(query);
                // _context.employee.where(a => a.id == 10).include(department).AsNoTracking().OrderBy(a => a.id);
            }

            if (page.HasValue && page > 0)
            {
                query = query.Skip((page.Value - 1) * pageSize).Take(pageSize);
                // _context.employee.where(a => a.id == 10).include(department).AsNoTracking().OrderBy(a => a.id).skip().take();
            }

            return await query.ToListAsync();
            // _context.employee.where(a => a.id == 10).include(department).AsNoTracking().OrderBy(a => a.id).skip().take().ToListAsync();
        }


        public async Task<int> CountAsync(Expression<Func<TEntity, bool>>? filter = null)
        {
            IQueryable<TEntity> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query.CountAsync();
        }


        // Add a new entity
        public async Task<TEntity> AddAsync(TEntity entity)
        {
            await dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();

            // Log the action
            await _auditLogService.LogAuditAsync("Insert", entity, null);
            return entity;
        }

        // Update an existing entity
        public async Task UpdateAsync(TEntity entity)
        {
            var entry = _context.Entry(entity);
            var originalValues = entry.OriginalValues.ToObject() as TEntity; // Get original values before update
            dbSet.Update(entity);
            await _context.SaveChangesAsync();

            // Log the action
            await _auditLogService.LogAuditAsync("Update", entity, originalValues);
        }

        // Delete an entity by ID
        public async Task DeleteAsync(int id)
        {
            var entity = await dbSet.FindAsync(id);
            if (entity != null)
            {
                dbSet.Remove(entity);
                await _context.SaveChangesAsync();

                // Log the action
                await _auditLogService.LogAuditAsync("Delete", null, entity);
            }
        }


    }
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        public Task<IEnumerable<TEntity>> GetAsync(
                                            Expression<Func<TEntity, bool>>? filter = null,
                                            int? page = null,
                                            int pageSize = 10,
                                            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
                                            List<Expression<Func<TEntity, object>>>? includeProperties = null,
                                            bool noTrack = false);

        public Task<int> CountAsync(Expression<Func<TEntity, bool>>? filter = null);

        // Add a new entity
        Task<TEntity> AddAsync(TEntity entity);

        // Update an existing entity
        Task UpdateAsync(TEntity entity);

        // Delete an entity by ID
        Task DeleteAsync(int id);
    }
}
