import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';
import { AdminApiService } from './admin-api.service';
import { ManagementPage } from './management.page';

describe('AdminApiService', () => {
  let api: AdminApiService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    api = TestBed.inject(AdminApiService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('loads users from the admin users endpoint', () => {
    const users = [
      {
        id: 'user-id',
        name: 'Alex',
        email: 'alex@example.com',
        isActive: true,
        createdAt: '2026-01-01T00:00:00Z',
        role: 'Admin' as const,
      },
    ];

    api.getUsers().subscribe((result) => expect(result).toEqual(users));

    http.expectOne('/api/users/').flush(users);
  });

  it('assigns a user role using the dedicated role endpoint', () => {
    const userId = 'user-id';

    api.updateUserRole(userId, 'SecurityOperator').subscribe();

    const request = http.expectOne(`/api/users/${userId}/role`);
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual({ role: 'SecurityOperator' });
    request.flush({
      id: userId,
      name: 'Alex',
      email: 'alex@example.com',
      isActive: true,
      createdAt: '2026-01-01T00:00:00Z',
      role: 'SecurityOperator',
    });
  });

  it('creates a user without keeping the password in the returned user model', () => {
    const requestBody = {
      name: 'Alex',
      email: 'alex@example.com',
      password: 'a-long-initial-password',
    };

    api.createUser(requestBody).subscribe((user) => {
      expect(user).not.toHaveProperty('password');
    });

    const request = http.expectOne('/api/users/');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(requestBody);
    request.flush({
      id: 'user-id',
      name: requestBody.name,
      email: requestBody.email,
      isActive: true,
      createdAt: '2026-01-01T00:00:00Z',
      role: 'SecurityOperator',
    });
  });

  it('deactivates an access card using the card state endpoint', () => {
    const cardId = 'card-id';

    api.setAccessCardActive(cardId, false).subscribe();

    const request = http.expectOne(`/api/cards/${cardId}/deactivate`);
    expect(request.request.method).toBe('POST');
    request.flush({
      id: cardId,
      userId: 'user-id',
      cardNumber: 'CARD-001',
      isActive: false,
      expiresAt: null,
      createdAt: '2026-01-01T00:00:00Z',
    });
  });

  it('creates a permission with its UTC validity interval', () => {
    const permission = {
      userId: 'user-id',
      accessPointId: 'point-id',
      validFrom: '2026-01-01T00:00:00.000Z',
      validUntil: null,
    };

    api.createAccessPermission(permission).subscribe();

    const request = http.expectOne('/api/permissions/');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(permission);
    request.flush({
      id: 'permission-id',
      ...permission,
      isActive: true,
    });
  });

  it('updates the active state of an access permission', () => {
    const permissionId = 'permission-id';
    const update = {
      userId: 'user-id',
      accessPointId: 'point-id',
      validFrom: '2026-01-01T00:00:00.000Z',
      validUntil: null,
      isActive: false,
    };

    api.updateAccessPermission(permissionId, update).subscribe();

    const request = http.expectOne(`/api/permissions/${permissionId}`);
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual(update);
    request.flush({ id: permissionId, ...update });
  });
});

describe('ManagementPage', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [ManagementPage],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('keeps a newly created user editable when the separate role update fails', () => {
    const fixture = TestBed.createComponent(ManagementPage);
    const root = fixture.nativeElement as HTMLElement;
    fixture.detectChanges();
    flushManagementLists(http);
    fixture.detectChanges();

    const usersTab = Array.from(root.querySelectorAll<HTMLButtonElement>('button')).find((button) =>
      button.textContent?.includes('Utilizadores'),
    );
    expect(usersTab).toBeDefined();
    usersTab?.click();
    fixture.detectChanges();

    setInput(root, '#user-name', 'Alex');
    setInput(root, '#user-email', 'alex@example.com');
    setInput(root, '#user-password', 'initial-password-123');
    const role = root.querySelector<HTMLSelectElement>('#user-role');
    role!.value = 'Admin';
    role!.dispatchEvent(new Event('change'));
    fixture.detectChanges();

    root.querySelector<HTMLButtonElement>('form button[type="submit"]')!.click();

    http.expectOne({ method: 'POST', url: '/api/users/' }).flush({
      id: 'user-id',
      name: 'Alex',
      email: 'alex@example.com',
      isActive: true,
      createdAt: '2026-01-01T00:00:00Z',
      role: 'SecurityOperator',
    });
    http
      .expectOne({ method: 'PUT', url: '/api/users/user-id/role' })
      .flush(
        { title: 'The last administrator cannot be demoted.' },
        { status: 409, statusText: 'Conflict' },
      );
    flushManagementLists(http);
    fixture.detectChanges();

    expect(root.textContent).toContain(
      'O perfil foi guardado, mas não foi possível alterar o papel.',
    );
    expect(root.querySelector<HTMLInputElement>('#user-email')?.value).toBe('alex@example.com');
    expect(root.querySelector<HTMLInputElement>('#user-password')).toBeNull();
    expect(
      root.querySelector<HTMLButtonElement>('form button[type="submit"]')?.textContent,
    ).toContain('Guardar utilizador');
  });
});

function flushManagementLists(http: HttpTestingController): void {
  http.expectOne('/api/buildings/').flush([]);
  http.expectOne('/api/floors/').flush([]);
  http.expectOne('/api/access-points/').flush([]);
  http.expectOne('/api/users/').flush([]);
  http.expectOne('/api/cards/').flush([]);
  http.expectOne('/api/permissions/').flush([]);
}

function setInput(element: HTMLElement, selector: string, value: string): void {
  const input = element.querySelector<HTMLInputElement>(selector);
  input!.value = value;
  input!.dispatchEvent(new Event('input'));
}
