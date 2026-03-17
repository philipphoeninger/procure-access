import 'reflect-metadata';
import { jsonObject, jsonMember, TypedJSON } from 'typedjson';

@jsonObject
export class LoginModel {
  @jsonMember
  email: string;

  @jsonMember
  password: string;

  constructor(
    pEmail: string,
    pPassword: string,
  ) {
    this.email = pEmail;
    this.password = pPassword;
  }
}

