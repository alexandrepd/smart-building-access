import { Routes } from '@angular/router';
import { adminGuard } from './auth/admin.guard';
import { authenticatedGuard } from './auth/auth.guard';
import { ManagementPage } from './admin/management.page';
import { AccessRequestPage } from './pages/access-request/access-request.page';
import { DashboardPage } from './pages/dashboard/dashboard.page';
import { LoginPage } from './pages/login/login.page';

export const routes: Routes = [
  { path: 'login', component: LoginPage },
  { path: 'dashboard', component: DashboardPage, canActivate: [authenticatedGuard] },
  { path: 'access-request', component: AccessRequestPage, canActivate: [authenticatedGuard] },
  { path: 'management', component: ManagementPage, canActivate: [authenticatedGuard, adminGuard] },
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  { path: '**', redirectTo: 'dashboard' },
];
