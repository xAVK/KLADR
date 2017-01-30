namespace Kladr.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DbElements",
                c => new
                    {
                        ID = c.String(nullable: false, maxLength: 128),
                        Street = c.String(),
                        NumberBuild = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.DbElements");
        }
    }
}
