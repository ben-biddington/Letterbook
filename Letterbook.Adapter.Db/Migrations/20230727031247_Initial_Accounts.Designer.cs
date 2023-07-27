﻿// <auto-generated />
using System;
using Letterbook.Adapter.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Letterbook.Adapter.Db.Migrations
{
    [DbContext(typeof(TransactionalContext))]
    [Migration("20230727031247_Initial_Accounts")]
    partial class Initial_Accounts
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("AudienceProfile", b =>
                {
                    b.Property<string>("AudiencesId")
                        .HasColumnType("text");

                    b.Property<string>("MembersId")
                        .HasColumnType("text");

                    b.HasKey("AudiencesId", "MembersId");

                    b.HasIndex("MembersId");

                    b.ToTable("AudienceProfile");
                });

            modelBuilder.Entity("Letterbook.Core.Models.Account", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("Letterbook.Core.Models.Audience", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("ImageId")
                        .HasColumnType("text");

                    b.Property<string>("NoteId")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("ImageId");

                    b.HasIndex("NoteId");

                    b.ToTable("Audience");
                });

            modelBuilder.Entity("Letterbook.Core.Models.Image", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<DateTime>("Expiration")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("FileLocation")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("LocalId")
                        .HasColumnType("text");

                    b.Property<string>("MimeType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Images");
                });

            modelBuilder.Entity("Letterbook.Core.Models.LinkedProfile", b =>
                {
                    b.Property<string>("AccountId")
                        .HasColumnType("text");

                    b.Property<string>("ProfileId")
                        .HasColumnType("text");

                    b.Property<decimal>("Permission")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("AccountId", "ProfileId");

                    b.HasIndex("ProfileId");

                    b.ToTable("LinkedProfile");
                });

            modelBuilder.Entity("Letterbook.Core.Models.Mention", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("ImageId")
                        .HasColumnType("text");

                    b.Property<string>("NoteId")
                        .HasColumnType("text");

                    b.Property<string>("SubjectId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Visibility")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ImageId");

                    b.HasIndex("NoteId");

                    b.HasIndex("SubjectId");

                    b.ToTable("Mention");
                });

            modelBuilder.Entity("Letterbook.Core.Models.Note", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("Client")
                        .HasColumnType("text");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("InReplyToId")
                        .HasColumnType("text");

                    b.Property<string>("LocalId")
                        .HasColumnType("text");

                    b.Property<string>("Summary")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("InReplyToId");

                    b.ToTable("Notes");
                });

            modelBuilder.Entity("Letterbook.Core.Models.Profile", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("ImageId")
                        .HasColumnType("text");

                    b.Property<string>("LocalId")
                        .HasColumnType("text");

                    b.Property<string>("OwnedById")
                        .HasColumnType("text");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ImageId");

                    b.HasIndex("OwnedById");

                    b.ToTable("Profiles");
                });

            modelBuilder.Entity("NoteProfile", b =>
                {
                    b.Property<string>("CreatedNotesId")
                        .HasColumnType("text");

                    b.Property<string>("CreatorsId")
                        .HasColumnType("text");

                    b.HasKey("CreatedNotesId", "CreatorsId");

                    b.HasIndex("CreatorsId");

                    b.ToTable("NoteProfile");
                });

            modelBuilder.Entity("NoteProfile1", b =>
                {
                    b.Property<string>("LikedById")
                        .HasColumnType("text");

                    b.Property<string>("LikedNotesId")
                        .HasColumnType("text");

                    b.HasKey("LikedById", "LikedNotesId");

                    b.HasIndex("LikedNotesId");

                    b.ToTable("NoteProfile1");
                });

            modelBuilder.Entity("NoteProfile2", b =>
                {
                    b.Property<string>("BoostedById")
                        .HasColumnType("text");

                    b.Property<string>("BoostedNotesId")
                        .HasColumnType("text");

                    b.HasKey("BoostedById", "BoostedNotesId");

                    b.HasIndex("BoostedNotesId");

                    b.ToTable("NoteProfile2");
                });

            modelBuilder.Entity("AudienceProfile", b =>
                {
                    b.HasOne("Letterbook.Core.Models.Audience", null)
                        .WithMany()
                        .HasForeignKey("AudiencesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Letterbook.Core.Models.Profile", null)
                        .WithMany()
                        .HasForeignKey("MembersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Letterbook.Core.Models.Audience", b =>
                {
                    b.HasOne("Letterbook.Core.Models.Image", null)
                        .WithMany("Visibility")
                        .HasForeignKey("ImageId");

                    b.HasOne("Letterbook.Core.Models.Note", null)
                        .WithMany("Visibility")
                        .HasForeignKey("NoteId");
                });

            modelBuilder.Entity("Letterbook.Core.Models.LinkedProfile", b =>
                {
                    b.HasOne("Letterbook.Core.Models.Account", "Account")
                        .WithMany("LinkedProfiles")
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Letterbook.Core.Models.Profile", "Profile")
                        .WithMany("RelatedAccounts")
                        .HasForeignKey("ProfileId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");

                    b.Navigation("Profile");
                });

            modelBuilder.Entity("Letterbook.Core.Models.Mention", b =>
                {
                    b.HasOne("Letterbook.Core.Models.Image", null)
                        .WithMany("Mentions")
                        .HasForeignKey("ImageId");

                    b.HasOne("Letterbook.Core.Models.Note", null)
                        .WithMany("Mentions")
                        .HasForeignKey("NoteId");

                    b.HasOne("Letterbook.Core.Models.Profile", "Subject")
                        .WithMany()
                        .HasForeignKey("SubjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Subject");
                });

            modelBuilder.Entity("Letterbook.Core.Models.Note", b =>
                {
                    b.HasOne("Letterbook.Core.Models.Note", "InReplyTo")
                        .WithMany("Replies")
                        .HasForeignKey("InReplyToId");

                    b.Navigation("InReplyTo");
                });

            modelBuilder.Entity("Letterbook.Core.Models.Profile", b =>
                {
                    b.HasOne("Letterbook.Core.Models.Image", null)
                        .WithMany("Creators")
                        .HasForeignKey("ImageId");

                    b.HasOne("Letterbook.Core.Models.Account", "OwnedBy")
                        .WithMany()
                        .HasForeignKey("OwnedById");

                    b.Navigation("OwnedBy");
                });

            modelBuilder.Entity("NoteProfile", b =>
                {
                    b.HasOne("Letterbook.Core.Models.Note", null)
                        .WithMany()
                        .HasForeignKey("CreatedNotesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Letterbook.Core.Models.Profile", null)
                        .WithMany()
                        .HasForeignKey("CreatorsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("NoteProfile1", b =>
                {
                    b.HasOne("Letterbook.Core.Models.Profile", null)
                        .WithMany()
                        .HasForeignKey("LikedById")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Letterbook.Core.Models.Note", null)
                        .WithMany()
                        .HasForeignKey("LikedNotesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("NoteProfile2", b =>
                {
                    b.HasOne("Letterbook.Core.Models.Profile", null)
                        .WithMany()
                        .HasForeignKey("BoostedById")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Letterbook.Core.Models.Note", null)
                        .WithMany()
                        .HasForeignKey("BoostedNotesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Letterbook.Core.Models.Account", b =>
                {
                    b.Navigation("LinkedProfiles");
                });

            modelBuilder.Entity("Letterbook.Core.Models.Image", b =>
                {
                    b.Navigation("Creators");

                    b.Navigation("Mentions");

                    b.Navigation("Visibility");
                });

            modelBuilder.Entity("Letterbook.Core.Models.Note", b =>
                {
                    b.Navigation("Mentions");

                    b.Navigation("Replies");

                    b.Navigation("Visibility");
                });

            modelBuilder.Entity("Letterbook.Core.Models.Profile", b =>
                {
                    b.Navigation("RelatedAccounts");
                });
#pragma warning restore 612, 618
        }
    }
}
