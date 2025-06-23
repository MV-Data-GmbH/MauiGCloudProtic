using SQLite;

namespace GCloudShared.Domain
{
    public class BasePersistable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
    }
}
