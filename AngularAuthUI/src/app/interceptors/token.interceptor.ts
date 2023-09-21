import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { ToastrService } from 'ngx-toastr';
import { Router } from '@angular/router';
import { TokenApiModel } from '../models/token-api.model';

@Injectable()
export class TokenInterceptor implements HttpInterceptor {

  constructor(private auth: AuthService, private toastr: ToastrService, private router: Router) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {

    const myToken = this.auth.getToken();

    if(myToken){
      request = request.clone({
        setHeaders:{Authorization: `Bearer ${myToken}`}
      });
    }
    return next.handle(request).pipe(
      catchError((err:any)=>{
        if(err instanceof HttpErrorResponse){
          if(err.status === 401){
            // this.toastr.warning('Token is expired, please login again', 'WARNING');
            // this.router.navigate(['login']);
            return this.handleUnAuthorizedError(request, next);
          }
        }
        return throwError(()=> new Error("Some other error occured"));
      })
    );
  }

  handleUnAuthorizedError(request: HttpRequest<any>, next: HttpHandler){
    let tokenApiModel = new TokenApiModel();
    tokenApiModel.accessToken = this.auth.getToken()!;
    tokenApiModel.refreshToken = this.auth.getRefreshToken()!;
    return this.auth.renewToken(tokenApiModel).pipe(
      switchMap((data: TokenApiModel) => {
        this.auth.storeRefreshToken(data.refreshToken);
        this.auth.storeToken(data.accessToken);

        request = request.clone({
          setHeaders:{Authorization: `Bearer ${data.accessToken}`}
        });
        return next.handle(request);
      }),
      catchError((error) => { // nếu như 1 ai đó thay đổi refreshToken thì cho nó về trang login
        return throwError(() => {
          this.toastr.warning('Token is expired, please login again', 'WARNING');
          this.router.navigate(['login']);
        })
      })
    );
  }
}
