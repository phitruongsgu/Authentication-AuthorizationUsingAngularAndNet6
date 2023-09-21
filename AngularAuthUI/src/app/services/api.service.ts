import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ApiService {

  private baseUrl: string = "https://localhost:7030/api/User/";
  constructor(private httpClient: HttpClient) { }

  getUsers(){
    return this.httpClient.get<any>(this.baseUrl);
  }
}
