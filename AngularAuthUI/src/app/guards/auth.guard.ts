import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { ToastrService } from 'ngx-toastr';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {

  constructor(private auth: AuthService, private router: Router, private toastr: ToastrService){

  }

  canActivate(): boolean{
    if(this.auth.isLoggedIn()){
      return true; // cho phép người dùng có thể truy cập các trang khác
    }else{
      this.toastr.error('Please log in first!', 'ERROR');
      this.router.navigate(['login']); // người dùng chưa login thì chưa có token, sẽ quay về trang login
      return false;
    }

  }

}
