import 'reflect-metadata';
import { jsonObject, jsonMember, TypedJSON } from 'typedjson';
import { UICustomization } from '@app/features/settings/models/uiCustomization.model';

@jsonObject
export class User {
  @jsonMember
  id: string;

  @jsonMember
  email: string;

  @jsonMember
  uiCustomization: UICustomization;

  // @jsonMember
  // twoFAEnabled: boolean;

  constructor(pId: string, pEmail: string, pUICustomization: UICustomization) {
    this.id = pId;
    this.email = pEmail;
    this.uiCustomization = pUICustomization;
    // this.twoFAEnabled = pTwoFAEnabled;
  }
}
