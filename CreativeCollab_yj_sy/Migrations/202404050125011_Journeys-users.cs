namespace CreativeCollab_yj_sy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Journeysusers : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Journeys",
                c => new
                    {
                        JourneyID = c.Int(nullable: false, identity: true),
                        JourneyName = c.String(),
                        JourneyDescription = c.String(),
                        UserID = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.JourneyID)
                .ForeignKey("dbo.AspNetUsers", t => t.UserID)
                .Index(t => t.UserID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Journeys", "UserID", "dbo.AspNetUsers");
            DropIndex("dbo.Journeys", new[] { "UserID" });
            DropTable("dbo.Journeys");
        }
    }
}
