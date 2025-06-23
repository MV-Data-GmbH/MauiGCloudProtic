using GCloudShared.Domain;
using SQLite;
using System.Linq.Expressions;

namespace GCloudShared.Repository
{
    public abstract class AbstractRepository<T> where T : BasePersistable, new()
    {
        protected readonly SQLiteConnection _connection;

        protected AbstractRepository(SQLiteConnection connection)
        {
            _connection = connection;
        }

        public int Count()
        {
            return _connection.Table<T>().Count();
        }

        public IEnumerable<T> FindAll()
        {
            return _connection.Table<T>().ToList();
        }

        public T FindById(object id)
        {
            return _connection.Find<T>(id);
        }

        public virtual int Insert(T entity)
        {
            return _connection.Insert(entity);
        }

        public int DeleteAll()
        {
            return _connection.DeleteAll<T>();
        }

        public T FirstOrDefault()
        {
            return _connection.Table<T>().FirstOrDefault();
        }

        public IEnumerable<T> FindBy(Expression<Func<T, bool>> predicate)
        {
            return _connection.Table<T>().Where(predicate).ToList();
        }

        public T FindFirstBy(Expression<Func<T, bool>> predicate)
        {
            return _connection.Table<T>().FirstOrDefault(predicate);
        }
    }
}
