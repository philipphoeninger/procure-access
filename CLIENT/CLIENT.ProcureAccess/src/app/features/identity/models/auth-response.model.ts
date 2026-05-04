import 'reflect-metadata';
import { jsonObject, jsonMember, TypedJSON } from 'typedjson';
import { User } from './user.model';

@jsonObject
export class AuthResponse {
  @jsonMember
  accessToken: string;

  @jsonMember
  refreshToken: string;

  @jsonMember
  user: User;

  constructor(
    pAccessToken: string,
    pRefreshToken: string,
    pUser: User,
  ) {
    this.accessToken = pAccessToken;
    this.refreshToken = pRefreshToken;
    this.user = pUser;
  }
}
