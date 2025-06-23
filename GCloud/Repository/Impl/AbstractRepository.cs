using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using GCloud.Models.Domain;
using RefactorThis.GraphDiff;

namespace GCloud.Repository
{
    public abstract class AbstractRepository<T> : IAbstractRepository<T> where T : class { 

        protected readonly DbContext _context;
        public bool AutoCommit { get; set; } = true;

        protected AbstractRepository(DbContext context)
        {
            _context = context;
        }

        public bool Any(Expression<Func<T, bool>> expression)
        {
            return _context.Set<T>().Any(expression);
        }

        public T FindById(object id)
        {
            return _context.Set<T>().Find(id);
        }

        public ICollection<T> GetLocal()
        {
            return _context.Set<T>().Local;
        }

        public IQueryable<T> FindAll()
        {
            return _context.Set<T>();
        }

        public IQueryable<T> FindBy(Expression<Func<T, bool>> predicate)
        {
            return _context.Set<T>().Where(predicate);
        }

        public T FindFirstOrDefault(Expression<Func<T, bool>> predicate)
        {
            return _context.Set<T>().FirstOrDefault(predicate);
        }

        public void Add(T entity)
        {
            _context.Set<T>().Add(entity);
            if (AutoCommit)
            {
                Save();
            }
        }

        public void Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
            if (AutoCommit)
            {
                Save();
            }
        }

        public void Delete(IEnumerable<T> entities)
        {
            _context.Set<T>().RemoveRange(entities);
        }

        public void Update(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            if (AutoCommit)
            {
                Save();
            }
            //var idValue = entity.GetType().GetProperty("Id")?.GetValue(entity);
            //var store = _context.Set<T>().Find(idValue);
            //_context.Entry(store).CurrentValues.SetValues(entity);
        }

        public void Update<TUpdateType>(TUpdateType entity, Expression<Func<IUpdateConfiguration<TUpdateType>, object>> mapping) where TUpdateType : class, T, new()
        {
            _context.UpdateGraph(entity, mapping);
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public bool Exists(object id)
        {
            return FindById(id) != null;
        }

        public void SetState(T entity, EntityState state)
        {
            _context.Entry(entity).State = state;
        }

        public void LoadCollection<TProperty>(T entity, Expression<Func<T,ICollection<TProperty>>> path) where TProperty : class
        {
            _context.Entry(entity).Collection(path).Load();
        }

        public void LoadCollection(T entity, string referenceName)
        {
            _context.Entry(entity).Collection(referenceName).Load();
        }

        public IQueryable<T> GetExpandedEntities<TProperty>(IQueryable<T> query, Expression<Func<T, TProperty>> path)
        {
            return query.Include(path);
        }
    }
}