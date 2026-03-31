import { PageResult } from '../../core/models/paging/page-result';
import { BasePagedQuery } from '../../core/models/paging/base-paged-query';

// === ENUMS ===

/**
 * Equipment stock status enum.
 */
export enum EquipmentStockStatus {
  Good = 'Good',
  Low = 'Low',
  OutOfStock = 'OutOfStock',
}

/**
 * Equipment item status enum.
 */
export enum EquipmentItemStatus {
  Available = 'Available',
  InUse = 'InUse',
  Maintenance = 'Maintenance',
  Lost = 'Lost',
}

/**
 * Equipment category enum.
 */
export enum EquipmentCategory {
  Helmet = 'Helmet',
  Suit = 'Suit',
  Gloves = 'Gloves',
  Balaclava = 'Balaclava',
  Other = 'Other',
}

// === QUERIES (READ) ===

/**
 * Query parameters for listing equipment types.
 */
export class ListEquipmentRequest extends BasePagedQuery {
  search?: string | null;
  onlyActive?: boolean | null;
  category?: EquipmentCategory | null;
  stockStatus?: EquipmentStockStatus | null;

  constructor() {
    super();
  }
}

/**
 * Query parameters for listing equipment types with their items.
 */

/**
 * Response item for equipment list.
 */
export interface ListEquipmentQueryDto {
  id: number;
  name: string;
  category: EquipmentCategory;
  size: string;
  price: number;
  totalItems: number;
  availableItems: number;
  inUseItems: number;
  maintenanceItems: number;
  lostItems: number;
  stockStatus: EquipmentStockStatus;
  isActive: boolean;
  description?: string | null;
}

export class ListWithItemsEquipmentRequest extends BasePagedQuery {
  search?: string | null;
}

/**
 * Equipment item in list-with-items and detail responses.
 */
export interface ListWithItemsEquipmentQueryDtoItem {
  id: number;
  itemCode: string;
  status: EquipmentItemStatus;
  purchaseDate?: string | null;
  lastInspectedAt?: string | null;
  notes?: string | null;
}
/**
 * Response item for equipment list with nested items.
 * Matches the backend paged query item shape.
 */
export interface ListWithItemsEquipmentQueryDto {
  id: number;
  name: string;
  price: number;
  size: string;
  description?: string | null;
  category: EquipmentCategory;
  totalItems: number;
  availableItems: number;
  inUseItems: number;
  maintenanceItems: number;
  lostItems: number;
  stockStatus: EquipmentStockStatus;
  items: ListWithItemsEquipmentQueryDtoItem[];
}

/**
 * Response for equipment details by id.
 */
export interface GetEquipmentByIdQueryDto {
  id: number;
  name: string;
  category: EquipmentCategory;
  size: string;
  price: number;
  totalItems: number;
  availableItems: number;
 inUseItems: number;
  maintenanceItems: number;
  lostItems: number;
  stockStatus: EquipmentStockStatus;
  isActive: boolean;
  description?: string | null;
  items: GetEquipmentByIdQueryDtoItem[];
}

/**
 * Response for equipment details with nested items.
 */
export interface GetEquipmentByIdQueryDtoItem {
    id: number;
  itemCode: string;
  status: EquipmentItemStatus;
  purchaseDate?: string | null;
  lastInspectedAt?: string | null;
  notes?: string | null;
}

/**
 * Paged response for equipment list.
 */
export type ListEquipmentResponse = PageResult<ListEquipmentQueryDto>;

/**
 * Paged response for equipment list with items.
 */
export type ListWithItemsEquipmentResponse = PageResult<ListWithItemsEquipmentQueryDto>;

// === COMMANDS (WRITE) ===

/**
 * Command for creating an equipment type.
 */
export interface CreateEquipmentTypeCommand {
  name: string;
  category: EquipmentCategory;
  size: string;
  price: number;
  description?: string | null;
}

/**
 * Command for updating an equipment type.
 */
export interface UpdateEquipmentTypeCommand {
  name?: string | null;
  category?: EquipmentCategory | null;
  size?: string | null;
  price?: number | null;
  description?: string | null;
}

/**
 * Command for creating one equipment item.
 */
export interface AddEquipmentItemCommand {
  itemCode: string;
  status: EquipmentItemStatus;
  purchaseDate?: string | null;
  lastInspectedAt?: string | null;
  notes?: string | null;
}

/**
 * Command for bulk-creating equipment items.
 */
export interface BulkAddEquipmentItemsCommand {
  prefix: string;
  startNumber: number;
  count: number;
  numberPadding?: number | null;
  status: EquipmentItemStatus;
  purchaseDate?: string | null;
  lastInspectedAt?: string | null;
  notes?: string | null;
}

// === COMPATIBILITY ALIASES ===

export type EquipmentItemDto = ListWithItemsEquipmentQueryDtoItem;
export type EquipmentTypeListItemDto = ListEquipmentQueryDto;
export type EquipmentTypeWithItemsListItemDto = ListWithItemsEquipmentQueryDto;
export type EquipmentTypeDetailsDto = GetEquipmentByIdQueryDto;
