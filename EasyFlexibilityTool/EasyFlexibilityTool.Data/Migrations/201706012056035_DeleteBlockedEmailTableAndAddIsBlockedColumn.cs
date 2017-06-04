namespace EasyFlexibilityTool.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DeleteBlockedEmailTableAndAddIsBlockedColumn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "IsBlocked", c => c.Boolean(nullable: false));
            DropTable("dbo.BlockedEmail");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.BlockedEmail",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Email = c.String(),
                        DateTimeStamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            DropColumn("dbo.AspNetUsers", "IsBlocked");
        }
    }
}
