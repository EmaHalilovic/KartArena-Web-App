using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KartArena.Domain.Common;
using KartArena.Domain.Entities.Identity;

namespace KartArena.Domain.Entities.Catalog
{
   public class CityEntity : BaseEntity
    {
        public required string Name { get; set; }
        public required string PostalCode { get; set; }   
        public required string Country { get; set; }

        //Collections 
        public IReadOnlyCollection<UserEntity>? Users { get; set; }
        public IReadOnlyCollection<TrackEntity>? Tracks { get; set; }

    }
}
