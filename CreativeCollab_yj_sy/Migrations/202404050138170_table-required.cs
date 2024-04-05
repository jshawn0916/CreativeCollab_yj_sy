namespace CreativeCollab_yj_sy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tablerequired : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.JourneyDestinations",
                c => new
                    {
                        Journey_JourneyID = c.Int(nullable: false),
                        Destination_DestinationID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Journey_JourneyID, t.Destination_DestinationID })
                .ForeignKey("dbo.Journeys", t => t.Journey_JourneyID, cascadeDelete: true)
                .ForeignKey("dbo.Destinations", t => t.Destination_DestinationID, cascadeDelete: true)
                .Index(t => t.Journey_JourneyID)
                .Index(t => t.Destination_DestinationID);
            
            CreateTable(
                "dbo.RestaurantJourneys",
                c => new
                    {
                        Restaurant_RestaurantID = c.Int(nullable: false),
                        Journey_JourneyID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Restaurant_RestaurantID, t.Journey_JourneyID })
                .ForeignKey("dbo.Restaurants", t => t.Restaurant_RestaurantID, cascadeDelete: true)
                .ForeignKey("dbo.Journeys", t => t.Journey_JourneyID, cascadeDelete: true)
                .Index(t => t.Restaurant_RestaurantID)
                .Index(t => t.Journey_JourneyID);
            
            AddColumn("dbo.Destinations", "DestinationDescription", c => c.String(nullable: false));
            AddColumn("dbo.Restaurants", "RestaurantDescription", c => c.String(nullable: false));
            AlterColumn("dbo.Destinations", "DestinationName", c => c.String(nullable: false));
            AlterColumn("dbo.Destinations", "Location", c => c.String(nullable: false));
            AlterColumn("dbo.Restaurants", "RestaurantName", c => c.String(nullable: false));
            AlterColumn("dbo.Restaurants", "Location", c => c.String(nullable: false));
            DropColumn("dbo.Destinations", "Description");
            DropColumn("dbo.Restaurants", "Description");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Restaurants", "Description", c => c.String());
            AddColumn("dbo.Destinations", "Description", c => c.String());
            DropForeignKey("dbo.RestaurantJourneys", "Journey_JourneyID", "dbo.Journeys");
            DropForeignKey("dbo.RestaurantJourneys", "Restaurant_RestaurantID", "dbo.Restaurants");
            DropForeignKey("dbo.JourneyDestinations", "Destination_DestinationID", "dbo.Destinations");
            DropForeignKey("dbo.JourneyDestinations", "Journey_JourneyID", "dbo.Journeys");
            DropIndex("dbo.RestaurantJourneys", new[] { "Journey_JourneyID" });
            DropIndex("dbo.RestaurantJourneys", new[] { "Restaurant_RestaurantID" });
            DropIndex("dbo.JourneyDestinations", new[] { "Destination_DestinationID" });
            DropIndex("dbo.JourneyDestinations", new[] { "Journey_JourneyID" });
            AlterColumn("dbo.Restaurants", "Location", c => c.String());
            AlterColumn("dbo.Restaurants", "RestaurantName", c => c.String());
            AlterColumn("dbo.Destinations", "Location", c => c.String());
            AlterColumn("dbo.Destinations", "DestinationName", c => c.String());
            DropColumn("dbo.Restaurants", "RestaurantDescription");
            DropColumn("dbo.Destinations", "DestinationDescription");
            DropTable("dbo.RestaurantJourneys");
            DropTable("dbo.JourneyDestinations");
        }
    }
}
