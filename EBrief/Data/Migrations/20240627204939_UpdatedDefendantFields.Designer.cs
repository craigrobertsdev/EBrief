﻿// <auto-generated />
using System;
using EBrief.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace EBrief.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20240627204939_UpdatedDefendantFields")]
    partial class UpdatedDefendantFields
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.6");

            modelBuilder.Entity("EBrief.Models.Data.BailAgreementModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("DateEnteredInto")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("DefendantModelDbKey")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("DefendantModelDbKey");

                    b.ToTable("BailAgreementModel");
                });

            modelBuilder.Entity("EBrief.Models.Data.CaseFileDocumentModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<Guid?>("CaseFileModelId")
                        .HasColumnType("TEXT");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CaseFileModelId");

                    b.ToTable("CaseFileDocumentModel");
                });

            modelBuilder.Entity("EBrief.Models.Data.CaseFileEnquiryLogModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<Guid?>("CaseFileModelId")
                        .HasColumnType("TEXT");

                    b.Property<string>("EnteredBy")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("EntryDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("EntryText")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CaseFileModelId");

                    b.ToTable("CaseFileEnquiryLogModel");
                });

            modelBuilder.Entity("EBrief.Models.Data.CaseFileModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("CaseFileNumber")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("CourtFileNumber")
                        .HasMaxLength(20)
                        .HasColumnType("TEXT");

                    b.Property<Guid>("CourtListId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("DefendantDbKey")
                        .HasColumnType("TEXT");

                    b.Property<string>("FactsOfCharge")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Notes")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CourtListId");

                    b.HasIndex("DefendantDbKey");

                    b.ToTable("CaseFiles");
                });

            modelBuilder.Entity("EBrief.Models.Data.ChargeModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<Guid?>("CaseFileModelId")
                        .HasColumnType("TEXT");

                    b.Property<string>("ChargeWording")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Date")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Sequence")
                        .HasColumnType("INTEGER");

                    b.Property<string>("VictimName")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CaseFileModelId");

                    b.ToTable("ChargeModel");
                });

            modelBuilder.Entity("EBrief.Models.Data.CourtListModel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<int>("CourtCode")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CourtDate")
                        .HasColumnType("TEXT");

                    b.Property<int>("CourtRoom")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("CourtLists");
                });

            modelBuilder.Entity("EBrief.Models.Data.DefendantModel", b =>
                {
                    b.Property<Guid>("DbKey")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Address")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DateOfBirth")
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .HasColumnType("TEXT");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Id")
                        .HasColumnType("INTEGER");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("OffenderHistory")
                        .HasColumnType("TEXT");

                    b.Property<string>("Phone")
                        .HasColumnType("TEXT");

                    b.HasKey("DbKey");

                    b.ToTable("DefendantModel");
                });

            modelBuilder.Entity("EBrief.Models.Data.HearingEntryModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AppearanceType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("CaseFileModelId")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("HearingDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Notes")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CaseFileModelId");

                    b.ToTable("HearingEntryModel");
                });

            modelBuilder.Entity("EBrief.Models.Data.InterventionOrderModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("DateIssued")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("DefendantModelDbKey")
                        .HasColumnType("TEXT");

                    b.Property<string>("OrderNumber")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("ProtectedPersonId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("DefendantModelDbKey");

                    b.HasIndex("ProtectedPersonId");

                    b.ToTable("InterventionOrderModel");
                });

            modelBuilder.Entity("EBrief.Models.Data.OccurrenceDocumentModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<Guid?>("CaseFileModelId")
                        .HasColumnType("TEXT");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CaseFileModelId");

                    b.ToTable("OccurrenceDocumentModel");
                });

            modelBuilder.Entity("EBrief.Models.Data.OrderConditionModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("BailAgreementModelId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Condition")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int?>("InterventionOrderModelId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Number")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("BailAgreementModelId");

                    b.HasIndex("InterventionOrderModelId");

                    b.ToTable("OrderConditionModel");
                });

            modelBuilder.Entity("EBrief.Models.Data.PersonModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("DateOfBirth")
                        .HasColumnType("TEXT");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("PersonModel");
                });

            modelBuilder.Entity("EBrief.Models.Data.BailAgreementModel", b =>
                {
                    b.HasOne("EBrief.Models.Data.DefendantModel", null)
                        .WithMany("BailAgreements")
                        .HasForeignKey("DefendantModelDbKey");
                });

            modelBuilder.Entity("EBrief.Models.Data.CaseFileDocumentModel", b =>
                {
                    b.HasOne("EBrief.Models.Data.CaseFileModel", null)
                        .WithMany("CaseFileDocuments")
                        .HasForeignKey("CaseFileModelId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("EBrief.Models.Data.CaseFileEnquiryLogModel", b =>
                {
                    b.HasOne("EBrief.Models.Data.CaseFileModel", null)
                        .WithMany("CfelEntries")
                        .HasForeignKey("CaseFileModelId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("EBrief.Models.Data.CaseFileModel", b =>
                {
                    b.HasOne("EBrief.Models.Data.CourtListModel", "CourtList")
                        .WithMany("CaseFiles")
                        .HasForeignKey("CourtListId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("EBrief.Models.Data.DefendantModel", "Defendant")
                        .WithMany()
                        .HasForeignKey("DefendantDbKey")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CourtList");

                    b.Navigation("Defendant");
                });

            modelBuilder.Entity("EBrief.Models.Data.ChargeModel", b =>
                {
                    b.HasOne("EBrief.Models.Data.CaseFileModel", null)
                        .WithMany("Charges")
                        .HasForeignKey("CaseFileModelId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("EBrief.Models.Data.HearingEntryModel", b =>
                {
                    b.HasOne("EBrief.Models.Data.CaseFileModel", null)
                        .WithMany("Schedule")
                        .HasForeignKey("CaseFileModelId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("EBrief.Models.Data.InterventionOrderModel", b =>
                {
                    b.HasOne("EBrief.Models.Data.DefendantModel", null)
                        .WithMany("InterventionOrders")
                        .HasForeignKey("DefendantModelDbKey");

                    b.HasOne("EBrief.Models.Data.PersonModel", "ProtectedPerson")
                        .WithMany()
                        .HasForeignKey("ProtectedPersonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ProtectedPerson");
                });

            modelBuilder.Entity("EBrief.Models.Data.OccurrenceDocumentModel", b =>
                {
                    b.HasOne("EBrief.Models.Data.CaseFileModel", null)
                        .WithMany("OccurrenceDocuments")
                        .HasForeignKey("CaseFileModelId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("EBrief.Models.Data.OrderConditionModel", b =>
                {
                    b.HasOne("EBrief.Models.Data.BailAgreementModel", null)
                        .WithMany("Conditions")
                        .HasForeignKey("BailAgreementModelId");

                    b.HasOne("EBrief.Models.Data.InterventionOrderModel", null)
                        .WithMany("Conditions")
                        .HasForeignKey("InterventionOrderModelId");
                });

            modelBuilder.Entity("EBrief.Models.Data.BailAgreementModel", b =>
                {
                    b.Navigation("Conditions");
                });

            modelBuilder.Entity("EBrief.Models.Data.CaseFileModel", b =>
                {
                    b.Navigation("CaseFileDocuments");

                    b.Navigation("CfelEntries");

                    b.Navigation("Charges");

                    b.Navigation("OccurrenceDocuments");

                    b.Navigation("Schedule");
                });

            modelBuilder.Entity("EBrief.Models.Data.CourtListModel", b =>
                {
                    b.Navigation("CaseFiles");
                });

            modelBuilder.Entity("EBrief.Models.Data.DefendantModel", b =>
                {
                    b.Navigation("BailAgreements");

                    b.Navigation("InterventionOrders");
                });

            modelBuilder.Entity("EBrief.Models.Data.InterventionOrderModel", b =>
                {
                    b.Navigation("Conditions");
                });
#pragma warning restore 612, 618
        }
    }
}
