import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';
import { DashboardPage } from './dashboard.page';

const sessionKey = 'smart-building-session';

describe('DashboardPage', () => {
  let http: HttpTestingController;

  beforeEach(() => {
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
});
