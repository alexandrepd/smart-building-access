import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

export interface Building {
  id: string;
  name: string;
  address: string;
  isActive: boolean;
  createdAt: string;
}

export interface Floor {
  id: string;
  buildingId: string;
  number: number;
  name: string;
  isActive: boolean;
}

export interface AccessPoint {
  id: string;
  floorId: string;
  name: string;
  location: string;
  isActive: boolean;
  supportsEntry: boolean;
  supportsExit: boolean;
  createdAt: string;
}

export interface AccessEvent {
  id: string;
  accessPointId: string;
  accessCardId: string | null;
  userId: string | null;
  occurredAt: string;
  direction: 'Entry' | 'Exit';
  result: string;
  reason: string;
}

export interface AccessRequest {
  cardNumber: string;
  accessPointId: string;
  direction: 'Entry' | 'Exit';
}

export interface BuildingUser {
  id: string;
  name: string;
  email: string;
  isActive: boolean;
  createdAt: string;
  role: 'Admin' | 'SecurityOperator';
}

export interface AccessCard {
  id: string;
  userId: string;
  cardNumber: string;
  isActive: boolean;
  expiresAt: string | null;
  createdAt: string;
}

export interface AccessPermission {
  id: string;
  userId: string;
  accessPointId: string;
  isActive: boolean;
  validFrom: string;
  validUntil: string | null;
}

@Injectable({ providedIn: 'root' })
export class AdminApiService {
  private readonly http = inject(HttpClient);

  getBuildings(): Observable<Building[]> {
    return this.http.get<Building[]>('/api/buildings/');
  }

  createBuilding(request: Pick<Building, 'name' | 'address'>): Observable<Building> {
    return this.http.post<Building>('/api/buildings/', request);
  }

  updateBuilding(
    id: string,
    request: Pick<Building, 'name' | 'address' | 'isActive'>,
  ): Observable<Building> {
    return this.http.put<Building>(`/api/buildings/${id}`, request);
  }

  deleteBuilding(id: string): Observable<void> {
    return this.http.delete<void>(`/api/buildings/${id}`);
  }

  getFloors(): Observable<Floor[]> {
    return this.http.get<Floor[]>('/api/floors/');
  }

  createFloor(request: Pick<Floor, 'buildingId' | 'number' | 'name'>): Observable<Floor> {
    return this.http.post<Floor>('/api/floors/', request);
  }

  updateFloor(
    id: string,
    request: Pick<Floor, 'buildingId' | 'number' | 'name' | 'isActive'>,
  ): Observable<Floor> {
    return this.http.put<Floor>(`/api/floors/${id}`, request);
  }

  deleteFloor(id: string): Observable<void> {
    return this.http.delete<void>(`/api/floors/${id}`);
  }

  getAccessPoints(): Observable<AccessPoint[]> {
    return this.http.get<AccessPoint[]>('/api/access-points/');
  }

  createAccessPoint(
    request: Pick<AccessPoint, 'floorId' | 'name' | 'location' | 'supportsEntry' | 'supportsExit'>,
  ): Observable<AccessPoint> {
    return this.http.post<AccessPoint>('/api/access-points/', request);
  }

  updateAccessPoint(
    id: string,
    request: Pick<
      AccessPoint,
      'floorId' | 'name' | 'location' | 'isActive' | 'supportsEntry' | 'supportsExit'
    >,
  ): Observable<AccessPoint> {
    return this.http.put<AccessPoint>(`/api/access-points/${id}`, request);
  }

  deleteAccessPoint(id: string): Observable<void> {
    return this.http.delete<void>(`/api/access-points/${id}`);
  }

  getAccessEvents(): Observable<AccessEvent[]> {
    return this.http.get<AccessEvent[]>('/api/access/events');
  }

  requestAccess(request: AccessRequest): Observable<AccessEvent> {
    return this.http.post<AccessEvent>('/api/access/requests', request);
  }

  getUsers(): Observable<BuildingUser[]> {
    return this.http.get<BuildingUser[]>('/api/users/');
  }

  createUser(request: { name: string; email: string; password: string }): Observable<BuildingUser> {
    return this.http.post<BuildingUser>('/api/users/', request);
  }

  updateUser(
    id: string,
    request: Pick<BuildingUser, 'name' | 'email' | 'isActive'>,
  ): Observable<BuildingUser> {
    return this.http.put<BuildingUser>(`/api/users/${id}`, request);
  }

  updateUserRole(id: string, role: BuildingUser['role']): Observable<BuildingUser> {
    return this.http.put<BuildingUser>(`/api/users/${id}/role`, { role });
  }

  deleteUser(id: string): Observable<void> {
    return this.http.delete<void>(`/api/users/${id}`);
  }

  getAccessCards(): Observable<AccessCard[]> {
    return this.http.get<AccessCard[]>('/api/cards/');
  }

  createAccessCard(
    request: Pick<AccessCard, 'userId' | 'cardNumber' | 'expiresAt'>,
  ): Observable<AccessCard> {
    return this.http.post<AccessCard>('/api/cards/', request);
  }

  updateAccessCard(
    id: string,
    request: Pick<AccessCard, 'userId' | 'cardNumber' | 'expiresAt'>,
  ): Observable<AccessCard> {
    return this.http.put<AccessCard>(`/api/cards/${id}`, request);
  }

  setAccessCardActive(id: string, isActive: boolean): Observable<AccessCard> {
    const action = isActive ? 'activate' : 'deactivate';
    return this.http.post<AccessCard>(`/api/cards/${id}/${action}`, null);
  }

  deleteAccessCard(id: string): Observable<void> {
    return this.http.delete<void>(`/api/cards/${id}`);
  }

  getAccessPermissions(): Observable<AccessPermission[]> {
    return this.http.get<AccessPermission[]>('/api/permissions/');
  }

  createAccessPermission(
    request: Pick<AccessPermission, 'userId' | 'accessPointId' | 'validFrom' | 'validUntil'>,
  ): Observable<AccessPermission> {
    return this.http.post<AccessPermission>('/api/permissions/', request);
  }

  updateAccessPermission(
    id: string,
    request: Pick<
      AccessPermission,
      'userId' | 'accessPointId' | 'validFrom' | 'validUntil' | 'isActive'
    >,
  ): Observable<AccessPermission> {
    return this.http.put<AccessPermission>(`/api/permissions/${id}`, request);
  }

  deleteAccessPermission(id: string): Observable<void> {
    return this.http.delete<void>(`/api/permissions/${id}`);
  }
}
