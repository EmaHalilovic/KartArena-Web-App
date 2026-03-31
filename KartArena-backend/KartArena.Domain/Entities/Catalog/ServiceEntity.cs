using KartArena.Domain.Common;
using KartArena.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Domain.Entities.Catalog
{
    public class ServiceEntity : BaseEntity
    {
        public DateTime? ServiceDate { get; set; }
        public string? Description { get; set; }
        public decimal? Cost { get; set; }
      

        // FK
        public int? KartId { get; set; }
        public KartEntity? Kart { get; set; }

        public int? EmployeeId { get; set; }
        public UserEntity? Employee { get; set; }
    }
}
