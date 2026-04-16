import 'reflect-metadata';
import { jsonObject, jsonMember, TypedJSON } from 'typedjson';

@jsonObject
export class ResetPasswordModel {
    @jsonMember
    email: string;

    @jsonMember
    token: string;

    @jsonMember
    newPassword: string;

    constructor(pEmail: string, pToken: string, pNewPassword: string) {
        this.email = pEmail;
        this.token = pToken;
        this.newPassword = pNewPassword;
    }
}
