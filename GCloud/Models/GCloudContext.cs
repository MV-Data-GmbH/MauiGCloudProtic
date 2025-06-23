using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SqlClient;
using EntityFramework.DynamicFilters;
using Microsoft.AspNet.Identity.EntityFramework;
using GCloud.Models.Domain;
using GCloud.Models.Domain.CouponUsageAction;
using GCloud.Models.Domain.CouponUsageRequirement;

namespace GCloud.Models.Domain
{
    using System;
    using System.Data.Entity;
    using System.Linq;

    public class GCloudContext : IdentityDbContext<User>
    {
        // Your context has been configured to use a 'CloudModel' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'GCloud.Models.CloudModel' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'CloudModel' 
        // connection string in the application configuration file.
        public GCloudContext()
            : base("name=GCloudContext")
        {
        }

        public IDbSet<MobilePhone> MobilePhones { get; set; }
        public IDbSet<Coupon> Coupons { get; set; }
        public IDbSet<Company> Companies { get; set; }
        public IDbSet<Store> Stores { get; set; }
        public IDbSet<Device> Devices { get; set; }
        public IDbSet<Country> Countries { get; set; }
        public IDbSet<TelNr> TelNrs { get; set; }
        public IDbSet<Redeem> Redeems { get; set; }
        public IDbSet<CouponImage> CouponImages { get; set; }
        public IDbSet<TurnoverJournal> TurnoverJournals { get; set; }
        public IDbSet<AbstractUsageAction> UsageActions { get; set; }
        public IDbSet<AbstractUsageRequirement> UsageRequirements { get; set; }
        public IDbSet<Bill> Bills { get; set; }
        public IDbSet<AnonymousMobilePhone> AnonymousMobilePhones { get; set; }
        public IDbSet<AnonymousUser> AnonymousUsers { get; set; }

        public IDbSet<PointsFromKassa> PointsFromKassas { get; set; }

        public IDbSet<SpecialProduct> SpecialProducts { get; set; } 

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Error>().ToTable("Errors");
            modelBuilder.Conventions.Remove<ForeignKeyIndexConvention>();
            //modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            //modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();

            modelBuilder.Filter("SoftDelete", (ISoftDeletable c) => c.IsDeleted,false);
            modelBuilder.Filter("CompanyFilter", (Company c) => c.User.IsDeleted == false);
            modelBuilder.Filter("StoreFilter", (Store s) => s.Company.IsDeleted == false && s.Company.User.IsDeleted == false && s.Company.User.IsActive);
            modelBuilder.Filter("CouponFilter", (Coupon c) => !c.AssignedStores.Any() || c.AssignedStores.Any(store => store.IsDeleted == false && store.Company.IsDeleted == false && store.Company.User.IsDeleted == false && store.Company.User.IsActive));
            modelBuilder.Filter("RedeemFilter", (Redeem r) => r.Coupon.IsDeleted == false && (!r.Coupon.AssignedStores.Any() || r.Coupon.AssignedStores.Any(store => store.IsDeleted == false && store.Company.IsDeleted == false && store.Company.User.IsDeleted == false && store.Company.User.IsActive)));
        }

        public static GCloudContext Create()
        {
            return new GCloudContext();
        }

        //public override int SaveChanges()
        //{
        //    foreach (var entry in ChangeTracker.Entries()
        //        .Where(p => p.Entity is ISoftDeletable && p.State == EntityState.Deleted))
        //        SoftDelete(entry);

        //    return base.SaveChanges();
        //}

        private void SoftDelete(DbEntityEntry entry)
        {
            Type entryEntityType = entry.Entity.GetType();

            string tableName = GetTableName(entryEntityType);
            string primaryKeyName = GetPrimaryKeyName(entryEntityType);

            string deletequery = $"UPDATE {tableName} SET IsDeleted = 1 WHERE {primaryKeyName} = @id";

            Database.ExecuteSqlCommand(
                deletequery,
                new SqlParameter("@id", entry.OriginalValues[primaryKeyName]));

            //Marking it Unchanged prevents the hard delete
            //entry.State = EntityState.Unchanged;
            //So does setting it to Detached:
            //And that is what EF does when it deletes an item
            //http://msdn.microsoft.com/en-us/data/jj592676.aspx
            entry.State = EntityState.Detached;
        }

        private static Dictionary<Type, EntitySetBase> _mappingCache = new Dictionary<Type, EntitySetBase>();

        private EntitySetBase GetEntitySet(Type type)
        {
            if (!_mappingCache.ContainsKey(type))
            {
                ObjectContext octx = ((IObjectContextAdapter)this).ObjectContext;

                string typeName = ObjectContext.GetObjectType(type).Name;

                var es = octx.MetadataWorkspace
                    .GetItemCollection(DataSpace.SSpace)
                    .GetItems<EntityContainer>()
                    .SelectMany(c => c.BaseEntitySets
                        .Where(e => e.Name == typeName))
                    .FirstOrDefault();

                if (es == null)
                    throw new ArgumentException("Entity type not found in GetTableName", typeName);

                _mappingCache.Add(type, es);
            }

            return _mappingCache[type];
        }

        private string GetTableName(Type type)
        {
            EntitySetBase es = GetEntitySet(type);

            return $"[{es.MetadataProperties["Schema"].Value}].[{es.MetadataProperties["Table"].Value}]";
        }

        private string GetPrimaryKeyName(Type type)
        {
            var es = GetEntitySet(type);

            return es.ElementType.KeyMembers[0].Name;
        }
    }

}