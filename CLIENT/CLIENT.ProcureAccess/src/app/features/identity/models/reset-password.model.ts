import 'reflect-metadata';
import { jsonObject, jsonMember, TypedJSON } from 'typedjson';

@jsonObject
export class ResetPasswordModel {
    @jsonMember
    email: string;

    @jsonMember
    token: string;

    @jsonMember
    password: string;

    constructor(pEmail: string, pToken: string, pPassword: string) {
        this.email = pEmail;
        this.token = pToken;
        this.password = pPassword;
    }
}
