namespace GCloud.Models.Domain
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Bills", "StoreApiToken", c => c.String());
            DropColumn("dbo.Bills", "StoreName");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Bills", "StoreName", c => c.String());
            DropColumn("dbo.Bills", "StoreApiToken");
        }
    }
}
