import 'reflect-metadata';
import { jsonObject, jsonMember, TypedJSON } from 'typedjson';

@jsonObject
export class User {
  @jsonMember
  email: string;

  @jsonMember
  username: string;

  @jsonMember
  twoFAEnabled: boolean;

  constructor(pEmail: string, pUsername: string, pTwoFAEnabled: boolean) {
    this.email = pEmail;
    this.username = pUsername;
    this.twoFAEnabled = pTwoFAEnabled;
  }
}