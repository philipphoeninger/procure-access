import 'reflect-metadata';
import { jsonObject, jsonMember, TypedJSON } from 'typedjson';

@jsonObject
export class User {
  @jsonMember
  email: string;

  @jsonMember
  twoFAEnabled: boolean;

  constructor(pEmail: string, pTwoFAEnabled: boolean) {
    this.email = pEmail;
    this.twoFAEnabled = pTwoFAEnabled;
  }
}