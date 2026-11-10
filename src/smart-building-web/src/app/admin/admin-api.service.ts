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
}
