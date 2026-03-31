using KartArena.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Domain.Entities.Identity
{
    public class RoleEntity : BaseEntity
    {
        public string Name { get; set; }= default!;

        public IReadOnlyCollection<UserEntity>? Users { get; set; }=new List<UserEntity>();
    }
}
