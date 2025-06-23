using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using GCloud.Models.Domain;
using RefactorThis.GraphDiff;

namespace GCloud.Service
{
    public interface IAbstractService<T> where T : class
    {
        bool Any(Expression<Func<T, bool>> expression);
        IQueryable<T> FindAll();
        T FindById(object id);
        IQueryable<T> FindBy(Expression<Func<T, bool>> expr);
        ICollection<T> GetLocal();
        void Update(T entity);
        void Update<TUpdateType>(TUpdateType entity, Expression<Func<IUpdateConfiguration<TUpdateType>, object>> mapping) where TUpdateType : class, T, new();
        void Add(T entity);
        void Delete(T entity);
        void LoadCollection<TProperty>(T entity, Expression<Func<T, ICollection<TProperty>>> path) where TProperty : class;
        void LoadCollection(T entity, string referenceName);
        IQueryable<T> LoadExpandedEntities<TProperty>(IQueryable<T> query, Expression<Func<T, TProperty>> path);
        void SetState(T entity, EntityState state);
    }
}