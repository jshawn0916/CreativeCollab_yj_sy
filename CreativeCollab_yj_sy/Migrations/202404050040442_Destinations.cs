namespace CreativeCollab_yj_sy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Destinations : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Destinations",
                c => new
                    {
                        DestinationID = c.Int(nullable: false, identity: true),
                        DestinationName = c.String(),
                        Description = c.String(),
                        Location = c.String(),
                    })
                .PrimaryKey(t => t.DestinationID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Destinations");
        }
    }
}
