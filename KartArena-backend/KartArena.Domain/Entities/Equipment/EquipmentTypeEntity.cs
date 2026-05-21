using KartArena.Domain.Common;
using KartArena.Domain.Entities.Reservations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KartArena.Domain.Entities.Equipment
{
    public class EquipmentTypeEntity : BaseEntity
    {
        private const int LowStockThreshold = 5;

        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Size { get; set; } = string.Empty;
        public string? Description { get; set; }

        public EquipmentCategory Category { get; set; }

        // Navigation
        public ICollection<EquipmentItemEntity> Items { get; set; } = new List<EquipmentItemEntity>();

        // Computed properties - not mapped to DB
        public int TotalItems => Items.Count;

        public int AvailableItems => Items.Count(i => i.Status == EquipmentItemStatus.Available);
        public int InUseItems => Items.Count(i => i.Status == EquipmentItemStatus.InUse);
        public int MaintenanceItems => Items.Count(i => i.Status == EquipmentItemStatus.Maintenance);

        public int LostItems => Items.Count(i => i.Status == EquipmentItemStatus.Lost);

        public EquipmentStockStatus StockStatus
        {
            get
            {
                if (AvailableItems == 0)
                    return EquipmentStockStatus.OutOfStock;

                if (AvailableItems <= LowStockThreshold)
                    return EquipmentStockStatus.Low;

                return EquipmentStockStatus.Good;
            }
        }

        // Collections
    }
}