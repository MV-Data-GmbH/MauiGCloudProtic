namespace GCloud.Models.Domain
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddingNeStoreWebSiteColumnInStoresTbl : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Stores", "StoreWebSite", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Stores", "StoreWebSite");
        }
    }
}
