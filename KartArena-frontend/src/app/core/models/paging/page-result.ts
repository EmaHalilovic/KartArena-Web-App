// src/app/core/models/page-result.ts

export interface PageResult<T> {
  items: T[];
   total?: number;

  /** Legacy/derived paging fields retained for existing frontend adapters. */
  pageSize?: number;
  currentPage?: number;
  includedTotal?: boolean;
  totalItems?: number;
  totalPages?: number;
}
