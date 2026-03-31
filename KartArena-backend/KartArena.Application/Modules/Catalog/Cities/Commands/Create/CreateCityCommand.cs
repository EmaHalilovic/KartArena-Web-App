using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Application.Modules.Catalog.Cities.Commands.Create
{
   public class CreateCityCommand : IRequest<int>
    {

        public required string Name { get; set; }
        public required string PostalCode { get; set; }
        public required string Country { get; set; }

    }
}
