namespace SwimmingSchool_Implementation.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixPendingChanges : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Lessons", "UserId", "dbo.AspNetUsers");
            AddColumn("dbo.Lessons", "User_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.Lessons", "User_Id");
            AddForeignKey("dbo.Lessons", "User_Id", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Lessons", "User_Id", "dbo.AspNetUsers");
            DropIndex("dbo.Lessons", new[] { "User_Id" });
            DropColumn("dbo.Lessons", "User_Id");
            AddForeignKey("dbo.Lessons", "UserId", "dbo.AspNetUsers", "Id");
        }
    }
}
