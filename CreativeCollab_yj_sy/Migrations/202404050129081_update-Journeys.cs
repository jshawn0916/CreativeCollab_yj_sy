namespace CreativeCollab_yj_sy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateJourneys : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Journeys", "UserID", "dbo.AspNetUsers");
            DropIndex("dbo.Journeys", new[] { "UserID" });
            AlterColumn("dbo.Journeys", "JourneyName", c => c.String(nullable: false));
            AlterColumn("dbo.Journeys", "JourneyDescription", c => c.String(nullable: false));
            AlterColumn("dbo.Journeys", "UserID", c => c.String(nullable: false, maxLength: 128));
            CreateIndex("dbo.Journeys", "UserID");
            AddForeignKey("dbo.Journeys", "UserID", "dbo.AspNetUsers", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Journeys", "UserID", "dbo.AspNetUsers");
            DropIndex("dbo.Journeys", new[] { "UserID" });
            AlterColumn("dbo.Journeys", "UserID", c => c.String(maxLength: 128));
            AlterColumn("dbo.Journeys", "JourneyDescription", c => c.String());
            AlterColumn("dbo.Journeys", "JourneyName", c => c.String());
            CreateIndex("dbo.Journeys", "UserID");
            AddForeignKey("dbo.Journeys", "UserID", "dbo.AspNetUsers", "Id");
        }
    }
}
