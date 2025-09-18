using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qiandao.Model.Entity
{
    public class Tenant
    {
        [Key]
        public long TenantId { get; set; }
        public string TenantCode { get; set; }
        public string TenantName { get; set; }
        public string Subdomain { get; set; }
        public string CustomDomain { get; set; }
        public string IndustryType { get; set; }
        public Nullable<int> PlanId { get; set; }
        public string BillingCurrency { get; set; }
        public string BillingEmail { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string TimeZone { get; set; }
        public string DataRegion { get; set; }
        public Nullable<int> MaxUsers { get; set; }
        public Nullable<long> MaxStorageMB { get; set; }
        public string LogoPath { get; set; }
        public string ThemeColor { get; set; }
        public string Settings { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTimeOffset CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTimeOffset> ModifiedDate { get; set; }
        public string Status { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public Nullable<System.DateTimeOffset> DeletedDate { get; set; }
        public string DeletedBy { get; set; }
        public Nullable<int> MaximumAttendanceMachines { get; set; }
    }
}
