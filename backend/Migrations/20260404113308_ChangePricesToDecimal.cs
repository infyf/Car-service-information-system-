using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoServ.Migrations
{
    /// <inheritdoc />
    public partial class ChangePricesToDecimal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceDetails_Services_ServiceId",
                table: "ServiceDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceItems_Services_ServiceId",
                table: "ServiceItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Services",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Services_Slug",
                table: "Services");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ServiceItems",
                table: "ServiceItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ServiceDetails",
                table: "ServiceDetails");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "users");

            migrationBuilder.RenameTable(
                name: "Services",
                newName: "services");

            migrationBuilder.RenameTable(
                name: "ServiceItems",
                newName: "serviceitems");

            migrationBuilder.RenameTable(
                name: "ServiceDetails",
                newName: "servicedetails");

            migrationBuilder.RenameIndex(
                name: "IX_ServiceItems_ServiceId",
                table: "serviceitems",
                newName: "IX_serviceitems_ServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_ServiceDetails_ServiceId",
                table: "servicedetails",
                newName: "IX_servicedetails_ServiceId");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "users",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "users",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "services",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<decimal>(
                name: "PriceFrom",
                table: "serviceitems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "DurationMinutes",
                table: "serviceitems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<decimal>(
                name: "PriceFrom",
                table: "servicedetails",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddPrimaryKey(
                name: "PK_users",
                table: "users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_services",
                table: "services",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_serviceitems",
                table: "serviceitems",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_servicedetails",
                table: "servicedetails",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "masters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FirstName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Phone = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ExperienceYears = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_masters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_masters_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user_profiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AvatarUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CarBrand = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CarModel = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CarYear = table.Column<int>(type: "int", nullable: true),
                    CarEngine = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CarPlate = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CarImageUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_profiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_profiles_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "bookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    MasterId = table.Column<int>(type: "int", nullable: true),
                    BookingDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    BookingTime = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    CustomerName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustomerPhone = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Comment = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CarPlate = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BookingType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParentBookingId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_bookings_bookings_ParentBookingId",
                        column: x => x.ParentBookingId,
                        principalTable: "bookings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_bookings_masters_MasterId",
                        column: x => x.MasterId,
                        principalTable: "masters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_bookings_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "consultations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    master_id = table.Column<int>(type: "int", nullable: true),
                    problem_description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    consultation_date = table.Column<DateOnly>(type: "date", nullable: false),
                    consultation_time = table.Column<TimeOnly>(type: "time(6)", nullable: false),
                    status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_consultations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_consultations_masters_master_id",
                        column: x => x.master_id,
                        principalTable: "masters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_consultations_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "masterspecializations",
                columns: table => new
                {
                    MasterId = table.Column<int>(type: "int", nullable: false),
                    ServiceItemId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_masterspecializations", x => new { x.MasterId, x.ServiceItemId });
                    table.ForeignKey(
                        name: "FK_masterspecializations_masters_MasterId",
                        column: x => x.MasterId,
                        principalTable: "masters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_masterspecializations_serviceitems_ServiceItemId",
                        column: x => x.ServiceItemId,
                        principalTable: "serviceitems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "booking_services",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    ServiceItemId = table.Column<int>(type: "int", nullable: false),
                    PriceAtBooking = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_booking_services", x => x.Id);
                    table.ForeignKey(
                        name: "FK_booking_services_bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_booking_services_serviceitems_ServiceItemId",
                        column: x => x.ServiceItemId,
                        principalTable: "serviceitems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "recommendations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BookingId = table.Column<int>(type: "int", nullable: false),
                    ServiceItemId = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsAccepted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedBookingId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recommendations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_recommendations_bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_recommendations_serviceitems_ServiceItemId",
                        column: x => x.ServiceItemId,
                        principalTable: "serviceitems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_booking_services_BookingId",
                table: "booking_services",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_booking_services_ServiceItemId",
                table: "booking_services",
                column: "ServiceItemId");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_MasterId",
                table: "bookings",
                column: "MasterId");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_ParentBookingId",
                table: "bookings",
                column: "ParentBookingId");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_UserId",
                table: "bookings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_consultations_master_id",
                table: "consultations",
                column: "master_id");

            migrationBuilder.CreateIndex(
                name: "IX_consultations_user_id",
                table: "consultations",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_masters_user_id",
                table: "masters",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_masterspecializations_ServiceItemId",
                table: "masterspecializations",
                column: "ServiceItemId");

            migrationBuilder.CreateIndex(
                name: "IX_recommendations_BookingId",
                table: "recommendations",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_recommendations_ServiceItemId",
                table: "recommendations",
                column: "ServiceItemId");

            migrationBuilder.CreateIndex(
                name: "IX_user_profiles_UserId",
                table: "user_profiles",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_servicedetails_services_ServiceId",
                table: "servicedetails",
                column: "ServiceId",
                principalTable: "services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_serviceitems_services_ServiceId",
                table: "serviceitems",
                column: "ServiceId",
                principalTable: "services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_servicedetails_services_ServiceId",
                table: "servicedetails");

            migrationBuilder.DropForeignKey(
                name: "FK_serviceitems_services_ServiceId",
                table: "serviceitems");

            migrationBuilder.DropTable(
                name: "booking_services");

            migrationBuilder.DropTable(
                name: "consultations");

            migrationBuilder.DropTable(
                name: "masterspecializations");

            migrationBuilder.DropTable(
                name: "recommendations");

            migrationBuilder.DropTable(
                name: "user_profiles");

            migrationBuilder.DropTable(
                name: "bookings");

            migrationBuilder.DropTable(
                name: "masters");

            migrationBuilder.DropPrimaryKey(
                name: "PK_users",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_services",
                table: "services");

            migrationBuilder.DropPrimaryKey(
                name: "PK_serviceitems",
                table: "serviceitems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_servicedetails",
                table: "servicedetails");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "users");

            migrationBuilder.DropColumn(
                name: "DurationMinutes",
                table: "serviceitems");

            migrationBuilder.RenameTable(
                name: "users",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "services",
                newName: "Services");

            migrationBuilder.RenameTable(
                name: "serviceitems",
                newName: "ServiceItems");

            migrationBuilder.RenameTable(
                name: "servicedetails",
                newName: "ServiceDetails");

            migrationBuilder.RenameIndex(
                name: "IX_serviceitems_ServiceId",
                table: "ServiceItems",
                newName: "IX_ServiceItems_ServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_servicedetails_ServiceId",
                table: "ServiceDetails",
                newName: "IX_ServiceDetails_ServiceId");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Slug",
                table: "Services",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "PriceFrom",
                table: "ServiceItems",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<int>(
                name: "PriceFrom",
                table: "ServiceDetails",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Services",
                table: "Services",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServiceItems",
                table: "ServiceItems",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServiceDetails",
                table: "ServiceDetails",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Services_Slug",
                table: "Services",
                column: "Slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceDetails_Services_ServiceId",
                table: "ServiceDetails",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceItems_Services_ServiceId",
                table: "ServiceItems",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
