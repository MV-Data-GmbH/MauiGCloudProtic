namespace GCloud.Models.Domain
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddingNewColumnsInCompanies : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "AwardPoints", c => c.Int(nullable: false));
            AddColumn("dbo.Companies", "RegistrationPoints", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Companies", "RegistrationPoints");
            DropColumn("dbo.Companies", "AwardPoints");
        }
    }
}
