import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Injectable({ providedIn: 'root' })
export class RoleGuard implements CanActivate {
  constructor(private auth: AuthService, private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot): boolean {
    const allowedRoles = route.data['roles'] as string[];
    
    if (!this.auth.isAuthenticated()) {
      this.router.navigate(['/login']);
      return false;
    }
    
    if (this.auth.hasRole(allowedRoles)) {
      return true;
    }
    
    // User doesn't have required role - redirect to their appropriate page
    const userRole = this.auth.getUserRole();
    
    if (userRole === 'Admin') {
      this.router.navigate(['/admin']);
    } else if (userRole === 'SupportManager') {
      this.router.navigate(['/reports']);
    } else if (userRole === 'SupportAgent') {
      this.router.navigate(['/tickets']);
    } else {
      this.router.navigate(['/my-tickets']);
    }
    
    return false;
  }
}