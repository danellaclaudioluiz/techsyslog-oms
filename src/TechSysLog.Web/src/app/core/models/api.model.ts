// ============================================================================
// API Response Models
// ============================================================================

export interface ApiResponse<T> {
  pagination: null;
  readonly success: boolean;
  readonly data?: T;
  readonly message?: string;
  readonly errors?: string[];
}

export interface PaginatedResponse<T> {
  readonly success: boolean;
  readonly data: T[];
  readonly pagination: {
    readonly cursor?: string;
    readonly hasMore: boolean;
    readonly limit: number;
  };
}

export interface ApiError {
  readonly status: number;
  readonly message: string;
  readonly errors?: string[];
}
