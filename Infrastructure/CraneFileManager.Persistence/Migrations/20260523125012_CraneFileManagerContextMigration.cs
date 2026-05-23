using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CraneFileManager.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CraneFileManagerContextMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileType",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValueSql: "(NULL)"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(NULL)"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(NULL)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FileType__3214EC07F9EC5D14", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValueSql: "(NULL)"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValueSql: "(NULL)"),
                    NotificationDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(NULL)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Notifica__3214EC07600A5E25", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValueSql: "(NULL)"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(NULL)"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(NULL)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Role__3214EC078D6B7E10", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolePermission",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    Method = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValueSql: "(NULL)"),
                    MethodDescription = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValueSql: "(NULL)"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(NULL)"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(NULL)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__RolePerm__3214EC07CC5B73BC", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValueSql: "(NULL)"),
                    Surname = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValueSql: "(NULL)"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValueSql: "(NULL)"),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValueSql: "(NULL)"),
                    IsBlcok = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "(NULL)"),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "(NULL)"),
                    ConfirmPassword = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValueSql: "(NULL)"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValueSql: "(NULL)"),
                    ProfileImage = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValueSql: "(NULL)"),
                    Birthday = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(NULL)"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(NULL)"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(NULL)"),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValueSql: "(NULL)"),
                    RefreshTokenExpiryTime = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(NULL)"),
                    SecretKey = table.Column<string>(type: "nvarchar(max)", nullable: true, defaultValueSql: "(NULL)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__User__3214EC07150C006D", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserPermission",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    UserAccess = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValueSql: "(NULL)"),
                    UserAccessDescription = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValueSql: "(NULL)"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(NULL)"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(NULL)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserPerm__3214EC070D532386", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "File",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    OrginalName = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValueSql: "(NULL)"),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValueSql: "(NULL)"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValueSql: "(NULL)"),
                    Size = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValueSql: "(NULL)"),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValueSql: "(NULL)"),
                    isRemove = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "(NULL)"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(NULL)"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(NULL)"),
                    FileTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__File__3214EC074F07067A", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileTypeId_forFile",
                        column: x => x.FileTypeId,
                        principalTable: "FileType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RoleClaim",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(NULL)"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(NULL)"),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RolePermissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__RoleClai__3214EC0728C9B922", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleId_forRoleClaim",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RolePermissionId_forRoleClaim",
                        column: x => x.RolePermissionId,
                        principalTable: "RolePermission",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserNotification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(NULL)"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(NULL)"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NotificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserNoti__3214EC070206A0D0", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationId_forUserNotification",
                        column: x => x.NotificationId,
                        principalTable: "Notification",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserId_forUserNotification",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserRole",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(NULL)"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(NULL)"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserRole__3214EC079612DB4B", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleId_forUserRole",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserId_forUserRole",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserClaim",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(NULL)"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(NULL)"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserPermitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserClai__3214EC07A38C720D", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserId_forUserClaim",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserPermitionId_forUserClaim",
                        column: x => x.UserPermitionId,
                        principalTable: "UserPermission",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FileShare",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(NULL)"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(NULL)"),
                    FileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FileShar__3214EC075BDEB216", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileId_forFileShare",
                        column: x => x.FileId,
                        principalTable: "File",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserId_forFileShare",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FileTrashCan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    ThrowTrashDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(NULL)"),
                    TakeofTrashDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(NULL)"),
                    FileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FileTras__3214EC07D403E5D7", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileId_forFileTrashCan",
                        column: x => x.FileId,
                        principalTable: "File",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserFile",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(NULL)"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "(NULL)"),
                    FileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__UserFile__3214EC076B9AF94C", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileId_forUseFile",
                        column: x => x.FileId,
                        principalTable: "File",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserId_forUseFile",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_File_FileTypeId",
                table: "File",
                column: "FileTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_FileShare_FileId",
                table: "FileShare",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_FileShare_UserId",
                table: "FileShare",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FileTrashCan_FileId",
                table: "FileTrashCan",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaim_RoleId",
                table: "RoleClaim",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaim_RolePermissionId",
                table: "RoleClaim",
                column: "RolePermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserClaim_UserId",
                table: "UserClaim",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserClaim_UserPermitionId",
                table: "UserClaim",
                column: "UserPermitionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFile_FileId",
                table: "UserFile",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFile_UserId",
                table: "UserFile",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotification_NotificationId",
                table: "UserNotification",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotification_UserId",
                table: "UserNotification",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_RoleId",
                table: "UserRole",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRole_UserId",
                table: "UserRole",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileShare");

            migrationBuilder.DropTable(
                name: "FileTrashCan");

            migrationBuilder.DropTable(
                name: "RoleClaim");

            migrationBuilder.DropTable(
                name: "UserClaim");

            migrationBuilder.DropTable(
                name: "UserFile");

            migrationBuilder.DropTable(
                name: "UserNotification");

            migrationBuilder.DropTable(
                name: "UserRole");

            migrationBuilder.DropTable(
                name: "RolePermission");

            migrationBuilder.DropTable(
                name: "UserPermission");

            migrationBuilder.DropTable(
                name: "File");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "FileType");
        }
    }
}
