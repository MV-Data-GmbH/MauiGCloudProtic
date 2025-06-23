namespace GCloud.Models.Domain
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AdditionalChanges : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Stores", "SpecialProduct_Id", c => c.Guid());
            AddForeignKey("dbo.Stores", "SpecialProduct_Id", "dbo.SpecialProducts", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Stores", "SpecialProduct_Id", "dbo.SpecialProducts");
            DropColumn("dbo.Stores", "SpecialProduct_Id");
        }
    }
}
