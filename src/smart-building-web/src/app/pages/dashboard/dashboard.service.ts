import { HttpErrorResponse } from '@angular/common/http';
import { Injectable, OnDestroy, inject, signal } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { Subscription, forkJoin, of } from 'rxjs';
import { AccessEvent, AdminApiService, CurrentOccupancyFloor } from '../../admin/admin-api.service';
import { AuthService } from '../../auth/auth.service';

type LiveState = 'connecting' | 'connected' | 'reconnecting' | 'disconnected' | 'error';

/** Dashboard-scoped: owns REST snapshots, live updates and their lifetime. */
@Injectable()
export class DashboardService implements OnDestroy {
  private readonly api = inject(AdminApiService);
  private readonly auth = inject(AuthService);
  readonly events = signal<AccessEvent[]>([]);
  readonly eventCounts = signal({ total: 0, granted: 0, denied: 0 });
  readonly counts = signal({ buildings: 0, floors: 0, accessPoints: 0 });
  readonly occupancy = signal<CurrentOccupancyFloor[]>([]);
  readonly loading = signal(true);
  readonly errorMessage = signal('');
  readonly liveState = signal<LiveState>('connecting');

  private connection?: HubConnection;
  private snapshot?: Subscription;
  private destroyed = false;
  private starting = false;
  private snapshotPending = false;
  private pendingEvents: AccessEvent[] = [];
  private pendingOccupancy: CurrentOccupancyFloor[] | null = null;
  private readonly eventHistory = new Map<string, AccessEvent>();

  start(): void {
    if (this.connection || this.destroyed) return;
    this.connection = new HubConnectionBuilder()
      .withUrl('/hubs/access', {
        accessTokenFactory: () => this.auth.getToken() ?? '',
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.None)
      .build();
    this.connection.on('AccessEventCreated', this.onAccessEvent);
    this.connection.on('OccupancyChanged', this.onOccupancy);
    this.connection.onreconnecting(() => {
      if (!this.destroyed) this.liveState.set('reconnecting');
    });
    this.connection.onreconnected(() => {
      if (this.destroyed) return;
      this.liveState.set('connected');
      // Reconcile changes missed while disconnected; the hub does not replay them.
      this.loadSnapshot();
    });
    this.connection.onclose(() => {
      if (!this.destroyed) this.liveState.set('disconnected');
    });
    // REST remains available even if negotiation fails.
    this.loadSnapshot();
    void this.connect();
  }

  async connect(): Promise<void> {
    if (
      !this.connection ||
      this.destroyed ||
      this.starting ||
      this.liveState() === 'connected' ||
      this.liveState() === 'reconnecting'
    ) {
      return;
    }
    this.starting = true;
    this.liveState.set('connecting');
    try {
      await this.connection.start();
      if (this.destroyed) return;
      this.liveState.set('connected');
      // Close the gap between the initial REST request and hub negotiation.
      this.loadSnapshot();
    } catch {
      if (!this.destroyed) this.liveState.set('error');
    } finally {
      this.starting = false;
    }
  }

  ngOnDestroy(): void {
    this.destroyed = true;
    this.snapshot?.unsubscribe();
    this.connection?.off('AccessEventCreated', this.onAccessEvent);
    this.connection?.off('OccupancyChanged', this.onOccupancy);
    // stop() also cancels negotiation and automatic reconnect.
    void this.connection?.stop().catch(() => {
      console.error('Não foi possível encerrar a ligação de atualizações em tempo real.');
    });
  }

  private readonly onAccessEvent = (event: AccessEvent): void => {
    if (this.destroyed) return;
    if (this.snapshotPending) this.pendingEvents.push(event);
    this.eventHistory.set(event.id, event);
    this.publishEvents();
  };

  private readonly onOccupancy = (occupancy: CurrentOccupancyFloor[]): void => {
    if (this.destroyed) return;
    if (this.snapshotPending) this.pendingOccupancy = occupancy;
    // This payload is the complete current-occupancy array, including empty arrays.
    this.occupancy.set(occupancy);
  };

  private loadSnapshot(): void {
    this.snapshot?.unsubscribe();
    this.snapshotPending = true;
    this.pendingEvents = [];
    this.pendingOccupancy = null;
    const isAdmin = this.auth.getSession()?.role === 'Admin';
    this.snapshot = forkJoin({
      events: this.api.getAccessEvents(),
      occupancy: this.api.getCurrentOccupancy(),
      buildings: isAdmin ? this.api.getBuildings() : of([]),
      floors: isAdmin ? this.api.getFloors() : of([]),
      accessPoints: isAdmin ? this.api.getAccessPoints() : of([]),
    }).subscribe({
      next: ({ events, occupancy, buildings, floors, accessPoints }) => {
        this.eventHistory.clear();
        for (const event of [...events, ...this.pendingEvents]) {
          this.eventHistory.set(event.id, event);
        }
        this.publishEvents();
        this.occupancy.set(this.pendingOccupancy ?? occupancy);
        this.counts.set({
          buildings: buildings.length,
          floors: floors.length,
          accessPoints: accessPoints.length,
        });
        this.errorMessage.set('');
        this.finishSnapshot();
      },
      error: (error: HttpErrorResponse) => {
        this.finishSnapshot();
        if (error.status === 401) {
          this.errorMessage.set('A sessão expirou. Inicie sessão novamente.');
        } else if (error.status === 403) {
          this.errorMessage.set('A sua conta não tem acesso ao histórico de operações.');
        } else if (error.status === 0) {
          this.errorMessage.set('Não foi possível ligar à API. Verifique a ligação e tente novamente.');
        } else {
          this.errorMessage.set(`Não foi possível carregar o painel (HTTP ${error.status}).`);
        }
      },
    });
  }

  private finishSnapshot(): void {
    this.snapshotPending = false;
    this.pendingEvents = [];
    this.pendingOccupancy = null;
    this.loading.set(false);
  }

  private publishEvents(): void {
    const events = [...this.eventHistory.values()];
    const granted = events.filter((event) => event.result === 'Granted').length;
    this.eventCounts.set({ total: events.length, granted, denied: events.length - granted });
    this.events.set(
      events
        .sort((left, right) => Date.parse(right.occurredAt) - Date.parse(left.occurredAt))
        .slice(0, 10),
    );
  }
}
