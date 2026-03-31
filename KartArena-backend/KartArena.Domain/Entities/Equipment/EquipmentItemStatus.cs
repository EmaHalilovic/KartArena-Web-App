using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Domain.Entities.Equipment
{
    public enum EquipmentItemStatus
    {
        Available = 0,
        InUse = 1,
        Maintenance = 2,
        Lost = 3,
    }
}
