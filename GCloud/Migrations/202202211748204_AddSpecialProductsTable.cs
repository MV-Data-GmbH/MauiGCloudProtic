namespace GCloud.Models.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure.Annotations;
    using System.Data.Entity.Migrations;
    
    public partial class AddSpecialProductsTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SpecialProducts",
                c => new
                    {
                        Id = c.Guid(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        ShortDescription = c.String(),
                        Value = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CreatedUserId = c.String(maxLength: 128),
                        Enabled = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                    },
                annotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_SpecialProduct_SoftDelete", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SpecialProducts", "CreatedUserId", "dbo.AspNetUsers");
            DropTable("dbo.SpecialProducts",
                removedAnnotations: new Dictionary<string, object>
                {
                    { "DynamicFilter_SpecialProduct_SoftDelete", "EntityFramework.DynamicFilters.DynamicFilterDefinition" },
                });
        }
    }
}
