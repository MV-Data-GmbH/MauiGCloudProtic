using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using GCloud.Models.Domain;
using GCloud.Repository;
using RefactorThis.GraphDiff;

namespace GCloud.Service.Impl
{
    public abstract class AbstractService<T> : IAbstractService<T> where T : class
    {
        protected readonly IAbstractRepository<T> _repository;

        protected AbstractService(IAbstractRepository<T> repository)
        {
            _repository = repository;
        }

        public virtual T FindById(object id)
        {
            return _repository.FindById(id);
        }

        public ICollection<T> GetLocal()
        {
            return _repository.GetLocal();
        }
        
        public IQueryable<T> FindBy(Expression<Func<T, bool>> expr)
        {
            return _repository.FindBy(expr);
        }

        public bool Any(Expression<Func<T, bool>> expression)
        {
            return _repository.Any(expression);
        }

        public virtual IQueryable<T> FindAll()
        {
            return _repository.FindAll();
        }

        public virtual void Update(T entity)
        {
            _repository.Update(entity);
            _repository.Save();
        }

        public virtual void Update<TUpdateType>(TUpdateType entity, Expression<Func<IUpdateConfiguration<TUpdateType>, object>> mapping) where TUpdateType : class, T, new()
        {
            _repository.SetState(entity, EntityState.Detached);
            _repository.Update(entity, mapping);
            _repository.Save();
        }

        public virtual void Add(T entity)
        {
            _repository.Add(entity);
            _repository.Save();
        }

        public virtual void Delete(T entity)
        {
            _repository.Delete(entity);
            _repository.Save();
        }

        public void LoadCollection<TProperty>(T entity, Expression<Func<T, ICollection<TProperty>>> path) where TProperty : class
        {
            _repository.LoadCollection(entity, path);
        }

        public void LoadCollection(T entity, string referenceName)
        {
            _repository.LoadCollection(entity,referenceName);
        }

        public IQueryable<T> LoadExpandedEntities<TProperty>(IQueryable<T> query, Expression<Func<T, TProperty>> path)
        {
            return _repository.GetExpandedEntities(query, path);
        }
        
        public void SetState(T entity, EntityState state)
        {
            _repository.SetState(entity, state);
        }
    }
}