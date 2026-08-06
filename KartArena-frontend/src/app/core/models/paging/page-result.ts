// src/app/core/models/page-result.ts

export interface PageResult<T> {
  items: T[];
  /** Total number of matching records returned by the backend. */
  total?: number;

  /** Legacy/derived paging fields retained for existing frontend adapters. */
  pageSize?: number;
  currentPage?: number;
  includedTotal?: boolean;
  totalItems?: number;
  totalPages?: number;
}
