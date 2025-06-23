using GCloudShared.Domain;
using SQLite;

namespace GCloudShared.Repository
{
    public class UserRepository: AbstractRepository<User>
    {
        public UserRepository(SQLiteConnection connection) : base(connection)
        {
        }

        public User GetCurrentUser()
        {
            var usercount = Count();

            return usercount > 0 ? FindAll().First() : null;
        }

        public override int Insert(User entity)
        {
            var current = Count();

            if (current > 0)
            {
                DeleteAll();
            }
            return base.Insert(entity);
        }
    }
}
