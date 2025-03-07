﻿// <auto-generated />
using System;
using Ichiba.Shipment.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ichiba.Shipment.Infrastructure.Migrations
{
    [DbContext(typeof(ShipmentDbContext))]
    [Migration("20250222080644_UpdateFiledEntitiyPackage")]
    partial class UpdateFiledEntitiyPackage
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Ichiba.Shipment.Domain.Entities.Carrier", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("DeletedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("DeletedBy")
                        .HasColumnType("text");

                    b.Property<int>("ShippingMethod")
                        .HasColumnType("integer");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("text");

                    b.Property<bool?>("lastmile_tracking")
                        .HasColumnType("boolean");

                    b.Property<string>("logo")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Carriers");
                });

            modelBuilder.Entity("Ichiba.Shipment.Domain.Entities.Package", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<decimal>("Amount")
                        .HasColumnType("numeric");

                    b.Property<Guid>("CarrierId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreateAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("CreateBy")
                        .HasColumnType("uuid");

                    b.Property<int>("CubitUnit")
                        .HasColumnType("integer");

                    b.Property<Guid>("CustomerId")
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("DeleteAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("DeleteBy")
                        .HasColumnType("uuid");

                    b.Property<decimal>("Height")
                        .HasColumnType("numeric");

                    b.Property<decimal>("Length")
                        .HasColumnType("numeric");

                    b.Property<string>("Note")
                        .HasColumnType("text");

                    b.Property<string>("PackageNumber")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("UpdateAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("UpdateBy")
                        .HasColumnType("uuid");

                    b.Property<Guid>("WarehouseId")
                        .HasColumnType("uuid");

                    b.Property<decimal>("Weight")
                        .HasColumnType("numeric");

                    b.Property<int>("WeightUnit")
                        .HasColumnType("integer");

                    b.Property<decimal>("Width")
                        .HasColumnType("numeric");

                    b.HasKey("Id");

                    b.ToTable("Packages");
                });

            modelBuilder.Entity("Ichiba.Shipment.Domain.Entities.PackageProduct", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Origin")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<double>("OriginPrice")
                        .HasColumnType("double precision");

                    b.Property<Guid>("PackageId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("ProductId")
                        .HasColumnType("uuid");

                    b.Property<string>("ProductLink")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ProductName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Quantity")
                        .HasColumnType("integer");

                    b.Property<double>("Total")
                        .HasColumnType("double precision");

                    b.Property<double>("Unit")
                        .HasColumnType("double precision");

                    b.HasKey("Id");

                    b.HasIndex("PackageId");

                    b.ToTable("PackageProducts");
                });

            modelBuilder.Entity("Ichiba.Shipment.Domain.Entities.ShipmentAddress", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("City")
                        .HasColumnType("text");

                    b.Property<string>("Code")
                        .HasColumnType("text");

                    b.Property<string>("Country")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreateAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("CreateBy")
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("DeleteAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("DeleteBy")
                        .HasColumnType("uuid");

                    b.Property<string>("District")
                        .HasColumnType("text");

                    b.Property<double?>("Latitude")
                        .HasColumnType("double precision");

                    b.Property<double?>("Longitude")
                        .HasColumnType("double precision");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Phone")
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<string>("PostCode")
                        .HasColumnType("text");

                    b.Property<string>("PrefixPhone")
                        .HasColumnType("text");

                    b.Property<Guid>("ShipmentId")
                        .HasColumnType("uuid");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("UpdateAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("UpdateBy")
                        .HasColumnType("uuid");

                    b.Property<string>("Ward")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("ShipmentId");

                    b.ToTable("ShipmentAddresses");
                });

            modelBuilder.Entity("Ichiba.Shipment.Domain.Entities.ShipmentEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("CarrierId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreateAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("CreateBy")
                        .HasColumnType("uuid");

                    b.Property<Guid>("CustomerId")
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("DeleteAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("DeleteBy")
                        .HasColumnType("uuid");

                    b.Property<decimal>("Height")
                        .HasColumnType("numeric");

                    b.Property<string>("Note")
                        .HasColumnType("text");

                    b.Property<string>("ShipmentNumber")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<decimal>("TotalAmount")
                        .HasColumnType("numeric");

                    b.Property<DateTime?>("UpdateAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("UpdateBy")
                        .HasColumnType("uuid");

                    b.Property<Guid>("WarehouseId")
                        .HasColumnType("uuid");

                    b.Property<decimal>("Weight")
                        .HasColumnType("numeric");

                    b.HasKey("Id");

                    b.ToTable("Shipments");
                });

            modelBuilder.Entity("Ichiba.Shipment.Domain.Entities.ShipmentPackage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreateAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("CreateBy")
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("DeleteAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("DeleteBy")
                        .HasColumnType("uuid");

                    b.Property<Guid>("PackageId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("ShipmentId")
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("UpdateAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("UpdateBy")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("PackageId");

                    b.HasIndex("ShipmentId");

                    b.ToTable("ShipmentPackages");
                });

            modelBuilder.Entity("Ichiba.Shipment.Domain.Entities.Tax", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid?>("PackageProductId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("PackageProductId");

                    b.ToTable("Tax");
                });

            modelBuilder.Entity("Ichiba.Shipment.Domain.Entities.Warehouse", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("City")
                        .HasColumnType("text");

                    b.Property<string>("Code")
                        .HasColumnType("text");

                    b.Property<string>("Country")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreateAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("CreateBy")
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("DeleteAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("DeleteBy")
                        .HasColumnType("uuid");

                    b.Property<string>("District")
                        .HasColumnType("text");

                    b.Property<double?>("Latitude")
                        .HasColumnType("double precision");

                    b.Property<string>("Logo")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<double?>("Longitude")
                        .HasColumnType("double precision");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Phone")
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<string>("PostCode")
                        .HasColumnType("text");

                    b.Property<string>("PrefixPhone")
                        .HasColumnType("text");

                    b.Property<DateTime?>("UpdateAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("UpdateBy")
                        .HasColumnType("uuid");

                    b.Property<string>("Ward")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Warehouses");
                });

            modelBuilder.Entity("PackageAddress", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("City")
                        .HasColumnType("text");

                    b.Property<string>("Code")
                        .HasColumnType("text");

                    b.Property<string>("Country")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreateAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("CreateBy")
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("DeleteAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("DeleteBy")
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("DeliveryDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("DeliveryInstructions")
                        .HasColumnType("text");

                    b.Property<string>("District")
                        .HasColumnType("text");

                    b.Property<DateTime?>("EstimatedDeliveryDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsDefault")
                        .HasColumnType("boolean");

                    b.Property<double?>("Latitude")
                        .HasColumnType("double precision");

                    b.Property<double?>("Longitude")
                        .HasColumnType("double precision");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("PackageId")
                        .HasColumnType("uuid");

                    b.Property<string>("Phone")
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<string>("PostCode")
                        .HasColumnType("text");

                    b.Property<string>("PrefixPhone")
                        .HasColumnType("text");

                    b.Property<string>("SearchIndex")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("SensitiveDataFlag")
                        .HasColumnType("boolean");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("UpdateAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("UpdateBy")
                        .HasColumnType("uuid");

                    b.Property<string>("Ward")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("PackageId");

                    b.ToTable("PackageAddresses");
                });

            modelBuilder.Entity("Ichiba.Shipment.Domain.Entities.PackageProduct", b =>
                {
                    b.HasOne("Ichiba.Shipment.Domain.Entities.Package", "Package")
                        .WithMany("PackageProducts")
                        .HasForeignKey("PackageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Package");
                });

            modelBuilder.Entity("Ichiba.Shipment.Domain.Entities.ShipmentAddress", b =>
                {
                    b.HasOne("Ichiba.Shipment.Domain.Entities.ShipmentEntity", "Shipment")
                        .WithMany("Addresses")
                        .HasForeignKey("ShipmentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Shipment");
                });

            modelBuilder.Entity("Ichiba.Shipment.Domain.Entities.ShipmentPackage", b =>
                {
                    b.HasOne("Ichiba.Shipment.Domain.Entities.Package", "Package")
                        .WithMany()
                        .HasForeignKey("PackageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Ichiba.Shipment.Domain.Entities.ShipmentEntity", "Shipment")
                        .WithMany("ShipmentPackages")
                        .HasForeignKey("ShipmentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Package");

                    b.Navigation("Shipment");
                });

            modelBuilder.Entity("Ichiba.Shipment.Domain.Entities.Tax", b =>
                {
                    b.HasOne("Ichiba.Shipment.Domain.Entities.PackageProduct", null)
                        .WithMany("Taxes")
                        .HasForeignKey("PackageProductId");
                });

            modelBuilder.Entity("PackageAddress", b =>
                {
                    b.HasOne("Ichiba.Shipment.Domain.Entities.Package", "Package")
                        .WithMany("PackageAdresses")
                        .HasForeignKey("PackageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Package");
                });

            modelBuilder.Entity("Ichiba.Shipment.Domain.Entities.Package", b =>
                {
                    b.Navigation("PackageAdresses");

                    b.Navigation("PackageProducts");
                });

            modelBuilder.Entity("Ichiba.Shipment.Domain.Entities.PackageProduct", b =>
                {
                    b.Navigation("Taxes");
                });

            modelBuilder.Entity("Ichiba.Shipment.Domain.Entities.ShipmentEntity", b =>
                {
                    b.Navigation("Addresses");

                    b.Navigation("ShipmentPackages");
                });
#pragma warning restore 612, 618
        }
    }
}
