import { HttpErrorResponse } from '@angular/common/http';
import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { forkJoin, Observable } from 'rxjs';
import { AuthService } from '../auth/auth.service';
import {
  AccessCard,
  AccessEvent,
  AccessPermission,
  AccessPoint,
  AdminApiService,
  Building,
  BuildingUser,
  Floor,
} from './admin-api.service';

type Resource = 'buildings' | 'floors' | 'access-points' | 'users' | 'cards' | 'permissions';

@Component({
  selector: 'app-management-page',
  imports: [DatePipe, ReactiveFormsModule, RouterLink],
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
  protected readonly users = signal<BuildingUser[]>([]);
  protected readonly accessCards = signal<AccessCard[]>([]);
  protected readonly accessPermissions = signal<AccessPermission[]>([]);
  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly editingId = signal<string | null>(null);
  protected readonly editingUserRole = signal<BuildingUser['role'] | null>(null);
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

  protected readonly userForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.pattern(/\S/), Validators.maxLength(200)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(320)]],
    password: ['', [Validators.required, Validators.minLength(12), Validators.maxLength(128)]],
    role: ['SecurityOperator' as BuildingUser['role'], Validators.required],
    isActive: [true],
  });

  protected readonly accessCardForm = this.formBuilder.nonNullable.group({
    userId: ['', Validators.required],
    cardNumber: ['', [Validators.required, Validators.pattern(/\S/), Validators.maxLength(100)]],
    expiresAt: [''],
  });

  protected readonly accessPermissionForm = this.formBuilder.nonNullable.group({
    userId: ['', Validators.required],
    accessPointId: ['', Validators.required],
    validFrom: ['', Validators.required],
    validUntil: [''],
    isActive: [true],
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

  protected editUser(user: BuildingUser): void {
    this.resetForms();
    this.resource.set('users');
    this.editingId.set(user.id);
    this.editingUserRole.set(user.role);
    this.userForm.controls.password.clearValidators();
    this.userForm.controls.password.updateValueAndValidity({ emitEvent: false });
    this.userForm.patchValue({
      name: user.name,
      email: user.email,
      password: '',
      role: user.role,
      isActive: user.isActive,
    });
    this.errorMessage.set('');
    this.successMessage.set('');
  }

  protected editAccessCard(card: AccessCard): void {
    this.resetForms();
    this.resource.set('cards');
    this.editingId.set(card.id);
    this.accessCardForm.patchValue({
      userId: card.userId,
      cardNumber: card.cardNumber,
      expiresAt: card.expiresAt ? this.toLocalDateTime(card.expiresAt) : '',
    });
    this.errorMessage.set('');
    this.successMessage.set('');
  }

  protected editAccessPermission(permission: AccessPermission): void {
    this.resetForms();
    this.resource.set('permissions');
    this.editingId.set(permission.id);
    this.accessPermissionForm.patchValue({
      userId: permission.userId,
      accessPointId: permission.accessPointId,
      validFrom: this.toLocalDateTime(permission.validFrom),
      validUntil: permission.validUntil ? this.toLocalDateTime(permission.validUntil) : '',
      isActive: permission.isActive,
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
      case 'users': {
        if (this.userForm.invalid) {
          this.userForm.markAllAsTouched();
          this.errorMessage.set('Preencha os campos obrigatórios corretamente.');
          return;
        }
        const { name, email, password, role, isActive } = this.userForm.getRawValue();
        this.saveUser(name.trim(), email.trim(), password, role, isActive);
        break;
      }
      case 'cards': {
        if (this.accessCardForm.invalid) {
          this.accessCardForm.markAllAsTouched();
          this.errorMessage.set('Preencha os campos obrigatórios corretamente.');
          return;
        }
        const { userId, cardNumber, expiresAt } = this.accessCardForm.getRawValue();
        const expiration = expiresAt ? new Date(expiresAt) : null;
        if (expiration !== null && !Number.isFinite(expiration.getTime())) {
          this.errorMessage.set('A data de expiração do cartão não é válida.');
          return;
        }
        const requestData = {
          userId,
          cardNumber: cardNumber.trim(),
          expiresAt: expiration?.toISOString() ?? null,
        };
        const id = this.editingId();
        const request = id
          ? this.api.updateAccessCard(id, requestData)
          : this.api.createAccessCard(requestData);
        this.submitRequest(request);
        break;
      }
      case 'permissions': {
        if (this.accessPermissionForm.invalid) {
          this.accessPermissionForm.markAllAsTouched();
          this.errorMessage.set('Preencha os campos obrigatórios corretamente.');
          return;
        }
        const { userId, accessPointId, validFrom, validUntil, isActive } =
          this.accessPermissionForm.getRawValue();
        const validFromDate = new Date(validFrom);
        const validUntilDate = validUntil ? new Date(validUntil) : null;
        if (
          !Number.isFinite(validFromDate.getTime()) ||
          (validUntilDate !== null &&
            (!Number.isFinite(validUntilDate.getTime()) || validUntilDate <= validFromDate))
        ) {
          this.errorMessage.set('A data de fim tem de ser posterior à data de início.');
          return;
        }
        const requestData = {
          userId,
          accessPointId,
          validFrom: validFromDate.toISOString(),
          validUntil: validUntilDate?.toISOString() ?? null,
        };
        const id = this.editingId();
        const request = id
          ? this.api.updateAccessPermission(id, { ...requestData, isActive })
          : this.api.createAccessPermission(requestData);
        this.submitRequest(request);
        break;
      }
    }
  }

  protected toggleAccessCard(card: AccessCard): void {
    this.saving.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');
    this.api.setAccessCardActive(card.id, !card.isActive).subscribe({
      next: () => {
        this.saving.set(false);
        this.successMessage.set(card.isActive ? 'Cartão desativado.' : 'Cartão ativado.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.errorMessage.set(this.describeError(error));
      },
    });
  }

  protected remove(id: string): void {
    const resource = this.resource();
    if (!window.confirm('Tem a certeza de que pretende eliminar este registo?')) return;

    const request =
      resource === 'buildings'
        ? this.api.deleteBuilding(id)
        : resource === 'floors'
          ? this.api.deleteFloor(id)
          : resource === 'access-points'
            ? this.api.deleteAccessPoint(id)
            : resource === 'users'
              ? this.api.deleteUser(id)
              : resource === 'cards'
                ? this.api.deleteAccessCard(id)
                : this.api.deleteAccessPermission(id);

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

  protected userName(id: string): string {
    return this.users().find((user) => user.id === id)?.name ?? 'Utilizador indisponível';
  }

  protected accessPointName(id: string): string {
    return this.accessPoints().find((point) => point.id === id)?.name ?? 'Ponto indisponível';
  }

  protected permissionValidity(permission: AccessPermission): string {
    const start = new Date(permission.validFrom).toLocaleString();
    return permission.validUntil
      ? `${start} – ${new Date(permission.validUntil).toLocaleString()}`
      : `${start} – sem data de fim`;
  }

  protected load(preserveMessages = false): void {
    this.loading.set(true);
    if (!preserveMessages) this.errorMessage.set('');
    forkJoin({
      buildings: this.api.getBuildings(),
      floors: this.api.getFloors(),
      accessPoints: this.api.getAccessPoints(),
      users: this.api.getUsers(),
      accessCards: this.api.getAccessCards(),
      accessPermissions: this.api.getAccessPermissions(),
    }).subscribe({
      next: ({ buildings, floors, accessPoints, users, accessCards, accessPermissions }) => {
        this.buildings.set(buildings);
        this.floors.set(floors);
        this.accessPoints.set(accessPoints);
        this.users.set(users);
        this.accessCards.set(accessCards);
        this.accessPermissions.set(accessPermissions);
        this.loading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.loading.set(false);
        this.errorMessage.set(this.describeError(error));
      },
    });
  }

  private submitRequest(request: Observable<unknown>): void {
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

  private saveUser(
    name: string,
    email: string,
    password: string,
    role: BuildingUser['role'],
    isActive: boolean,
  ): void {
    const id = this.editingId();
    const request = id
      ? this.api.updateUser(id, { name, email, isActive })
      : this.api.createUser({ name, email, password });

    this.saving.set(true);
    request.subscribe({
      next: (user) => {
        if (user.role === role) {
          this.finishSave();
          return;
        }

        this.api.updateUserRole(user.id, role).subscribe({
          next: () => this.finishSave(),
          error: (error: HttpErrorResponse) => {
            this.saving.set(false);
            this.editUser(user);
            this.errorMessage.set(
              `O perfil foi guardado, mas não foi possível alterar o papel. ${this.describeError(error)} Pode corrigir o papel e tentar novamente.`,
            );
            this.load(true);
          },
        });
      },
      error: (error: HttpErrorResponse) => {
        this.saving.set(false);
        this.errorMessage.set(this.describeError(error));
      },
    });
  }

  private finishSave(): void {
    this.saving.set(false);
    this.editingId.set(null);
    this.resetForms();
    this.successMessage.set('Alterações guardadas.');
    this.load();
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
    this.userForm.reset({
      name: '',
      email: '',
      password: '',
      role: 'SecurityOperator',
      isActive: true,
    });
    this.userForm.controls.password.setValidators([
      Validators.required,
      Validators.minLength(12),
      Validators.maxLength(128),
    ]);
    this.userForm.controls.password.updateValueAndValidity({ emitEvent: false });
    this.accessCardForm.reset({ userId: '', cardNumber: '', expiresAt: '' });
    this.accessPermissionForm.reset({
      userId: '',
      accessPointId: '',
      validFrom: '',
      validUntil: '',
      isActive: true,
    });
    this.editingUserRole.set(null);
  }

  private toLocalDateTime(value: string): string {
    const date = new Date(value);
    const offset = date.getTimezoneOffset() * 60_000;
    return new Date(date.getTime() - offset).toISOString().slice(0, 16);
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
