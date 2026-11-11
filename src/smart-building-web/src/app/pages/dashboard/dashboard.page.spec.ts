import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { AccessEvent, CurrentOccupancyFloor } from '../../admin/admin-api.service';
import { AuthService } from '../../auth/auth.service';
import { DashboardPage } from './dashboard.page';
import { DashboardService } from './dashboard.service';

const hub = vi.hoisted(() => ({
  handlers: new Map<string, (payload: never) => void>(),
  start: vi.fn<() => Promise<void>>(),
  stop: vi.fn<() => Promise<void>>(),
  off: vi.fn(),
  withUrl: vi.fn(),
  reconnecting: undefined as (() => void) | undefined,
  reconnected: undefined as (() => void) | undefined,
  close: undefined as (() => void) | undefined,
}));

vi.mock('@microsoft/signalr', () => ({
  LogLevel: { None: 6 },
  HubConnectionBuilder: class {
    withUrl(...args: unknown[]) { hub.withUrl(...args); return this; }
    withAutomaticReconnect() { return this; }
    configureLogging() { return this; }
    build() {
      return {
        start: hub.start,
        stop: hub.stop,
        on: (name: string, handler: (payload: never) => void) => hub.handlers.set(name, handler),
        off: hub.off,
        onreconnecting: (handler: () => void) => { hub.reconnecting = handler; },
        onreconnected: (handler: () => void) => { hub.reconnected = handler; },
        onclose: (handler: () => void) => { hub.close = handler; },
      };
    }
  },
}));

const accessEvent = (id: string, overrides: Partial<AccessEvent> = {}): AccessEvent => ({
  id,
  accessPointId: 'access-point',
  accessCardId: null,
  userId: null,
  occurredAt: '2026-01-01T10:00:00Z',
  direction: 'Entry',
  result: 'Granted',
  reason: id,
  ...overrides,
});

const floor: CurrentOccupancyFloor = {
  buildingId: 'building',
  buildingName: 'Headquarters',
  floorId: 'floor',
  floorName: 'Live floor',
  occupantCount: 1,
  occupants: [{
    userId: 'user',
    userName: 'Live occupant',
    enteredAt: '2026-01-01T10:00:00Z',
    durationSeconds: 120,
  }],
};

function emit(name: string, payload: AccessEvent | CurrentOccupancyFloor[]): void {
  hub.handlers.get(name)!(payload as never);
}

const sessionKey = 'smart-building-session';

describe('DashboardPage', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    vi.clearAllMocks();
    hub.handlers.clear();
    hub.start.mockImplementation(() => new Promise<void>(() => {}));
    hub.stop.mockResolvedValue(undefined);
    sessionStorage.setItem(
      sessionKey,
      JSON.stringify({
        accessToken: 'test-token',
        expiresAt: new Date(Date.now() + 60_000).toISOString(),
        name: 'Alex Operator',
        role: 'SecurityOperator',
      }),
    );
    TestBed.configureTestingModule({
      imports: [DashboardPage],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
    TestBed.resetTestingModule();
    sessionStorage.removeItem(sessionKey);
  });

  it('shows current occupancy by floor and each active session duration', () => {
    const fixture = TestBed.createComponent(DashboardPage);
    const root = fixture.nativeElement as HTMLElement;
    fixture.detectChanges();

    http.expectOne('/api/access/events').flush([]);
    http.expectOne('/api/occupancy/current').flush([
      {
        buildingId: 'building-id',
        buildingName: 'Headquarters',
        floorId: 'floor-id',
        floorName: 'Floor 2',
        occupantCount: 1,
        occupants: [
          {
            userId: 'user-id',
            userName: 'Alex',
            enteredAt: '2026-01-01T10:00:00Z',
            durationSeconds: 2520,
          },
        ],
      },
      {
        buildingId: 'building-id',
        buildingName: 'Headquarters',
        floorId: 'empty-floor-id',
        floorName: 'Floor 1',
        occupantCount: 0,
        occupants: [],
      },
    ]);
    fixture.detectChanges();

    expect(root.textContent).toContain('1 pessoa no edifício');
    expect(root.textContent).toContain('Floor 2');
    expect(root.textContent).toContain('Alex');
    expect(root.textContent).toContain('42 min');
    expect(root.textContent).toContain('Sem ocupantes neste piso.');
  });

  it('uses the existing JWT token factory and hub URL', () => {
    const fixture = TestBed.createComponent(DashboardPage);
    fixture.detectChanges();
    http.expectOne('/api/access/events').flush([]);
    http.expectOne('/api/occupancy/current').flush([]);
    expect(hub.withUrl).toHaveBeenCalledWith('/hubs/access', {
      accessTokenFactory: expect.any(Function),
    });
    const factory = hub.withUrl.mock.calls[0][1].accessTokenFactory as () => string;
    expect(factory()).toBe('test-token');
    vi.spyOn(TestBed.inject(AuthService), 'getToken').mockReturnValue('updated-token');
    expect(factory()).toBe('updated-token');
  });

  it('updates live history and full counts in place, sorted, capped and deduplicated', () => {
    const fixture = TestBed.createComponent(DashboardPage);
    fixture.detectChanges();
    const dashboard = fixture.debugElement.injector.get(DashboardService);
    http.expectOne('/api/access/events').flush(
      Array.from({ length: 12 }, (_, i) => accessEvent(`initial-${i}`)),
    );
    http.expectOne('/api/occupancy/current').flush([]);

    const event = accessEvent('new denial', {
      occurredAt: '2026-01-02T10:00:00Z', result: 'Denied',
    });
    emit('AccessEventCreated', event);
    emit('AccessEventCreated', event);
    fixture.detectChanges();

    expect(dashboard.eventCounts()).toEqual({ total: 13, granted: 12, denied: 1 });
    expect(dashboard.events()).toHaveLength(10);
    expect(dashboard.events()[0]).toEqual(event);
    expect(fixture.nativeElement.querySelector('tbody tr').textContent).toContain('new denial');
    expect(fixture.nativeElement.textContent).toContain('Negado');
    http.expectNone('/api/access/events');
  });

  it('replaces the full occupancy snapshot including occupants, durations and empty arrays', () => {
    const fixture = TestBed.createComponent(DashboardPage);
    fixture.detectChanges();
    http.expectOne('/api/access/events').flush([]);
    http.expectOne('/api/occupancy/current').flush([]);
    emit('OccupancyChanged', [floor]);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('1 pessoa no edifício');
    expect(fixture.nativeElement.textContent).toContain('Live occupant');
    expect(fixture.nativeElement.textContent).toContain('2 min');

    emit('OccupancyChanged', [{ ...floor, occupantCount: 0, occupants: [] }]);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).not.toContain('Live occupant');
    expect(fixture.nativeElement.textContent).toContain('Sem ocupantes neste piso.');
    emit('OccupancyChanged', []);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('0 pessoas no edifício');
    expect(fixture.nativeElement.textContent).toContain('Ainda não existem pisos');
    http.expectNone('/api/occupancy/current');
  });

  it('does not overwrite live changes with a late REST snapshot or double-count overlaps', () => {
    const fixture = TestBed.createComponent(DashboardPage);
    fixture.detectChanges();
    const event = accessEvent('overlap');
    emit('AccessEventCreated', event);
    emit('AccessEventCreated', accessEvent('live-only'));
    emit('OccupancyChanged', []);
    http.expectOne('/api/access/events').flush([event]);
    http.expectOne('/api/occupancy/current').flush([floor]);
    const dashboard = fixture.debugElement.injector.get(DashboardService);
    expect(dashboard.eventCounts().total).toBe(2);
    expect(dashboard.occupancy()).toEqual([]);
  });

  it('retains the admin REST summary behavior', () => {
    const session = JSON.parse(sessionStorage.getItem(sessionKey)!);
    sessionStorage.setItem(sessionKey, JSON.stringify({ ...session, role: 'Admin' }));
    const fixture = TestBed.createComponent(DashboardPage);
    fixture.detectChanges();
    http.expectOne('/api/access/events').flush([]);
    http.expectOne('/api/occupancy/current').flush([]);
    http.expectOne('/api/buildings/').flush([{}]);
    http.expectOne('/api/floors/').flush([{}, {}]);
    http.expectOne('/api/access-points/').flush([{}, {}, {}]);
    expect(fixture.debugElement.injector.get(DashboardService).counts()).toEqual({
      buildings: 1, floors: 2, accessPoints: 3,
    });
  });

  it('shows connection errors without losing REST data and allows retry and reconnect reconciliation', async () => {
    hub.start.mockRejectedValueOnce(new Error('negotiation failed'));
    const fixture = TestBed.createComponent(DashboardPage);
    fixture.detectChanges();
    http.expectOne('/api/access/events').flush([accessEvent('rest-event')]);
    http.expectOne('/api/occupancy/current').flush([]);
    await fixture.whenStable();
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Não foi possível ligar às atualizações');
    expect(fixture.nativeElement.textContent).toContain('rest-event');

    hub.start.mockResolvedValueOnce(undefined);
    (fixture.nativeElement.querySelector('.live-status button') as HTMLButtonElement).click();
    await fixture.whenStable();
    http.expectOne('/api/access/events').flush([accessEvent('rest-event')]);
    http.expectOne('/api/occupancy/current').flush([]);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Atualizações em tempo real ativas.');

    hub.reconnecting!();
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('A restabelecer');
    hub.reconnected!();
    emit('AccessEventCreated', accessEvent('during-reconciliation'));
    http.expectOne('/api/access/events').flush([accessEvent('missed-event')]);
    http.expectOne('/api/occupancy/current').flush([floor]);
    const dashboard = fixture.debugElement.injector.get(DashboardService);
    expect(dashboard.eventCounts().total).toBe(2);
    expect(dashboard.occupancy()).toEqual([floor]);
    hub.close!();
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Atualizações em tempo real desligadas.');
  });

  it('preserves REST error messages independently of live connection failures', async () => {
    hub.start.mockRejectedValueOnce(new Error('offline'));
    const fixture = TestBed.createComponent(DashboardPage);
    fixture.detectChanges();
    const occupancy = http.expectOne('/api/occupancy/current');
    http.expectOne('/api/access/events').flush({}, { status: 401, statusText: 'Unauthorized' });
    expect(occupancy.cancelled).toBe(true);
    await fixture.whenStable();
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('A sessão expirou');
    expect(fixture.nativeElement.textContent).toContain('Não foi possível ligar às atualizações');
  });

  it('removes handlers, cancels REST and stops a pending connection on destroy', async () => {
    let finishStart!: () => void;
    hub.start.mockImplementationOnce(() => new Promise<void>((resolve) => { finishStart = resolve; }));
    const fixture = TestBed.createComponent(DashboardPage);
    fixture.detectChanges();
    const dashboard = fixture.debugElement.injector.get(DashboardService);
    const events = http.expectOne('/api/access/events');
    const occupancy = http.expectOne('/api/occupancy/current');
    fixture.destroy();
    expect(events.cancelled).toBe(true);
    expect(occupancy.cancelled).toBe(true);
    expect(hub.off).toHaveBeenCalledWith('AccessEventCreated', hub.handlers.get('AccessEventCreated'));
    expect(hub.off).toHaveBeenCalledWith('OccupancyChanged', hub.handlers.get('OccupancyChanged'));
    expect(hub.stop).toHaveBeenCalledOnce();
    emit('AccessEventCreated', accessEvent('after-destroy'));
    emit('OccupancyChanged', [floor]);
    hub.reconnected!();
    finishStart();
    await Promise.resolve();
    expect(dashboard.events()).toEqual([]);
    expect(dashboard.occupancy()).toEqual([]);
    http.expectNone('/api/access/events');
  });
});
