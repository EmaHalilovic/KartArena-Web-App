import {
  EquipmentCategory,
  GetEquipmentByIdQueryDto,
  EquipmentItemStatus,
  EquipmentStockStatus,
  ListWithItemsEquipmentQueryDtoItem,
} from '../../../api-services/equipment/equipment-api.models';

export interface EquipmentSummaryCard {
  key: string;
  label: string;
  count: number;
  tone: string;
  icon: string;
}

export function getEquipmentStockTone(status: EquipmentStockStatus): string {
  switch (status) {
    case EquipmentStockStatus.Good:
      return 'good';
    case EquipmentStockStatus.Low:
      return 'low';
    case EquipmentStockStatus.OutOfStock:
      return 'out-of-stock';
    default:
      return 'neutral';
  }
}

export function getEquipmentStockLabel(status: EquipmentStockStatus): string {
  switch (status) {
    case EquipmentStockStatus.Good:
      return 'Good';
    case EquipmentStockStatus.Low:
      return 'Low';
    case EquipmentStockStatus.OutOfStock:
      return 'Out of stock';
    default:
      return 'Unknown';
  }
}

export function getEquipmentCategoryLabel(category: EquipmentCategory): string {
  switch (category) {
    case EquipmentCategory.Helmet:
      return 'Helmet';
    case EquipmentCategory.Suit:
      return 'Suit';
    case EquipmentCategory.Gloves:
      return 'Gloves';
    case EquipmentCategory.Balaclava:
      return 'Balaclava';
    case EquipmentCategory.Other:
      return 'Other';
    default:
      return 'Other';
  }
}

export function getItemStatusTone(status: EquipmentItemStatus): string {
  switch (status) {
    case EquipmentItemStatus.Available:
      return 'good';
    case EquipmentItemStatus.InUse:
      return 'info';
    case EquipmentItemStatus.Maintenance:
      return 'warn';
    case EquipmentItemStatus.Lost:
      return 'danger';
    default:
      return 'neutral';
  }
}

export function getItemStatusLabel(status: EquipmentItemStatus): string {
  switch (status) {
    case EquipmentItemStatus.Available:
      return 'Available';
    case EquipmentItemStatus.InUse:
      return 'In use';
    case EquipmentItemStatus.Maintenance:
      return 'Maintenance';
    case EquipmentItemStatus.Lost:
      return 'Lost';
    default:
      return 'Unknown';
  }
}

export function buildEquipmentSummaryCards(
  equipment: GetEquipmentByIdQueryDto & { items?: ListWithItemsEquipmentQueryDtoItem[] }
): EquipmentSummaryCard[] {
  const items = equipment.items ?? [];

  return [
    { key: 'total', label: 'Total items', count: equipment.totalItems || items.length, tone: 'neutral', icon: 'inventory_2' },
    {
      key: 'available',
      label: 'Available',
      count: equipment.availableItems || countByStatus(items, EquipmentItemStatus.Available),
      tone: 'good',
      icon: 'check_circle',
    },
    {
      key: 'inUse',
      label: 'In use',
      count: equipment.inUseItems || countByStatus(items, EquipmentItemStatus.InUse),
      tone: 'info',
      icon: 'sports_motorsports',
    },
    {
      key: 'maintenance',
      label: 'Maintenance',
      count: equipment.maintenanceItems || countByStatus(items, EquipmentItemStatus.Maintenance),
      tone: 'warn',
      icon: 'build_circle',
    },
    {
      key: 'lost',
      label: 'Lost',
      count: equipment.lostItems || countByStatus(items, EquipmentItemStatus.Lost),
      tone: 'danger',
      icon: 'report_gmailerrorred',
    },
  ];
}

export function getEquipmentCategories(): EquipmentCategory[] {
  return Object.values(EquipmentCategory);
}

function countByStatus(items: ListWithItemsEquipmentQueryDtoItem[], status: EquipmentItemStatus): number {
  return items.filter((item) => item.status === status).length;
}
