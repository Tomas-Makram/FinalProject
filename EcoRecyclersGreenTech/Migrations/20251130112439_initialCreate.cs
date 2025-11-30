using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoRecyclersGreenTech.Migrations
{
    /// <inheritdoc />
    public partial class initialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    AdminID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdminType = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.AdminID);
                });

            migrationBuilder.CreateTable(
                name: "Craftsmen",
                columns: table => new
                {
                    CraftsmanID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SkillType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExperienceYears = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Craftsmen", x => x.CraftsmanID);
                });

            migrationBuilder.CreateTable(
                name: "Factories",
                columns: table => new
                {
                    FactoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FactoryName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FactoryImgURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FactoryType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Factories", x => x.FactoryID);
                });

            migrationBuilder.CreateTable(
                name: "Individuals",
                columns: table => new
                {
                    IndividualID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Occupation = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Individuals", x => x.IndividualID);
                });

            migrationBuilder.CreateTable(
                name: "UserTypes",
                columns: table => new
                {
                    TypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RealTypeID = table.Column<int>(type: "int", nullable: true),
                    TypeName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTypes", x => x.TypeID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserProfieImgURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    phoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HashPassword = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Locaton = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserTypeID = table.Column<int>(type: "int", nullable: false),
                    JoinDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Verified = table.Column<bool>(type: "bit", nullable: false),
                    Blocked = table.Column<bool>(type: "bit", nullable: false),
                    FailedLoginAttempts = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                    table.ForeignKey(
                        name: "FK_Users_UserTypes_UserTypeID",
                        column: x => x.UserTypeID,
                        principalTable: "UserTypes",
                        principalColumn: "TypeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuctionStores",
                columns: table => new
                {
                    AuctionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SellerID = table.Column<int>(type: "int", nullable: false),
                    ProductType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductImgURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    StartPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AuctionDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuctionStores", x => x.AuctionID);
                    table.ForeignKey(
                        name: "FK_AuctionStores_Users_SellerID",
                        column: x => x.SellerID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChatUsers",
                columns: table => new
                {
                    ChatUserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    To = table.Column<int>(type: "int", nullable: false),
                    From = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatUsers", x => x.ChatUserID);
                    table.ForeignKey(
                        name: "FK_ChatUsers_Users_From",
                        column: x => x.From,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatUsers_Users_To",
                        column: x => x.To,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Complaints",
                columns: table => new
                {
                    ComplaintID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    From = table.Column<int>(type: "int", nullable: false),
                    StatusCounter = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Complaints", x => x.ComplaintID);
                    table.ForeignKey(
                        name: "FK_Complaints_Users_From",
                        column: x => x.From,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Feedbacks",
                columns: table => new
                {
                    FeedbackID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    To = table.Column<int>(type: "int", nullable: false),
                    From = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedbacks", x => x.FeedbackID);
                    table.ForeignKey(
                        name: "FK_Feedbacks_Users_From",
                        column: x => x.From,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Feedbacks_Users_To",
                        column: x => x.To,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobStores",
                columns: table => new
                {
                    JobID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostedBy = table.Column<int>(type: "int", nullable: false),
                    JobType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WorkHours = table.Column<int>(type: "int", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Salary = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobStores", x => x.JobID);
                    table.ForeignKey(
                        name: "FK_JobStores_Users_PostedBy",
                        column: x => x.PostedBy,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MachineStores",
                columns: table => new
                {
                    MachineID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SellerID = table.Column<int>(type: "int", nullable: false),
                    MachineType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MachineImgURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ManufactureDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Condition = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineStores", x => x.MachineID);
                    table.ForeignKey(
                        name: "FK_MachineStores_Users_SellerID",
                        column: x => x.SellerID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MaterialStores",
                columns: table => new
                {
                    MaterialID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SellerID = table.Column<int>(type: "int", nullable: false),
                    ProductType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProductImgURL = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialStores", x => x.MaterialID);
                    table.ForeignKey(
                        name: "FK_MaterialStores_Users_SellerID",
                        column: x => x.SellerID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RentalStores",
                columns: table => new
                {
                    RentalID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerID = table.Column<int>(type: "int", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Area = table.Column<double>(type: "float", nullable: false),
                    PricePerMonth = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AvailableFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Condition = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RentalStores", x => x.RentalID);
                    table.ForeignKey(
                        name: "FK_RentalStores_Users_OwnerID",
                        column: x => x.OwnerID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuctionOrders",
                columns: table => new
                {
                    AuctionOrderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuctionStoreID = table.Column<int>(type: "int", nullable: false),
                    WinnerID = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuctionOrders", x => x.AuctionOrderID);
                    table.ForeignKey(
                        name: "FK_AuctionOrders_AuctionStores_AuctionStoreID",
                        column: x => x.AuctionStoreID,
                        principalTable: "AuctionStores",
                        principalColumn: "AuctionID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuctionOrders_Users_WinnerID",
                        column: x => x.WinnerID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JobOrders",
                columns: table => new
                {
                    JobOrderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobStoreID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Meetingdate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PickupLocation = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobOrders", x => x.JobOrderID);
                    table.ForeignKey(
                        name: "FK_JobOrders_JobStores_JobStoreID",
                        column: x => x.JobStoreID,
                        principalTable: "JobStores",
                        principalColumn: "JobID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JobOrders_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MachineOrders",
                columns: table => new
                {
                    MachineOrderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MachineStoreID = table.Column<int>(type: "int", nullable: false),
                    BuyerID = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ArrivalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PickupLocation = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MachineOrders", x => x.MachineOrderID);
                    table.ForeignKey(
                        name: "FK_MachineOrders_MachineStores_MachineStoreID",
                        column: x => x.MachineStoreID,
                        principalTable: "MachineStores",
                        principalColumn: "MachineID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MachineOrders_Users_BuyerID",
                        column: x => x.BuyerID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MaterialOrders",
                columns: table => new
                {
                    MaterialOrderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaterialStoreID = table.Column<int>(type: "int", nullable: false),
                    BuyerID = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ArrivalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PickupLocation = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialOrders", x => x.MaterialOrderID);
                    table.ForeignKey(
                        name: "FK_MaterialOrders_MaterialStores_MaterialStoreID",
                        column: x => x.MaterialStoreID,
                        principalTable: "MaterialStores",
                        principalColumn: "MaterialID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaterialOrders_Users_BuyerID",
                        column: x => x.BuyerID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RentalOrders",
                columns: table => new
                {
                    RentalOrderID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RentalStoreID = table.Column<int>(type: "int", nullable: false),
                    BuyerID = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RentalOrders", x => x.RentalOrderID);
                    table.ForeignKey(
                        name: "FK_RentalOrders_RentalStores_RentalStoreID",
                        column: x => x.RentalStoreID,
                        principalTable: "RentalStores",
                        principalColumn: "RentalID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RentalOrders_Users_BuyerID",
                        column: x => x.BuyerID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuctionOrders_AuctionStoreID",
                table: "AuctionOrders",
                column: "AuctionStoreID");

            migrationBuilder.CreateIndex(
                name: "IX_AuctionOrders_WinnerID",
                table: "AuctionOrders",
                column: "WinnerID");

            migrationBuilder.CreateIndex(
                name: "IX_AuctionStores_SellerID",
                table: "AuctionStores",
                column: "SellerID");

            migrationBuilder.CreateIndex(
                name: "IX_ChatUsers_From",
                table: "ChatUsers",
                column: "From");

            migrationBuilder.CreateIndex(
                name: "IX_ChatUsers_To",
                table: "ChatUsers",
                column: "To");

            migrationBuilder.CreateIndex(
                name: "IX_Complaints_From",
                table: "Complaints",
                column: "From");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_From",
                table: "Feedbacks",
                column: "From");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_To",
                table: "Feedbacks",
                column: "To");

            migrationBuilder.CreateIndex(
                name: "IX_JobOrders_JobStoreID",
                table: "JobOrders",
                column: "JobStoreID");

            migrationBuilder.CreateIndex(
                name: "IX_JobOrders_UserID",
                table: "JobOrders",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_JobStores_PostedBy",
                table: "JobStores",
                column: "PostedBy");

            migrationBuilder.CreateIndex(
                name: "IX_MachineOrders_BuyerID",
                table: "MachineOrders",
                column: "BuyerID");

            migrationBuilder.CreateIndex(
                name: "IX_MachineOrders_MachineStoreID",
                table: "MachineOrders",
                column: "MachineStoreID");

            migrationBuilder.CreateIndex(
                name: "IX_MachineStores_SellerID",
                table: "MachineStores",
                column: "SellerID");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialOrders_BuyerID",
                table: "MaterialOrders",
                column: "BuyerID");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialOrders_MaterialStoreID",
                table: "MaterialOrders",
                column: "MaterialStoreID");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialStores_SellerID",
                table: "MaterialStores",
                column: "SellerID");

            migrationBuilder.CreateIndex(
                name: "IX_RentalOrders_BuyerID",
                table: "RentalOrders",
                column: "BuyerID");

            migrationBuilder.CreateIndex(
                name: "IX_RentalOrders_RentalStoreID",
                table: "RentalOrders",
                column: "RentalStoreID");

            migrationBuilder.CreateIndex(
                name: "IX_RentalStores_OwnerID",
                table: "RentalStores",
                column: "OwnerID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserTypeID",
                table: "Users",
                column: "UserTypeID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.DropTable(
                name: "AuctionOrders");

            migrationBuilder.DropTable(
                name: "ChatUsers");

            migrationBuilder.DropTable(
                name: "Complaints");

            migrationBuilder.DropTable(
                name: "Craftsmen");

            migrationBuilder.DropTable(
                name: "Factories");

            migrationBuilder.DropTable(
                name: "Feedbacks");

            migrationBuilder.DropTable(
                name: "Individuals");

            migrationBuilder.DropTable(
                name: "JobOrders");

            migrationBuilder.DropTable(
                name: "MachineOrders");

            migrationBuilder.DropTable(
                name: "MaterialOrders");

            migrationBuilder.DropTable(
                name: "RentalOrders");

            migrationBuilder.DropTable(
                name: "AuctionStores");

            migrationBuilder.DropTable(
                name: "JobStores");

            migrationBuilder.DropTable(
                name: "MachineStores");

            migrationBuilder.DropTable(
                name: "MaterialStores");

            migrationBuilder.DropTable(
                name: "RentalStores");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "UserTypes");
        }
    }
}
