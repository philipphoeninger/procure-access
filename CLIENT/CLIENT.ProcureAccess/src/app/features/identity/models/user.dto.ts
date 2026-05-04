import 'reflect-metadata';
import { jsonObject, jsonMember, TypedJSON } from 'typedjson';

@jsonObject
export class UserDto {
  @jsonMember
  id: string;

  @jsonMember
  email: string;

  constructor(pId: string, pEmail: string) {
    this.id = pId;
    this.email = pEmail;
  }
}
