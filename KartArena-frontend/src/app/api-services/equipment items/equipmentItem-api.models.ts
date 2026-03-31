import { PageResult } from '../../core/models/paging/page-result';
import { BasePagedQuery } from '../../core/models/paging/base-paged-query';

// === ENUMS ===

/**
 * Equipment item status enum.
 */
export enum EquipmentItemStatus {
  Available = 'Available',
  InUse = 'InUse',
  Maintenance = 'Maintenance',
  Lost = 'Lost',
}

// === QUERIES (READ) ===

/**
 * Query parameters for listing equipment items.
 */
export class ListEquipmentItemRequest extends BasePagedQuery {
  search?: string | null;
  equipmentTypeId?: number | null;
  status?: EquipmentItemStatus | null;

  constructor() {
    super();
  }
}

/**
 * Response item for equipment item list.
 */
export interface ListEquipmentItemQueryDto {
  id: number;
  itemCode: string;
  equipmentTypeId: number;
  equipmentTypeName?: string | null;
  status: EquipmentItemStatus;
  purchaseDate?: string | null;
  lastInspectedAt?: string | null;
  notes?: string | null;
}

/**
 * Response for equipment item details by id.
 */
export interface GetEquipmentItemByIdQueryDto {
  id: number;
  itemCode: string;
  equipmentTypeId: number;
  equipmentTypeName?: string | null;
  status: EquipmentItemStatus;
  purchaseDate?: string | null;
  lastInspectedAt?: string | null;
  notes?: string | null;
}

/**
 * Paged response for equipment item list.
 */
export type ListEquipmentItemResponse = PageResult<ListEquipmentItemQueryDto>;

// === COMMANDS (WRITE) ===

/**
 * Command for creating an equipment item.
 */
export interface CreateEquipmentItemCommand {
  itemCode: string;
  equipmentTypeId: number;
  status: EquipmentItemStatus;
  purchaseDate?: string | null;
  lastInspectedAt?: string | null;
  notes?: string | null;
}

/**
 * Command for updating an equipment item.
 */
export interface UpdateEquipmentItemCommand {
  itemCode?: string | null;
  equipmentTypeId?: number | null;
  status?: EquipmentItemStatus | null;
  purchaseDate?: string | null;
  lastInspectedAt?: string | null;
  notes?: string | null;
}

// === COMPATIBILITY ALIASES ===

export type EquipmentItemListItemDto = ListEquipmentItemQueryDto;
export type EquipmentItemDetailsDto = GetEquipmentItemByIdQueryDto;