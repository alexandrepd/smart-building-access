import { DatePipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../auth/auth.service';
import { DashboardService } from './dashboard.service';

@Component({
  selector: 'app-dashboard-page',
  imports: [DatePipe, RouterLink],
  providers: [DashboardService],
  templateUrl: './dashboard.page.html',
  styleUrl: './dashboard.page.scss',
})
export class DashboardPage implements OnInit {
  protected readonly auth = inject(AuthService);
  protected readonly dashboard = inject(DashboardService);
  protected readonly session = this.auth.getSession();
  protected readonly isAdmin = this.session?.role === 'Admin';
  protected readonly events = this.dashboard.events;
  protected readonly eventCounts = this.dashboard.eventCounts;
  protected readonly counts = this.dashboard.counts;
  protected readonly occupancy = this.dashboard.occupancy;
  protected readonly loading = this.dashboard.loading;
  protected readonly errorMessage = this.dashboard.errorMessage;

  ngOnInit(): void {
    this.dashboard.start();
  }

  protected grantedCount(): number {
    return this.eventCounts().granted;
  }

  protected deniedCount(): number {
    return this.eventCounts().denied;
  }

  protected shortId(id: string): string {
    return id.slice(0, 8);
  }

  protected currentOccupantCount(): number {
    return this.occupancy().reduce((total, floor) => total + floor.occupantCount, 0);
  }

  protected formatDuration(seconds: number): string {
    const hours = Math.floor(seconds / 3600);
    const minutes = Math.floor((seconds % 3600) / 60);
    if (hours > 0) return `${hours} h ${minutes} min`;
    if (minutes > 0) return `${minutes} min`;
    return `${seconds} s`;
  }
}
