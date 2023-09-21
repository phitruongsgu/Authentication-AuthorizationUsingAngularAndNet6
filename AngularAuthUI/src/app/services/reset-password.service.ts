import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ResetPassword } from '../models/reset-passsword.model';

@Injectable({
  providedIn: 'root'
})
export class ResetPasswordService {

  private baseUrl: string = "https://localhost:7058/api/User";

  constructor(private HttpClient : HttpClient) { }

  sendResetPasswordLink(email: string){
    return this.HttpClient.post<any>(`${this.baseUrl}/send-reset-email/${email}`, {});

  }

  resetPassword(resetPasswordObj : ResetPassword){
    return this.HttpClient.post<any>(`${this.baseUrl}/reset-password`, resetPasswordObj);
  }
}
