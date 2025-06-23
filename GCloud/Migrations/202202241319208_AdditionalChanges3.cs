namespace GCloud.Models.Domain
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AdditionalChanges3 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.SpecialProducts", "Company_Id", "dbo.Companies");
            DropColumn("dbo.SpecialProducts", "Company_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SpecialProducts", "Company_Id", c => c.Guid());
            AddForeignKey("dbo.SpecialProducts", "Company_Id", "dbo.Companies", "Id");
        }
    }
}
