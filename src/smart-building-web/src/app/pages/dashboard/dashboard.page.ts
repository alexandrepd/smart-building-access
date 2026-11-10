import { HttpErrorResponse } from '@angular/common/http';
import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { AccessEvent, AdminApiService } from '../../admin/admin-api.service';
import { AuthService } from '../../auth/auth.service';

@Component({
  selector: 'app-dashboard-page',
  imports: [DatePipe, RouterLink],
  templateUrl: './dashboard.page.html',
  styleUrl: './dashboard.page.scss',
})
export class DashboardPage implements OnInit {
  protected readonly auth = inject(AuthService);
  private readonly api = inject(AdminApiService);
  protected readonly session = this.auth.getSession();
  protected readonly isAdmin = this.session?.role === 'Admin';
  protected readonly events = signal<AccessEvent[]>([]);
  protected readonly eventCounts = signal({ total: 0, granted: 0, denied: 0 });
  protected readonly counts = signal({ buildings: 0, floors: 0, accessPoints: 0 });
  protected readonly loading = signal(true);
  protected readonly errorMessage = signal('');

  ngOnInit(): void {
    if (this.isAdmin) {
      forkJoin({
        events: this.api.getAccessEvents(),
        buildings: this.api.getBuildings(),
        floors: this.api.getFloors(),
        accessPoints: this.api.getAccessPoints(),
      }).subscribe({
        next: ({ events, buildings, floors, accessPoints }) => {
          this.setEvents(events);
          this.counts.set({
            buildings: buildings.length,
            floors: floors.length,
            accessPoints: accessPoints.length,
          });
          this.loading.set(false);
        },
        error: (error: HttpErrorResponse) => this.handleLoadError(error),
      });
      return;
    }

    this.api.getAccessEvents().subscribe({
      next: (events) => {
        this.setEvents(events);
        this.loading.set(false);
      },
      error: (error: HttpErrorResponse) => this.handleLoadError(error),
    });
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

  private sortEvents(events: AccessEvent[]): AccessEvent[] {
    return [...events]
      .sort((left, right) => Date.parse(right.occurredAt) - Date.parse(left.occurredAt))
      .slice(0, 10);
  }

  private setEvents(events: AccessEvent[]): void {
    this.eventCounts.set({
      total: events.length,
      granted: events.filter((event) => event.result === 'Granted').length,
      denied: events.filter((event) => event.result !== 'Granted').length,
    });
    this.events.set(this.sortEvents(events));
  }

  private handleLoadError(error: HttpErrorResponse): void {
    this.loading.set(false);
    if (error.status === 401) {
      this.errorMessage.set('A sessão expirou. Inicie sessão novamente.');
    } else if (error.status === 403) {
      this.errorMessage.set('A sua conta não tem acesso ao histórico de operações.');
    } else if (error.status === 0) {
      this.errorMessage.set('Não foi possível ligar à API. Verifique a ligação e tente novamente.');
    } else {
      this.errorMessage.set(`Não foi possível carregar o painel (HTTP ${error.status}).`);
    }
  }
}
