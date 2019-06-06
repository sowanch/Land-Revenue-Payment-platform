namespace TangaDummy.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IDnumber : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "IDNumber", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "IDNumber");
        }
    }
}
