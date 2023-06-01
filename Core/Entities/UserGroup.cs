using Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class UserGroup: BaseEntity
    {
        public string Email { get; set; }
        public long GroupId { get; set; }
        public virtual Group Group { get; set; }
    }
}
