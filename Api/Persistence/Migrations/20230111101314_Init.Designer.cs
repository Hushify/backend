﻿// <auto-generated />

using Hushify.Api.Features.Drive.Entities;
using Hushify.Api.Features.Identity.Entities;
using System;
using Hushify.Api.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hushify.Api.Persistence.Migrations
{
    [DbContext(typeof(WorkspaceDbContext))]
    [Migration("20230111101314_Init")]
    partial class Init
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Hushify.Api.Persistence.Entities.AppRole", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<Guid>("WorkspaceId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .HasDatabaseName("RoleNameIndex");

                    b.HasIndex("WorkspaceId");

                    b.HasIndex("NormalizedName", "WorkspaceId")
                        .IsUnique();

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Hushify.Api.Persistence.Entities.AppUser", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("integer");

                    b.Property<KeyPairBundle>("AsymmetricEncKeyBundle")
                        .HasColumnType("jsonb");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("timestamp with time zone");

                    b.Property<SecretKeyBundle>("MasterKeyBundle")
                        .HasColumnType("jsonb");

                    b.Property<DateTimeOffset?>("ModifiedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("boolean");

                    b.Property<SecretKeyBundle>("RecoveryKeyBundle")
                        .HasColumnType("jsonb");

                    b.Property<SecretKeyBundle>("RecoveryMasterKeyBundle")
                        .HasColumnType("jsonb");

                    b.Property<string>("Salt")
                        .HasColumnType("text");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("text");

                    b.Property<KeyPairBundle>("SigningKeyBundle")
                        .HasColumnType("jsonb");

                    b.Property<string>("StripeCustomerId")
                        .HasColumnType("text");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("boolean");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<Guid>("WorkspaceId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.HasIndex("WorkspaceId")
                        .IsUnique();

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("Hushify.Api.Persistence.Entities.Drive.FileNode", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("BucketName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("EncryptedSize")
                        .HasColumnType("bigint");

                    b.Property<SecretKeyBundle>("FileKeyBundle")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("MaterializedPath")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<MetadataBundle>("MetadataBundle")
                        .HasColumnType("jsonb");

                    b.Property<Guid?>("ParentFolderId")
                        .HasColumnType("uuid");

                    b.Property<string>("Region")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("UploadStatus")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("WorkspaceId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("Key")
                        .IsUnique();

                    b.HasIndex("MaterializedPath");

                    b.HasIndex("ParentFolderId");

                    b.HasIndex("WorkspaceId");

                    b.HasIndex("Id", "WorkspaceId")
                        .IsUnique();

                    b.ToTable("Files");
                });

            modelBuilder.Entity("Hushify.Api.Persistence.Entities.Drive.FolderNode", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<SecretKeyBundle>("FolderKeyBundle")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<string>("FolderStatus")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("MaterializedPath")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<MetadataBundle>("MetadataBundle")
                        .HasColumnType("jsonb");

                    b.Property<Guid?>("ParentFolderId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("WorkspaceId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("MaterializedPath");

                    b.HasIndex("ParentFolderId");

                    b.HasIndex("WorkspaceId");

                    b.HasIndex("Id", "WorkspaceId")
                        .IsUnique();

                    b.ToTable("Folders");
                });

            modelBuilder.Entity("Hushify.Api.Persistence.Entities.RefreshToken", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<Guid?>("AppUserId")
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatedByUserAgent")
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("Expires")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ReasonRevoked")
                        .HasColumnType("text");

                    b.Property<string>("ReplacedByTokenId")
                        .HasColumnType("text");

                    b.Property<DateTimeOffset?>("Revoked")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("RevokedByUserAgent")
                        .HasColumnType("text");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("AppUserId");

                    b.HasIndex("ReplacedByTokenId");

                    b.ToTable("RefreshToken");
                });

            modelBuilder.Entity("Hushify.Api.Persistence.Entities.Workspace", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<long>("StorageSize")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("Workspaces");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.DataProtection.EntityFrameworkCore.DataProtectionKey", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("FriendlyName")
                        .HasColumnType("text");

                    b.Property<string>("Xml")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("DataProtectionKeys");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<System.Guid>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<System.Guid>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<System.Guid>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("text");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("text");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<System.Guid>", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uuid");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<System.Guid>", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("Hushify.Api.Persistence.Entities.AppRole", b =>
                {
                    b.HasOne("Hushify.Api.Persistence.Entities.Workspace", "Workspace")
                        .WithMany("AppRoles")
                        .HasForeignKey("WorkspaceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Workspace");
                });

            modelBuilder.Entity("Hushify.Api.Persistence.Entities.AppUser", b =>
                {
                    b.HasOne("Hushify.Api.Persistence.Entities.Workspace", "Workspace")
                        .WithOne("AppUser")
                        .HasForeignKey("Hushify.Api.Persistence.Entities.AppUser", "WorkspaceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Workspace");
                });

            modelBuilder.Entity("Hushify.Api.Persistence.Entities.Drive.FileNode", b =>
                {
                    b.HasOne("Hushify.Api.Persistence.Entities.Drive.FolderNode", "ParentFolder")
                        .WithMany("Files")
                        .HasForeignKey("ParentFolderId");

                    b.HasOne("Hushify.Api.Persistence.Entities.Workspace", "Workspace")
                        .WithMany()
                        .HasForeignKey("WorkspaceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ParentFolder");

                    b.Navigation("Workspace");
                });

            modelBuilder.Entity("Hushify.Api.Persistence.Entities.Drive.FolderNode", b =>
                {
                    b.HasOne("Hushify.Api.Persistence.Entities.Drive.FolderNode", "ParentFolder")
                        .WithMany("Folders")
                        .HasForeignKey("ParentFolderId");

                    b.HasOne("Hushify.Api.Persistence.Entities.Workspace", "Workspace")
                        .WithMany()
                        .HasForeignKey("WorkspaceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ParentFolder");

                    b.Navigation("Workspace");
                });

            modelBuilder.Entity("Hushify.Api.Persistence.Entities.RefreshToken", b =>
                {
                    b.HasOne("Hushify.Api.Persistence.Entities.AppUser", null)
                        .WithMany("RefreshTokens")
                        .HasForeignKey("AppUserId");

                    b.HasOne("Hushify.Api.Persistence.Entities.RefreshToken", "ReplacedByToken")
                        .WithMany()
                        .HasForeignKey("ReplacedByTokenId");

                    b.Navigation("ReplacedByToken");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<System.Guid>", b =>
                {
                    b.HasOne("Hushify.Api.Persistence.Entities.AppRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<System.Guid>", b =>
                {
                    b.HasOne("Hushify.Api.Persistence.Entities.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<System.Guid>", b =>
                {
                    b.HasOne("Hushify.Api.Persistence.Entities.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<System.Guid>", b =>
                {
                    b.HasOne("Hushify.Api.Persistence.Entities.AppRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Hushify.Api.Persistence.Entities.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<System.Guid>", b =>
                {
                    b.HasOne("Hushify.Api.Persistence.Entities.AppUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Hushify.Api.Persistence.Entities.AppUser", b =>
                {
                    b.Navigation("RefreshTokens");
                });

            modelBuilder.Entity("Hushify.Api.Persistence.Entities.Drive.FolderNode", b =>
                {
                    b.Navigation("Files");

                    b.Navigation("Folders");
                });

            modelBuilder.Entity("Hushify.Api.Persistence.Entities.Workspace", b =>
                {
                    b.Navigation("AppRoles");

                    b.Navigation("AppUser")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
