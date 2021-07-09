﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace TimeOffTracker.Model
{
    public partial class masterContext : DbContext
    {
        public masterContext()
        {
        }

        public masterContext(DbContextOptions<masterContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ProjectRoleType> ProjectRoleTypes { get; set; }
        public virtual DbSet<Request> Requests { get; set; }
        public virtual DbSet<RequestType> RequestTypes { get; set; }
        public virtual DbSet<StateDetail> StateDetails { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserRole> UserRoles { get; set; }
        public virtual DbSet<UserSignature> UserSignatures { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=localhost; user=SA; password=456789Zxc");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "Cyrillic_General_CI_AS");

            modelBuilder.Entity<ProjectRoleType>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .HasName("Project_role_type_pk")
                    .IsClustered(false);

                entity.ToTable("Project_role_type", "_public");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Comments)
                    .IsUnicode(false)
                    .HasColumnName("comments");

                entity.Property(e => e.Deleted).HasColumnName("deleted");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("type");
            });

            modelBuilder.Entity<Request>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .HasName("Request_pk")
                    .IsClustered(false);

                entity.ToTable("Request", "_public");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.DateTimeFrom)
                    .HasColumnType("datetime")
                    .HasColumnName("date_time_from");

                entity.Property(e => e.DateTimeTo)
                    .HasColumnType("datetime")
                    .HasColumnName("date_time_to");

                entity.Property(e => e.ProjectRoleComment)
                    .IsUnicode(false)
                    .HasColumnName("project_role_comment");

                entity.Property(e => e.ProjectRoleTypeId).HasColumnName("project_role_type_id");

                entity.Property(e => e.Reason)
                    .IsUnicode(false)
                    .HasColumnName("reason");

                entity.Property(e => e.RequestTypeId).HasColumnName("request_type_id");

                entity.Property(e => e.StateDetailId).HasColumnName("state_detail_id");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.ProjectRoleType)
                    .WithMany(p => p.Requests)
                    .HasForeignKey(d => d.ProjectRoleTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Request_Project_role_type_id_fk");

                entity.HasOne(d => d.RequestType)
                    .WithMany(p => p.Requests)
                    .HasForeignKey(d => d.RequestTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Request_Request_type_id_fk");

                entity.HasOne(d => d.StateDetail)
                    .WithMany(p => p.Requests)
                    .HasForeignKey(d => d.StateDetailId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Request_State_detail_id_fk");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Requests)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("Request_User_id_fk");
            });

            modelBuilder.Entity<RequestType>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .HasName("Request_type_pk")
                    .IsClustered(false);

                entity.ToTable("Request_type", "_public");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Comments)
                    .IsUnicode(false)
                    .HasColumnName("comments");

                entity.Property(e => e.Deleted).HasColumnName("deleted");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("type");
            });

            modelBuilder.Entity<StateDetail>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .HasName("State_detail_pk")
                    .IsClustered(false);

                entity.ToTable("State_detail", "_public");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Comments)
                    .IsUnicode(false)
                    .HasColumnName("comments");

                entity.Property(e => e.Deleted).HasColumnName("deleted");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("type");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .HasName("User_pk")
                    .IsClustered(false);

                entity.ToTable("User", "_public");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Deleted).HasColumnName("deleted");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("email");

                entity.Property(e => e.FirstName)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("first_name");

                entity.Property(e => e.Login)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("login");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("password");

                entity.Property(e => e.RoleId).HasColumnName("role_id");

                entity.Property(e => e.SecondName)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("second_name");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("User_User_Role_id_fk");
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .HasName("User_Role_pk")
                    .IsClustered(false);

                entity.ToTable("User_Role", "_public");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Comments)
                    .IsUnicode(false)
                    .HasColumnName("comments");

                entity.Property(e => e.Deleted).HasColumnName("deleted");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("type");
            });

            modelBuilder.Entity<UserSignature>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .HasName("User_Signature_pk")
                    .IsClustered(false);

                entity.ToTable("User_Signature", "_public");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Approved).HasColumnName("approved");

                entity.Property(e => e.Deleted).HasColumnName("deleted");

                entity.Property(e => e.NInQueue).HasColumnName("N_in_queue");

                entity.Property(e => e.RequestId).HasColumnName("request_id");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Request)
                    .WithMany(p => p.UserSignatures)
                    .HasForeignKey(d => d.RequestId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("User_Signature_Request_id_fk");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserSignatures)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("User_Signature_User_id_fk");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}