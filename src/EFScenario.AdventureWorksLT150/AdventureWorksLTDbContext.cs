namespace EFScenario.AdventureWorksLT
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class AdventureWorksLTDbContext : DbContext
    {
        public AdventureWorksLTDbContext()
            : base("name=AdventureWorksLTDbContext")
        {
        }

        //public virtual DbSet<Address> Address { get; set; }
        public virtual DbSet<Customer> Customer { get; set; }
        //public virtual DbSet<CustomerAddress> CustomerAddress { get; set; }
        public virtual DbSet<Product> Product { get; set; }
        public virtual DbSet<ProductCategory> ProductCategory { get; set; }
        public virtual DbSet<ProductDescription> ProductDescription { get; set; }
        public virtual DbSet<ProductModel> ProductModel { get; set; }
        //public virtual DbSet<ProductModelProductDescription> ProductModelProductDescription { get; set; }
        //tmp2 - public virtual DbSet<SalesOrderDetail> SalesOrderDetail { get; set; }
        //tmp2 - public virtual DbSet<SalesOrderHeader> SalesOrderHeader { get; set; }

        //protected override void OnModelCreating(DbModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<Address>()
        //        .HasMany(e => e.CustomerAddress)
        //        .WithRequired(e => e.Address)
        //        .WillCascadeOnDelete(false);

        //    modelBuilder.Entity<Address>()
        //        .HasMany(e => e.SalesOrderHeader)
        //        .WithOptional(e => e.Address)
        //        .HasForeignKey(e => e.BillToAddressID);

        //    modelBuilder.Entity<Address>()
        //        .HasMany(e => e.SalesOrderHeader1)
        //        .WithOptional(e => e.Address1)
        //        .HasForeignKey(e => e.ShipToAddressID);

        //    modelBuilder.Entity<Customer>()
        //        .Property(e => e.PasswordHash)
        //        .IsUnicode(false);

        //    modelBuilder.Entity<Customer>()
        //        .Property(e => e.PasswordSalt)
        //        .IsUnicode(false);

        //    modelBuilder.Entity<Customer>()
        //        .HasMany(e => e.CustomerAddress)
        //        .WithRequired(e => e.Customer)
        //        .WillCascadeOnDelete(false);

        //    modelBuilder.Entity<Customer>()
        //        .HasMany(e => e.SalesOrderHeader)
        //        .WithRequired(e => e.Customer)
        //        .WillCascadeOnDelete(false);

        //    modelBuilder.Entity<Product>()
        //        .Property(e => e.StandardCost)
        //        .HasPrecision(19, 4);

        //    modelBuilder.Entity<Product>()
        //        .Property(e => e.ListPrice)
        //        .HasPrecision(19, 4);

        //    modelBuilder.Entity<Product>()
        //        .Property(e => e.Weight)
        //        .HasPrecision(8, 2);

        //    modelBuilder.Entity<Product>()
        //        .HasMany(e => e.SalesOrderDetail)
        //        .WithRequired(e => e.Product)
        //        .WillCascadeOnDelete(false);

        //    modelBuilder.Entity<ProductCategory>()
        //        .HasMany(e => e.ProductCategory1)
        //        .WithOptional(e => e.ProductCategory2)
        //        .HasForeignKey(e => e.ParentProductCategoryID);

        //    modelBuilder.Entity<ProductDescription>()
        //        .HasMany(e => e.ProductModelProductDescription)
        //        .WithRequired(e => e.ProductDescription)
        //        .WillCascadeOnDelete(false);

        //    modelBuilder.Entity<ProductModel>()
        //        .HasMany(e => e.ProductModelProductDescription)
        //        .WithRequired(e => e.ProductModel)
        //        .WillCascadeOnDelete(false);

        //    modelBuilder.Entity<ProductModelProductDescription>()
        //        .Property(e => e.Culture)
        //        .IsFixedLength();

        //    modelBuilder.Entity<SalesOrderDetail>()
        //        .Property(e => e.UnitPrice)
        //        .HasPrecision(19, 4);

        //    modelBuilder.Entity<SalesOrderDetail>()
        //        .Property(e => e.UnitPriceDiscount)
        //        .HasPrecision(19, 4);

        //    modelBuilder.Entity<SalesOrderDetail>()
        //        .Property(e => e.LineTotal)
        //        .HasPrecision(38, 6);

        //    modelBuilder.Entity<SalesOrderHeader>()
        //        .Property(e => e.CreditCardApprovalCode)
        //        .IsUnicode(false);

        //    modelBuilder.Entity<SalesOrderHeader>()
        //        .Property(e => e.SubTotal)
        //        .HasPrecision(19, 4);

        //    modelBuilder.Entity<SalesOrderHeader>()
        //        .Property(e => e.TaxAmt)
        //        .HasPrecision(19, 4);

        //    modelBuilder.Entity<SalesOrderHeader>()
        //        .Property(e => e.Freight)
        //        .HasPrecision(19, 4);

        //    modelBuilder.Entity<SalesOrderHeader>()
        //        .Property(e => e.TotalDue)
        //        .HasPrecision(19, 4);
        //}
    }
}
