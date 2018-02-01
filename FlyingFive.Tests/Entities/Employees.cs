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
    public class Employees
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
}
