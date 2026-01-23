using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InkVault.Migrations
{
    /// <inheritdoc />
    public partial class DeleteSpecificUsersV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                BEGIN TRANSACTION;
                
                DECLARE @userIds TABLE (Id NVARCHAR(450));
                INSERT INTO @userIds VALUES 
                    ('837b769d-a59b-4c61-a6d1-cb71ee3f1acc'),
                    ('dde76665-fd1a-44b7-9ca1-78172ea009e2'),
                    ('f4f629d5-18ae-428e-9d83-15c127786604');

                DELETE FROM [JournalViews] WHERE [UserId] IN (SELECT Id FROM @userIds);
                DELETE FROM [FriendRequests] WHERE [SenderId] IN (SELECT Id FROM @userIds) OR [ReceiverId] IN (SELECT Id FROM @userIds);
                DELETE FROM [Friends] WHERE [UserId] IN (SELECT Id FROM @userIds) OR [FriendUserId] IN (SELECT Id FROM @userIds);
                DELETE FROM [Journals] WHERE [UserId] IN (SELECT Id FROM @userIds);
                DELETE FROM [AspNetUserRoles] WHERE [UserId] IN (SELECT Id FROM @userIds);
                DELETE FROM [AspNetUserClaims] WHERE [UserId] IN (SELECT Id FROM @userIds);
                DELETE FROM [AspNetUserTokens] WHERE [UserId] IN (SELECT Id FROM @userIds);
                DELETE FROM [AspNetUserLogins] WHERE [UserId] IN (SELECT Id FROM @userIds);
                DELETE FROM [AspNetUsers] WHERE [Id] IN (SELECT Id FROM @userIds);
                
                COMMIT TRANSACTION;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Cannot reverse user deletion
        }
    }
}
