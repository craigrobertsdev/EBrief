using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EBrief.Data.Migrations
{
    /// <inheritdoc />
    public partial class MoveToGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CourtLists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CourtDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CourtCode = table.Column<int>(type: "INTEGER", nullable: false),
                    CourtRoom = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourtLists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DefendantModel",
                columns: table => new
                {
                    DbKey = table.Column<Guid>(type: "TEXT", nullable: false),
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    OffenderHistory = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefendantModel", x => x.DbKey);
                });

            migrationBuilder.CreateTable(
                name: "PersonModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonModel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BailAgreementModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DateEnteredInto = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DefendantModelDbKey = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BailAgreementModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BailAgreementModel_DefendantModel_DefendantModelDbKey",
                        column: x => x.DefendantModelDbKey,
                        principalTable: "DefendantModel",
                        principalColumn: "DbKey");
                });

            migrationBuilder.CreateTable(
                name: "CaseFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CourtListId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CaseFileNumber = table.Column<string>(type: "TEXT", nullable: false),
                    DefendantDbKey = table.Column<Guid>(type: "TEXT", nullable: false),
                    CourtFileNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    FactsOfCharge = table.Column<string>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseFiles_CourtLists_CourtListId",
                        column: x => x.CourtListId,
                        principalTable: "CourtLists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CaseFiles_DefendantModel_DefendantDbKey",
                        column: x => x.DefendantDbKey,
                        principalTable: "DefendantModel",
                        principalColumn: "DbKey",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InterventionOrderModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrderNumber = table.Column<string>(type: "TEXT", nullable: false),
                    ProtectedPersonId = table.Column<int>(type: "INTEGER", nullable: false),
                    DateIssued = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DefendantModelDbKey = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterventionOrderModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterventionOrderModel_DefendantModel_DefendantModelDbKey",
                        column: x => x.DefendantModelDbKey,
                        principalTable: "DefendantModel",
                        principalColumn: "DbKey");
                    table.ForeignKey(
                        name: "FK_InterventionOrderModel_PersonModel_ProtectedPersonId",
                        column: x => x.ProtectedPersonId,
                        principalTable: "PersonModel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CaseFileDocumentModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    CaseFileModelId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseFileDocumentModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseFileDocumentModel_CaseFiles_CaseFileModelId",
                        column: x => x.CaseFileModelId,
                        principalTable: "CaseFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CaseFileEnquiryLogModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EntryText = table.Column<string>(type: "TEXT", nullable: false),
                    EnteredBy = table.Column<string>(type: "TEXT", nullable: false),
                    EntryDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CaseFileModelId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseFileEnquiryLogModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseFileEnquiryLogModel_CaseFiles_CaseFileModelId",
                        column: x => x.CaseFileModelId,
                        principalTable: "CaseFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChargeModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Sequence = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    VictimName = table.Column<string>(type: "TEXT", nullable: true),
                    ChargeWording = table.Column<string>(type: "TEXT", nullable: false),
                    CaseFileModelId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChargeModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChargeModel_CaseFiles_CaseFileModelId",
                        column: x => x.CaseFileModelId,
                        principalTable: "CaseFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HearingEntryModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    HearingDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AppearanceType = table.Column<string>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: false),
                    CaseFileModelId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HearingEntryModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HearingEntryModel_CaseFiles_CaseFileModelId",
                        column: x => x.CaseFileModelId,
                        principalTable: "CaseFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OccurrenceDocumentModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    CaseFileModelId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OccurrenceDocumentModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OccurrenceDocumentModel_CaseFiles_CaseFileModelId",
                        column: x => x.CaseFileModelId,
                        principalTable: "CaseFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderConditionModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Number = table.Column<int>(type: "INTEGER", nullable: false),
                    Condition = table.Column<string>(type: "TEXT", nullable: false),
                    BailAgreementModelId = table.Column<int>(type: "INTEGER", nullable: true),
                    InterventionOrderModelId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderConditionModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderConditionModel_BailAgreementModel_BailAgreementModelId",
                        column: x => x.BailAgreementModelId,
                        principalTable: "BailAgreementModel",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrderConditionModel_InterventionOrderModel_InterventionOrderModelId",
                        column: x => x.InterventionOrderModelId,
                        principalTable: "InterventionOrderModel",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BailAgreementModel_DefendantModelDbKey",
                table: "BailAgreementModel",
                column: "DefendantModelDbKey");

            migrationBuilder.CreateIndex(
                name: "IX_CaseFileDocumentModel_CaseFileModelId",
                table: "CaseFileDocumentModel",
                column: "CaseFileModelId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseFileEnquiryLogModel_CaseFileModelId",
                table: "CaseFileEnquiryLogModel",
                column: "CaseFileModelId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseFiles_CourtListId",
                table: "CaseFiles",
                column: "CourtListId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseFiles_DefendantDbKey",
                table: "CaseFiles",
                column: "DefendantDbKey");

            migrationBuilder.CreateIndex(
                name: "IX_ChargeModel_CaseFileModelId",
                table: "ChargeModel",
                column: "CaseFileModelId");

            migrationBuilder.CreateIndex(
                name: "IX_HearingEntryModel_CaseFileModelId",
                table: "HearingEntryModel",
                column: "CaseFileModelId");

            migrationBuilder.CreateIndex(
                name: "IX_InterventionOrderModel_DefendantModelDbKey",
                table: "InterventionOrderModel",
                column: "DefendantModelDbKey");

            migrationBuilder.CreateIndex(
                name: "IX_InterventionOrderModel_ProtectedPersonId",
                table: "InterventionOrderModel",
                column: "ProtectedPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_OccurrenceDocumentModel_CaseFileModelId",
                table: "OccurrenceDocumentModel",
                column: "CaseFileModelId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderConditionModel_BailAgreementModelId",
                table: "OrderConditionModel",
                column: "BailAgreementModelId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderConditionModel_InterventionOrderModelId",
                table: "OrderConditionModel",
                column: "InterventionOrderModelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaseFileDocumentModel");

            migrationBuilder.DropTable(
                name: "CaseFileEnquiryLogModel");

            migrationBuilder.DropTable(
                name: "ChargeModel");

            migrationBuilder.DropTable(
                name: "HearingEntryModel");

            migrationBuilder.DropTable(
                name: "OccurrenceDocumentModel");

            migrationBuilder.DropTable(
                name: "OrderConditionModel");

            migrationBuilder.DropTable(
                name: "CaseFiles");

            migrationBuilder.DropTable(
                name: "BailAgreementModel");

            migrationBuilder.DropTable(
                name: "InterventionOrderModel");

            migrationBuilder.DropTable(
                name: "CourtLists");

            migrationBuilder.DropTable(
                name: "DefendantModel");

            migrationBuilder.DropTable(
                name: "PersonModel");
        }
    }
}
