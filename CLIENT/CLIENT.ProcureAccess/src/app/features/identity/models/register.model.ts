import 'reflect-metadata';
import { jsonObject, jsonMember, TypedJSON } from 'typedjson';

@jsonObject
export class RegisterModel {
  @jsonMember
  email: string;

  @jsonMember
  username: string;

  @jsonMember
  password: string;

  constructor(pEmail: string, pUsername: string, pPassword: string) {
    this.email = pEmail;
    this.username = pUsername;
    this.password = pPassword;
  }
}

