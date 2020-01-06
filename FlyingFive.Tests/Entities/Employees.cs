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

    //public class EmployeeMap : EntityMappingConfiguration<Employee>
    //{
    //    public EmployeeMap()
    //    {
    //        this.Table("Employees");
    //        this.Property(e => e.EmployeeID).HasColumnName("EmployeeID").HasMaxSize(4).HasIdentity().HasRequired(true).HasPrimaryKey();
    //        this.Property(e => e.LastName).HasMaxSize(20).HasRequired(true);
    //        this.Property(e => e.FirstName).HasMaxSize(10).HasRequired(true);
    //        this.Property(e => e.Title).HasMaxSize(30);
    //        this.Property(e => e.TitleOfCourtesy).HasMaxSize(25);
    //        this.Property(e => e.BirthDate).HasMaxSize(8);
    //        this.Property(e => e.HireDate).HasMaxSize(8);
    //        this.Property(e => e.Address).HasMaxSize(60);
    //        this.Property(e => e.City).HasMaxSize(15);
    //        this.Property(e => e.Region).HasMaxSize(15);
    //        this.Property(e => e.PostalCode).HasMaxSize(10);
    //        this.Property(e => e.Country).HasMaxSize(15);
    //        this.Property(e => e.HomePhone).HasMaxSize(24);
    //        this.Property(e => e.Extension).HasMaxSize(4);
    //        this.Property(e => e.Photo).HasMaxSize(int.MaxValue);
    //        this.Property(e => e.Notes).HasMaxSize(int.MaxValue);
    //        this.Property(e => e.ReportsTo).HasMaxSize(4);
    //        this.Property(e => e.PhotoPath).HasMaxSize(255);
    //    }
    //}
}
