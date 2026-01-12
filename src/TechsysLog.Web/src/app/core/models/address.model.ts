// ============================================================================
// Address / CEP Domain Models
// ============================================================================

export interface AddressLookup {
  readonly cep: string;
  readonly street: string;
  readonly neighborhood: string;
  readonly city: string;
  readonly state: string;
}

export interface AddressLookupResponse {
  readonly success: boolean;
  readonly data?: AddressLookup;
  readonly message?: string;
}
