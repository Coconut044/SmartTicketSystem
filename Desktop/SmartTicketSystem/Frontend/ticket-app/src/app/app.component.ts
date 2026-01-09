import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterOutlet, NavigationEnd } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatBadgeModule } from '@angular/material/badge';
import { MatDividerModule } from '@angular/material/divider';
import { MatSidenav, MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { RouterModule } from '@angular/router';
import { filter } from 'rxjs/operators';

import { AuthService } from './core/services/auth.service';
import { NotificationService } from './core/services/notification.service';
import { Notification, User } from './models';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    RouterModule,
    MatToolbarModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatBadgeModule,
    MatDividerModule,
    MatSidenavModule,
    MatListModule,
  ],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {

  @ViewChild('sidenav') sidenav!: MatSidenav;

  user: User | null = null;
  isAuth = false;
  isMobile = false;

  // 🔔 Notifications
  unreadCount = 0;
  notifications: Notification[] = [];

  constructor(
    private auth: AuthService,
    private notificationSvc: NotificationService,
    private router: Router
  ) {
    this.checkScreenSize();
    if (typeof window !== 'undefined') {
      window.addEventListener('resize', () => this.checkScreenSize());
    }
  }

  ngOnInit(): void {
    this.user = this.auth.getCurrentUser();
    this.isAuth = !!this.user;

    // 🔔 Load notifications ONLY if logged in
    if (this.isAuth) {
      this.notificationSvc.unreadCount$.subscribe(count => {
        this.unreadCount = count;
      });

      // initial fetch
      this.notificationSvc.getUnreadCount().subscribe();
    }

    // Auto-close sidenav on mobile after navigation
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe(() => {
        if (this.isMobile && this.sidenav) {
          this.sidenav.close();
        }
      });
  }

  checkScreenSize(): void {
    if (typeof window !== 'undefined') {
      this.isMobile = window.innerWidth < 768;
    }
  }

  isAuthPage(): boolean {
    const url = this.router.url;
    return url.startsWith('/login') || url.startsWith('/register');
  }

  hasRole(roles: string[]): boolean {
    if (!this.user) return false;
    return roles.includes(this.user.role);
  }

  onNavClick(): void {
    if (this.isMobile && this.sidenav) {
      this.sidenav.close();
    }
  }

  // 🔔 Open notification menu
  openNotifications(): void {
    this.notificationSvc.getNotifications().subscribe(res => {
      if (res.success && res.data) {
        this.notifications = res.data;
      }
    });
  }

  // 🔔 Mark single notification as read
  markAsRead(id: number): void {
    this.notificationSvc.markAsRead(id).subscribe();
  }

  logout(): void {
    this.auth.logout();
    this.user = null;
    this.isAuth = false;
    this.router.navigate(['/login']);
  }
}
