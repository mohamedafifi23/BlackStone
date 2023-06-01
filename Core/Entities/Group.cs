using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class Group: BaseEntitySingleKey
    {
        public string Name { get; set; }
        public string Description { get; set; }   
        
        public virtual ICollection<UserGroup> UserGroups { get; set; }
    }
}
