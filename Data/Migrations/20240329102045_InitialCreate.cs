using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EBrief.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CourtLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourtLists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DefendantModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefendantModel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InformationModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InformationModel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CaseFiles",
                columns: table => new
                {
                    CaseFileNumber = table.Column<string>(type: "TEXT", nullable: false),
                    DefendantId = table.Column<int>(type: "INTEGER", nullable: false),
                    CourtFileNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    FactsOfCharge = table.Column<string>(type: "TEXT", nullable: false),
                    InformationId = table.Column<int>(type: "INTEGER", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: false),
                    CourtListModelId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseFiles", x => x.CaseFileNumber);
                    table.ForeignKey(
                        name: "FK_CaseFiles_CourtLists_CourtListModelId",
                        column: x => x.CourtListModelId,
                        principalTable: "CourtLists",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CaseFiles_DefendantModel_DefendantId",
                        column: x => x.DefendantId,
                        principalTable: "DefendantModel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CaseFiles_InformationModel_InformationId",
                        column: x => x.InformationId,
                        principalTable: "InformationModel",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "InformationEntryModel",
                columns: table => new
                {
                    InformationModelId = table.Column<int>(type: "INTEGER", nullable: false),
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Sequence = table.Column<int>(type: "INTEGER", nullable: false),
                    Text = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InformationEntryModel", x => new { x.InformationModelId, x.Id });
                    table.ForeignKey(
                        name: "FK_InformationEntryModel_InformationModel_InformationModelId",
                        column: x => x.InformationModelId,
                        principalTable: "InformationModel",
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
                    CaseFileModelCaseFileNumber = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseFileDocumentModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseFileDocumentModel_CaseFiles_CaseFileModelCaseFileNumber",
                        column: x => x.CaseFileModelCaseFileNumber,
                        principalTable: "CaseFiles",
                        principalColumn: "CaseFileNumber");
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
                    CaseFileModelCaseFileNumber = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseFileEnquiryLogModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseFileEnquiryLogModel_CaseFiles_CaseFileModelCaseFileNumber",
                        column: x => x.CaseFileModelCaseFileNumber,
                        principalTable: "CaseFiles",
                        principalColumn: "CaseFileNumber");
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
                    CaseFileModelCaseFileNumber = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChargeModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChargeModel_CaseFiles_CaseFileModelCaseFileNumber",
                        column: x => x.CaseFileModelCaseFileNumber,
                        principalTable: "CaseFiles",
                        principalColumn: "CaseFileNumber");
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
                    CaseFileModelCaseFileNumber = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HearingEntryModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HearingEntryModel_CaseFiles_CaseFileModelCaseFileNumber",
                        column: x => x.CaseFileModelCaseFileNumber,
                        principalTable: "CaseFiles",
                        principalColumn: "CaseFileNumber");
                });

            migrationBuilder.CreateTable(
                name: "OccurrenceDocumentModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    CaseFileModelCaseFileNumber = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OccurrenceDocumentModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OccurrenceDocumentModel_CaseFiles_CaseFileModelCaseFileNumber",
                        column: x => x.CaseFileModelCaseFileNumber,
                        principalTable: "CaseFiles",
                        principalColumn: "CaseFileNumber");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaseFileDocumentModel_CaseFileModelCaseFileNumber",
                table: "CaseFileDocumentModel",
                column: "CaseFileModelCaseFileNumber");

            migrationBuilder.CreateIndex(
                name: "IX_CaseFileEnquiryLogModel_CaseFileModelCaseFileNumber",
                table: "CaseFileEnquiryLogModel",
                column: "CaseFileModelCaseFileNumber");

            migrationBuilder.CreateIndex(
                name: "IX_CaseFiles_CourtListModelId",
                table: "CaseFiles",
                column: "CourtListModelId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseFiles_DefendantId",
                table: "CaseFiles",
                column: "DefendantId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseFiles_InformationId",
                table: "CaseFiles",
                column: "InformationId");

            migrationBuilder.CreateIndex(
                name: "IX_ChargeModel_CaseFileModelCaseFileNumber",
                table: "ChargeModel",
                column: "CaseFileModelCaseFileNumber");

            migrationBuilder.CreateIndex(
                name: "IX_HearingEntryModel_CaseFileModelCaseFileNumber",
                table: "HearingEntryModel",
                column: "CaseFileModelCaseFileNumber");

            migrationBuilder.CreateIndex(
                name: "IX_OccurrenceDocumentModel_CaseFileModelCaseFileNumber",
                table: "OccurrenceDocumentModel",
                column: "CaseFileModelCaseFileNumber");
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
                name: "InformationEntryModel");

            migrationBuilder.DropTable(
                name: "OccurrenceDocumentModel");

            migrationBuilder.DropTable(
                name: "CaseFiles");

            migrationBuilder.DropTable(
                name: "CourtLists");

            migrationBuilder.DropTable(
                name: "DefendantModel");

            migrationBuilder.DropTable(
                name: "InformationModel");
        }
    }
}
