using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Ajax.Utilities;
using RefactorThis.GraphDiff;

namespace GCloud.Repository
{
    public interface IAbstractRepository<TEntity> where TEntity : class
    {
        bool Any(Expression<Func<TEntity, bool>> expression);
        TEntity FindById(object id);
        ICollection<TEntity> GetLocal();
        IQueryable<TEntity> FindAll();
        IQueryable<TEntity> FindBy(Expression<Func<TEntity, bool>> predicate);
        TEntity FindFirstOrDefault(Expression<Func<TEntity, bool>> predicate);
        void Add(TEntity entity);
        void Delete(TEntity entity);
        void Delete(IEnumerable<TEntity> entities);
        void Update(TEntity entity);
        void Update<TUpdateType>(TUpdateType entity, Expression<Func<IUpdateConfiguration<TUpdateType>, object>> mapping) where TUpdateType : class, TEntity, new();
        void Save();
        bool Exists(object id);

        void SetState(TEntity entity, EntityState state);

        void LoadCollection(TEntity entity, string referenceName);

        void LoadCollection<TProperty>(TEntity entity, Expression<Func<TEntity, ICollection<TProperty>>> path) where TProperty : class;

        IQueryable<TEntity> GetExpandedEntities<TProperty>(IQueryable<TEntity> query, Expression<Func<TEntity, TProperty>> path);
    }
}
