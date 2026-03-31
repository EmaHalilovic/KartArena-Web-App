using KartArena.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartArena.Domain.Entities.Equipment
{
    public class EquipmentItemEntity:BaseEntity
    {
        [MaxLength(50)]
        public string ItemCode { get; set; } = string.Empty;
        [Required]
        public int EquipmentTypeId { get; set; }
        public EquipmentTypeEntity EquipmentType { get; set; }

        public EquipmentItemStatus Status { get; set; } = EquipmentItemStatus.Available;
        public DateTime? PurchaseDate { get; set; }
        public DateTime? LastInspectedAt { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
