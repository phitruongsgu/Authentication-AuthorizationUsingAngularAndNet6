import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import ValidateForm from 'src/app/helpers/validateForm';
import { AuthService } from 'src/app/services/auth.service';
import { ResetPasswordService } from 'src/app/services/reset-password.service';
import { UserStoreService } from 'src/app/services/user-store.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {

  type:string = "password";
  isText:boolean = false;
  eyeIcon:string = "fa fa-eye-slash";
  loginForm!: FormGroup;

  public resetPasswordByEmail!:string;
  public isValidEmail!:boolean;

  constructor(private fb: FormBuilder, private auth: AuthService, private router: Router, private toastr: ToastrService, private userStore: UserStoreService,
    private resetService: ResetPasswordService

    ) { }

  ngOnInit(): void {
    this.loginForm = this.fb.group({
      username: ['', Validators.required],
      password: ['', Validators.required]
    })
  }

  hideShowPass(){
    this.isText = !this.isText;
    this.isText ? this.eyeIcon = "fa-eye" : this.eyeIcon = "fa-eye-slash";
    this.isText ? this.type = "text" : this.type = "password";
  }

  onLogin(){
    if(this.loginForm.valid){
      console.log("Value from FORM: ", this.loginForm.value);
      this.auth.login(this.loginForm.value).subscribe({
        next:(res)=>{
          //alert(res.message);
          this.toastr.success(res.message, 'SUCCESS');
          this.loginForm.reset();
          this.auth.storeToken(res.accessToken);
          this.auth.storeRefreshToken(res.refreshToken);
          const tokenPayload = this.auth.decodedToken();
          this.userStore.setFullNameForStore(tokenPayload.name);
          this.userStore.setRoleForStore(tokenPayload.role);
          this.router.navigate(['dashboard']);
        },
        error:(err)=>{
          // alert(err?.error.message);
          this.toastr.error(err?.error.message, 'ERROR');
        }
      });
    }else{
      console.log("Form is not valid!");
      ValidateForm.validateAllFormFields(this.loginForm);
      alert("Form is not valid!");
    }
  }


  checkValidEmail(event:string){
    const value = event;
    const pattern = /^[\w-\.]+@([\w-]+\.)+[\w-]{2,3}$/;
    this.isValidEmail = pattern.test(value);
    return this.isValidEmail;
  }

  confirmToSend(){
    if(this.checkValidEmail(this.resetPasswordByEmail)){
      console.log("Reset Password By Email: ", this.resetPasswordByEmail);


      this.resetService.sendResetPasswordLink(this.resetPasswordByEmail).subscribe({
        next: (res) => {
          this.toastr.success("Reset success!", 'Success');
          this.resetPasswordByEmail = "";
          const buttonRef = document.getElementById("closeBtn");
          buttonRef?.click();
        },
        error: (err) => {
          this.toastr.error("Something went wrong!", 'ERROR');
        }
    });
    }
  }
}
