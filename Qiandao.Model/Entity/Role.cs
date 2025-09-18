using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qiandao.Model.Entity
{
    public class Role
    {
        public Role()
        {
            this.Users = new HashSet<Users>();
        }

        [Key]
        public int RoleId { get; set; }
        public string Name { get; set; }
        public Nullable<System.DateTime> created_date { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<System.DateTime> updated { get; set; }
        public Nullable<System.DateTime> deletedAt { get; set; }
        public Nullable<long> CreatedBy { get; set; }
        public Nullable<long> ModifiedBy { get; set; }
        public Nullable<long> TenantId { get; set; }

        public virtual ICollection<Users> Users { get; set; }
        public virtual Tenant Tenant { get; set; }
    }
}
