import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { forkJoin, Observable } from 'rxjs';
import { AuthService } from '../auth/auth.service';
import { AccessPoint, AdminApiService, Building, Floor } from './admin-api.service';

type Resource = 'buildings' | 'floors' | 'access-points';

@Component({
  selector: 'app-management-page',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './management.page.html',
  styleUrl: './management.page.scss',
})
export class ManagementPage implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  protected readonly auth = inject(AuthService);
  protected readonly api = inject(AdminApiService);

  protected readonly resource = signal<Resource>('buildings');
  protected readonly buildings = signal<Building[]>([]);
  protected readonly floors = signal<Floor[]>([]);
  protected readonly accessPoints = signal<AccessPoint[]>([]);
  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly editingId = signal<string | null>(null);
  protected readonly errorMessage = signal('');
  protected readonly successMessage = signal('');

  protected readonly buildingForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.pattern(/\S/), Validators.maxLength(200)]],
    address: ['', [Validators.required, Validators.pattern(/\S/), Validators.maxLength(500)]],
    isActive: [true],
  });

  protected readonly floorForm = this.formBuilder.nonNullable.group({
    buildingId: ['', Validators.required],
    number: [0, Validators.required],
    name: ['', [Validators.required, Validators.pattern(/\S/), Validators.maxLength(200)]],
    isActive: [true],
  });

  protected readonly accessPointForm = this.formBuilder.nonNullable.group({
    floorId: ['', Validators.required],
    name: ['', [Validators.required, Validators.pattern(/\S/), Validators.maxLength(200)]],
    location: ['', [Validators.required, Validators.pattern(/\S/), Validators.maxLength(500)]],
    isActive: [true],
    supportsEntry: [true],
    supportsExit: [true],
  });

  ngOnInit(): void {
    this.load();
  }

  protected selectResource(resource: Resource): void {
    this.resource.set(resource);
    this.resetForms();
    this.errorMessage.set('');
    this.successMessage.set('');
    this.editingId.set(null);
  }

  protected startCreate(): void {
    this.resetForms();
    this.editingId.set(null);
    this.errorMessage.set('');
    this.successMessage.set('');
  }

  protected editBuilding(building: Building): void {
    this.resetForms();
    this.resource.set('buildings');
    this.editingId.set(building.id);
    this.buildingForm.patchValue({
      name: building.name,
      address: building.address,
      isActive: building.isActive,
    });
    this.errorMessage.set('');
    this.successMessage.set('');
  }

  protected editFloor(floor: Floor): void {
    this.resetForms();
    this.resource.set('floors');
    this.editingId.set(floor.id);
    this.floorForm.patchValue({
      buildingId: floor.buildingId,
      number: floor.number,
      name: floor.name,
      isActive: floor.isActive,
    });
    this.errorMessage.set('');
    this.successMessage.set('');
  }

  protected editAccessPoint(accessPoint: AccessPoint): void {
    this.resetForms();
    this.resource.set('access-points');
    this.editingId.set(accessPoint.id);
    this.accessPointForm.patchValue({
      floorId: accessPoint.floorId,
      name: accessPoint.name,
      location: accessPoint.location,
      isActive: accessPoint.isActive,
      supportsEntry: accessPoint.supportsEntry,
      supportsExit: accessPoint.supportsExit,
    });
    this.errorMessage.set('');
    this.successMessage.set('');
  }

  protected save(): void {
    this.errorMessage.set('');
    this.successMessage.set('');
    if (this.saving()) return;

    switch (this.resource()) {
      case 'buildings': {
        if (this.buildingForm.invalid) {
          this.buildingForm.markAllAsTouched();
          this.errorMessage.set('Preencha os campos obrigatórios corretamente.');
          return;
        }
        const { name, address, isActive } = this.buildingForm.getRawValue();
        const id = this.editingId();
        const request = id
          ? this.api.updateBuilding(id, { name: name.trim(), address: address.trim(), isActive })
          : this.api.createBuilding({ name: name.trim(), address: address.trim() });
        this.submitRequest(request);
        break;
      }
      case 'floors': {
        if (this.floorForm.invalid) {
          this.floorForm.markAllAsTouched();
          this.errorMessage.set('Preencha os campos obrigatórios corretamente.');
          return;
        }
        const { buildingId, number, name, isActive } = this.floorForm.getRawValue();
        const id = this.editingId();
        const request = id
          ? this.api.updateFloor(id, { buildingId, number, name: name.trim(), isActive })
          : this.api.createFloor({ buildingId, number, name: name.trim() });
        this.submitRequest(request);
        break;
      }
      case 'access-points': {
        if (this.accessPointForm.invalid) {
          this.accessPointForm.markAllAsTouched();
          this.errorMessage.set('Preencha os campos obrigatórios corretamente.');
          return;
        }
        const { floorId, name, location, isActive, supportsEntry, supportsExit } =
          this.accessPointForm.getRawValue();
        const id = this.editingId();
        const request = id
          ? this.api.updateAccessPoint(id, {
              floorId,
              name: name.trim(),
              location: location.trim(),
              isActive,
              supportsEntry,
              supportsExit,
            })
          : this.api.createAccessPoint({
              floorId,
              name: name.trim(),
              location: location.trim(),
              supportsEntry,
              supportsExit,
            });
        this.submitRequest(request);
        break;
      }
    }
  }

  protected remove(id: string): void {
    const resource = this.resource();
    if (!window.confirm('Tem a certeza de que pretende eliminar este registo?')) return;

    const request =
      resource === 'buildings'
        ? this.api.deleteBuilding(id)
        : resource === 'floors'
          ? this.api.deleteFloor(id)
          : this.api.deleteAccessPoint(id);

    this.saving.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');
    request.subscribe({
      next: () => {
        this.saving.set(false);
        if (this.editingId() === id) this.startCreate();
        this.successMessage.set('Registo eliminado.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.errorMessage.set(this.describeError(error));
      },
    });
  }

  protected buildingName(id: string): string {
    return this.buildings().find((building) => building.id === id)?.name ?? 'Edifício indisponível';
  }

  protected floorLabel(floor: Floor): string {
    return `${this.buildingName(floor.buildingId)} · ${floor.name}`;
  }

  protected floorName(id: string): string {
    return this.floors().find((floor) => floor.id === id)?.name ?? 'Piso indisponível';
  }

  protected load(): void {
    this.loading.set(true);
    this.errorMessage.set('');
    forkJoin({
      buildings: this.api.getBuildings(),
      floors: this.api.getFloors(),
      accessPoints: this.api.getAccessPoints(),
    }).subscribe({
      next: ({ buildings, floors, accessPoints }) => {
        this.buildings.set(buildings);
        this.floors.set(floors);
        this.accessPoints.set(accessPoints);
        this.loading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.errorMessage.set(this.describeError(error));
      },
    });
  }

  private submitRequest(request: Observable<Building | Floor | AccessPoint>): void {
    this.saving.set(true);
    request.subscribe({
      next: () => {
        this.saving.set(false);
        this.editingId.set(null);
        this.resetForms();
        this.successMessage.set('Alterações guardadas.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.errorMessage.set(this.describeError(error));
      },
    });
  }

  private resetForms(): void {
    this.buildingForm.reset({ name: '', address: '', isActive: true });
    this.floorForm.reset({ buildingId: '', number: 0, name: '', isActive: true });
    this.accessPointForm.reset({
      floorId: '',
      name: '',
      location: '',
      isActive: true,
      supportsEntry: true,
      supportsExit: true,
    });
  }

  private describeError(error: HttpErrorResponse): string {
    if (error.status === 401) return 'A sessão expirou. Inicie sessão novamente.';
    if (error.status === 403) return 'Não tem permissões para esta operação.';
    if (error.status === 0)
      return 'Não foi possível ligar à API. Verifique a ligação e tente novamente.';
    if (typeof error.error === 'object' && error.error !== null) {
      const problem = error.error as { detail?: unknown; title?: unknown };
      if (typeof problem.detail === 'string') return problem.detail;
      if (typeof problem.title === 'string') return problem.title;
    }
    return `A operação falhou (HTTP ${error.status}).`;
  }
}
