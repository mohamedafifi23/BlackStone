using Core.IRepositories;
using Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class GroupRepository:IGroupRepository
    {
        public GroupRepository(ApplicationDbContext context)
        {
            
        }
    }
}
