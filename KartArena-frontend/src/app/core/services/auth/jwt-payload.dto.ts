/**
 * Represents the payload contained in the JWT access token.
 */
export type UserRole = 'Admin' | 'Manager' | 'Employee' | 'User';

export interface JwtPayloadDto {
  sub: string;

  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress': string;
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': UserRole;

  iat: number;
  exp: number;
  aud: string | string[];
  iss: string;
}