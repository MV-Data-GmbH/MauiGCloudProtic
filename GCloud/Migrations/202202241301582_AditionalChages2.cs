namespace GCloud.Models.Domain
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AditionalChages2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SpecialProducts", "Company_Id", c => c.Guid());
            AddForeignKey("dbo.SpecialProducts", "Company_Id", "dbo.Companies", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SpecialProducts", "Company_Id", "dbo.Companies");
            DropColumn("dbo.SpecialProducts", "Company_Id");
        }
    }
}
