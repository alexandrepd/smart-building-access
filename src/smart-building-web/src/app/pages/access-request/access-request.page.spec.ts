import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';
import { AccessRequestPage } from './access-request.page';

const sessionKey = 'smart-building-session';

describe('AccessRequestPage', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    sessionStorage.setItem(
      sessionKey,
      JSON.stringify({
        accessToken: 'test-token',
        expiresAt: new Date(Date.now() + 60_000).toISOString(),
        name: 'Alex',
        role: 'SecurityOperator',
      }),
    );
    TestBed.configureTestingModule({
      imports: [AccessRequestPage],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
    sessionStorage.removeItem(sessionKey);
  });

  it('submits an access request and shows the recorded decision', () => {
    const fixture = TestBed.createComponent(AccessRequestPage);
    const root = fixture.nativeElement as HTMLElement;
    fixture.detectChanges();

    http.expectOne('/api/access/events').flush([]);
    fixture.detectChanges();

    setInput(root, '#card-number', 'CARD-001');
    setInput(root, '#access-point-id', '01234567-89ab-cdef-0123-456789abcdef');
    fixture.detectChanges();
    root.querySelector<HTMLFormElement>('form')!.requestSubmit();

    const request = http.expectOne('/api/access/requests');
    expect(request.request.body).toEqual({
      cardNumber: 'CARD-001',
      accessPointId: '01234567-89ab-cdef-0123-456789abcdef',
      direction: 'Entry',
    });
    request.flush({
      id: 'event-id',
      accessPointId: '01234567-89ab-cdef-0123-456789abcdef',
      accessCardId: null,
      userId: null,
      occurredAt: '2026-01-01T10:00:00Z',
      direction: 'Entry',
      result: 'Denied',
      reason: 'Card not found.',
    });
    fixture.detectChanges();

    expect(root.textContent).toContain('Acesso negado. A decisão foi registada.');
    expect(root.textContent).toContain('Card not found.');
    expect(root.querySelector<HTMLInputElement>('#card-number')?.value).toBe('');
  });

  it('offers Admin selections for existing cards and access points', () => {
    sessionStorage.setItem(
      sessionKey,
      JSON.stringify({
        accessToken: 'test-token',
        expiresAt: new Date(Date.now() + 60_000).toISOString(),
        name: 'Alex',
        role: 'Admin',
      }),
    );
    const fixture = TestBed.createComponent(AccessRequestPage);
    const root = fixture.nativeElement as HTMLElement;
    fixture.detectChanges();

    http.expectOne('/api/access/events').flush([]);
    http.expectOne('/api/cards/').flush([
      {
        id: 'card-id',
        userId: 'user-id',
        cardNumber: 'CARD-001',
        isActive: true,
        expiresAt: null,
        createdAt: '2026-01-01T00:00:00Z',
      },
    ]);
    http.expectOne('/api/access-points/').flush([
      {
        id: '01234567-89ab-cdef-0123-456789abcdef',
        floorId: 'floor-id',
        name: 'Entrada principal',
        location: 'Receção',
        isActive: true,
        supportsEntry: true,
        supportsExit: true,
        createdAt: '2026-01-01T00:00:00Z',
      },
    ]);
    fixture.detectChanges();

    expect(root.querySelector('#card-number')?.tagName).toBe('SELECT');
    expect(root.querySelector('#access-point-id')?.tagName).toBe('SELECT');
    expect(root.textContent).toContain('Entrada principal');
  });

  it('does not send a request when the point identifier is invalid', () => {
    const fixture = TestBed.createComponent(AccessRequestPage);
    const root = fixture.nativeElement as HTMLElement;
    fixture.detectChanges();
    http.expectOne('/api/access/events').flush([]);

    setInput(root, '#card-number', 'CARD-001');
    setInput(root, '#access-point-id', 'not-a-guid');
    fixture.detectChanges();
    root.querySelector<HTMLFormElement>('form')!.requestSubmit();
    fixture.detectChanges();

    http.expectNone('/api/access/requests');
    expect(root.textContent).toContain('Introduza um GUID válido.');
  });
});

function setInput(element: HTMLElement, selector: string, value: string): void {
  const input = element.querySelector<HTMLInputElement>(selector);
  input!.value = value;
  input!.dispatchEvent(new Event('input'));
}
