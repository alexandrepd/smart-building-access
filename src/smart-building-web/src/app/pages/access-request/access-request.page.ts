import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import {
  AccessCard,
  AccessEvent,
  AccessPoint,
  AdminApiService,
} from '../../admin/admin-api.service';
import { AuthService } from '../../auth/auth.service';

const guidPattern = /^[0-9a-f]{8}-(?:[0-9a-f]{4}-){3}[0-9a-f]{12}$/i;

@Component({
  selector: 'app-access-request-page',
  imports: [DatePipe, ReactiveFormsModule, RouterLink],
  templateUrl: './access-request.page.html',
  styleUrl: './access-request.page.scss',
})
export class AccessRequestPage implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  protected readonly auth = inject(AuthService);
  private readonly api = inject(AdminApiService);

  protected readonly session = this.auth.getSession();
  protected readonly isAdmin = this.session?.role === 'Admin';
  protected readonly accessCards = signal<AccessCard[]>([]);
  protected readonly accessPoints = signal<AccessPoint[]>([]);
  protected readonly events = signal<AccessEvent[]>([]);
  protected readonly loadingOptions = signal(false);
  protected readonly loadingEvents = signal(false);
  protected readonly saving = signal(false);
  protected readonly optionsError = signal('');
  protected readonly eventsError = signal('');
  protected readonly errorMessage = signal('');
  protected readonly successMessage = signal('');

  protected readonly requestForm = this.formBuilder.nonNullable.group({
    cardNumber: ['', [Validators.required, Validators.pattern(/\S/), Validators.maxLength(100)]],
    accessPointId: ['', [Validators.required, Validators.pattern(guidPattern)]],
    direction: ['Entry' as 'Entry' | 'Exit', Validators.required],
  });

  ngOnInit(): void {
    this.loadEvents();
    if (this.isAdmin) this.loadAdminOptions();
  }

  protected loadEvents(): void {
    this.loadingEvents.set(true);
    this.eventsError.set('');
    this.api.getAccessEvents().subscribe({
      next: (events) => this.events.set(this.sortEvents(events).slice(0, 10)),
      error: (error: HttpErrorResponse) => {
        this.eventsError.set(this.describeError(error, 'histórico'));
        this.loadingEvents.set(false);
      },
      complete: () => this.loadingEvents.set(false),
    });
  }

  protected submitRequest(): void {
    if (this.requestForm.invalid || this.saving()) {
      this.requestForm.markAllAsTouched();
      return;
    }

    this.errorMessage.set('');
    this.successMessage.set('');
    this.saving.set(true);
    this.api.requestAccess(this.requestForm.getRawValue()).subscribe({
      next: (event) => {
        this.events.update((events) =>
          this.sortEvents([event, ...events.filter((existing) => existing.id !== event.id)]).slice(
            0,
            10,
          ),
        );
        this.successMessage.set(
          event.result === 'Granted'
            ? 'Acesso permitido. A decisão foi registada.'
            : 'Acesso negado. A decisão foi registada.',
        );
        this.requestForm.controls.cardNumber.reset('');
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage.set(this.describeError(error, 'pedido de acesso'));
        this.saving.set(false);
      },
      complete: () => this.saving.set(false),
    });
  }

  protected shortId(id: string): string {
    return id.slice(0, 8);
  }

  protected resultLabel(result: string): string {
    return result === 'Granted' ? 'Permitido' : 'Negado';
  }

  protected directionLabel(direction: AccessEvent['direction']): string {
    return direction === 'Entry' ? 'Entrada' : 'Saída';
  }

  private loadAdminOptions(): void {
    this.loadingOptions.set(true);
    this.optionsError.set('');
    forkJoin({
      cards: this.api.getAccessCards(),
      accessPoints: this.api.getAccessPoints(),
    }).subscribe({
      next: ({ cards, accessPoints }) => {
        this.accessCards.set(cards);
        this.accessPoints.set(accessPoints);
      },
      error: (error: HttpErrorResponse) => {
        this.optionsError.set(this.describeError(error, 'opções de cartões e pontos de acesso'));
        this.loadingOptions.set(false);
      },
      complete: () => this.loadingOptions.set(false),
    });
  }

  private sortEvents(events: AccessEvent[]): AccessEvent[] {
    return [...events].sort(
      (left, right) => Date.parse(right.occurredAt) - Date.parse(left.occurredAt),
    );
  }

  private describeError(error: HttpErrorResponse, operation: string): string {
    if (error.status === 0) return `Não foi possível comunicar com a API durante ${operation}.`;
    if (error.status === 400) return 'A API rejeitou os dados enviados como inválidos.';
    if (error.status === 401) return 'A sessão expirou. Inicie sessão novamente.';
    if (error.status === 403) return 'A sua conta não tem autorização para esta operação.';
    if (error.status === 404) return 'Um recurso necessário para esta operação não foi encontrado.';
    return `Não foi possível concluir ${operation} (HTTP ${error.status}).`;
  }
}
