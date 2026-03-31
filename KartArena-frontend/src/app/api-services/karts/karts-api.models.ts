import { PageResult } from '../../core/models/paging/page-result';
import { BasePagedQuery } from '../../core/models/paging/base-paged-query';

export interface ListKartsQueryDto {
  id: number;
  name?: string;
  colour?: string;
  manufacturer?: string;
  isEnabled: boolean;
  imageUrl?: string;
  pricePerSession?: number;
  description?: string;
}

export interface GetKartByIdQueryDto {
  id: number;
  name?: string;
  colour?: string;
  yearOfManufacture?: number;
  chassisNumber?: string;
  manufacturer?: string;
  powertrainTypeId?: number | null;
    powertrainTypeName?: string;
  isEnabled: boolean;
  imageUrl?: string;
  pricePerSession?: number;
  description?: string;
}

export class ListKartsRequest extends BasePagedQuery {
  search?: string;
  onlyEnabled?: boolean;
}

export interface CreateKartCommand {
  name?: string;
  colour?: string;
  yearOfManufacture?: number;
  chassisNumber?: string;
  manufacturer?: string;
  powertrainTypeId?: number | null;
  imageUrl?: string;
  pricePerSession?: number | null;
  description?: string;
}

export interface UpdateKartCommand extends CreateKartCommand {}

export type { PageResult };
