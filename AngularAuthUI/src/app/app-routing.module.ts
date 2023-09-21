import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './components/login/login.component';
import { SignupComponent } from './components/signup/signup.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { AuthGuard } from './guards/auth.guard';
import { ResetComponent } from './components/reset/reset.component';

const routes: Routes = [
  {path: '', redirectTo: 'login',  pathMatch: 'full'}, // mặc định nếu vào 4200 sẽ redirect về 4200/login
  {path:'login', component: LoginComponent},
  {path:'signup', component: SignupComponent},
  {path:'dashboard', component: DashboardComponent, canActivate:[AuthGuard]}, // canActivate:[AuthGuard] nghĩa là người dùng muốn vào page dashboard này phải được guard trả về true (tức có login và có token)
  {path:'reset', component: ResetComponent}
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
