using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class BaseEntitySingleKey: BaseEntity
    {
        public long Id { get; set; }
    }
}
