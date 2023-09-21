import { Injectable } from '@angular/core';
import {HttpClient} from '@angular/common/http';
import { Router } from '@angular/router';
import { JwtHelperService } from '@auth0/angular-jwt';
import { TokenApiModel } from '../models/token-api.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private baseUrl:string = "https://localhost:7030/api/User/";
  private userPayload:any;

  constructor(private httpClient: HttpClient, private router: Router) {
    this.userPayload = this.decodedToken();
  }

  signUp(userModel:any){
    return this.httpClient.post<any>(`${this.baseUrl}register`,userModel);
  }

  login(userModel:any){
    return this.httpClient.post<any>(`${this.baseUrl}authenticate`,userModel);
  }

  signOut(){
    localStorage.clear();
    //localStorage.removeItem('token');
    this.router.navigate(['login']);
  }

  // Set and Get Token after login
  storeToken(tokenValue: string){
    localStorage.setItem('token', tokenValue);
  }

  storeRefreshToken(tokenValue: string){
    localStorage.setItem('refreshToken', tokenValue);
  }

  getToken(){
    return localStorage.getItem('token');
  }

  getRefreshToken(){
    return localStorage.getItem('refreshToken');
  }

  isLoggedIn(): boolean{
    return !!localStorage.getItem('token'); // 2 cái ! là convert từ string -> boolean
  }

  decodedToken(){
    const jwtHelper = new JwtHelperService();
    const token = this.getToken()!;
    console.log('jwtHelper.decodeToken(token): ', jwtHelper.decodeToken(token));

    return jwtHelper.decodeToken(token);
  }

  getFullNameFromToken(){
    if(this.userPayload){
      return this.userPayload.name;
    }
  }

  getRoleFromToken(){
    if(this.userPayload){
      return this.userPayload.role;
    }
  }

  renewToken(tokenApi : TokenApiModel){
    return this.httpClient.post<any>(`${this.baseUrl}refresh`, tokenApi);
  }
}
