namespace Kladr.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class add_regionandcity : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DbElements", "Region", c => c.String());
            AddColumn("dbo.DbElements", "City", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.DbElements", "City");
            DropColumn("dbo.DbElements", "Region");
        }
    }
}
