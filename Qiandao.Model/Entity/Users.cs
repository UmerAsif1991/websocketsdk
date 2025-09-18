using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qiandao.Model.Entity
{
    public class Users
    {
        [Key]
        public long UserId { get; set; }
        public string Name { get; set; }
        public string fName { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public Nullable<bool> IsAdmin { get; set; }
        public Nullable<bool> IsSuperAdmin { get; set; }
        public int? RoleId { get; set; }
        public Nullable<System.DateTime> deletedAt { get; set; }
        public bool IsActive { get; set; }
        public Nullable<System.DateTime> updatedAt { get; set; }
        public System.DateTime created_date { get; set; }
        public Nullable<long> TenantId { get; set; }
        public string? Email { get; set; }
        public string? Code { get; set; }
        public string? City { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AlternatePhone { get; set; }
        public int FailedLoginAttempts { get; set; } = 0;
        public bool IsLocked { get; set; } = false;
        public Nullable<System.DateTime> LockoutEnd { get; set; }
        public string? AddressLine { get; set; }
        public string? CityId { get; set; }
        public string? PostalCode { get; set; }
        public string? ProfileImagePath { get; set; }
        public string? LanguagePreference { get; set; }
        public Nullable<System.DateTime> LastPasswordChange { get; set; }
        public Nullable<System.DateTime> PasswordExpiryDate { get; set; }
        public Nullable<bool> IsTwoFactorEnabled { get; set; }
        public string? SecurityAnswerHash { get; set; }
        public string? PasswordHash { get; set; }
        public string? PasswordSalt { get; set; }
        public string? SecurityQuestion { get; set; }

        public virtual Roles? Role { get; set; }
        public virtual Tenant Tenant { get; set; }
    }
}
