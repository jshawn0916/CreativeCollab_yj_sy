namespace CreativeCollab_yj_sy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Restaurants : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Restaurants",
                c => new
                    {
                        RestaurantID = c.Int(nullable: false, identity: true),
                        RestaurantName = c.String(),
                        Description = c.String(),
                        rate = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Location = c.String(),
                    })
                .PrimaryKey(t => t.RestaurantID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Restaurants");
        }
    }
}
