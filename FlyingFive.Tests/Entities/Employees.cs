using FlyingFive.Data.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlyingFive.Tests.Entities
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class Employee
    {

        /// <summary>
        /// 
        /// </summary>
        public virtual int EmployeeID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual string LastName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual string FirstName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual string Title { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual string TitleOfCourtesy { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual DateTime? BirthDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual DateTime? HireDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual string Address { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual string City { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual string Region { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual string PostalCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual string Country { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual string HomePhone { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual string Extension { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual byte[] Photo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual string Notes { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual int? ReportsTo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual string PhotoPath { get; set; }

    }

    public class EmployeeMap : EntityMappingConfiguration<Employee>
    {
        public EmployeeMap()
        {
            this.Table("Employees");
            this.Property(e => e.EmployeeID).HasColumnName("EmployeeID").HasMaxLength(4).HasIdentity().HasRequired(true).HasPrimaryKey();
            this.Property(e => e.LastName).HasMaxLength(20).HasRequired(true);
            this.Property(e => e.FirstName).HasMaxLength(10).HasRequired(true);
            this.Property(e => e.Title).HasMaxLength(30);
            this.Property(e => e.TitleOfCourtesy).HasMaxLength(25);
            this.Property(e => e.BirthDate).HasMaxLength(8);
            this.Property(e => e.HireDate).HasMaxLength(8);
            this.Property(e => e.Address).HasMaxLength(60);
            this.Property(e => e.City).HasMaxLength(15);
            this.Property(e => e.Region).HasMaxLength(15);
            this.Property(e => e.PostalCode).HasMaxLength(10);
            this.Property(e => e.Country).HasMaxLength(15);
            this.Property(e => e.HomePhone).HasMaxLength(24);
            this.Property(e => e.Extension).HasMaxLength(4);
            this.Property(e => e.Photo).HasMaxLength(int.MaxValue);
            this.Property(e => e.Notes).HasMaxLength(int.MaxValue);
            this.Property(e => e.ReportsTo).HasMaxLength(4);
            this.Property(e => e.PhotoPath).HasMaxLength(255);
        }
    }
}
