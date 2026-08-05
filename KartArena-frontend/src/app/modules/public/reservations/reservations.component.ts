import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { AbstractControl, FormBuilder, ValidationErrors, Validators } from '@angular/forms';
import { Subscription, forkJoin, of, throwError } from 'rxjs';
import { catchError, finalize, map, switchMap, tap } from 'rxjs/operators';

import {
  ListKartsQueryDto,
  ListKartsRequest,
} from '../../../api-services/karts/karts-api.models';
import { KartsApiService } from '../../../api-services/karts/karts-api.service';
import {
  ListPaymentTypesQueryDto,
  ListPaymentTypesRequest,
} from '../../../api-services/payment-types/payment-types-api.models';
import { PaymentTypesApiService } from '../../../api-services/payment-types/payment-types-api.service';
import {
  CheckoutReservationsRequest,
  GetReservationAvailabilityDto,
   AvailableTimeDto,
  AvailableTrackDto,
} from '../../../api-services/reservations/reservation-api.models';
import { ReservationApiService } from '../../../api-services/reservations/reservation-api.service';
import { PaymentService } from '../../../core/services/payment.service';
import { ToasterService } from '../../../core/services/toaster.service';

interface ReservationOption {
  id: number;
  name: string;
  detail?: string;
  price?: number | null;
}

type OptionSource = 'backend' | 'mock';

interface ReservationCartItem {
  id: string;
  date: string;
  startTime: string;
  endTime: string;
  duration: number;
  trackId: number;
  trackName: string;
  kartId: number;
  kartName: string;
  price?: number | null;
}

@Component({
  selector: 'app-public-reservations',
  standalone: false,
  templateUrl: './reservations.component.html',
  styleUrl: './reservations.component.scss',
})
export class ReservationsComponent implements OnInit, OnDestroy {
  private readonly cartStorageKey = 'kartArenaReservationCart';
  private readonly savedCartStorageKey = 'kartArenaSavedReservationCart';
  private readonly fb = inject(FormBuilder);
  private readonly kartsApi = inject(KartsApiService);
  private readonly paymentTypesApi = inject(PaymentTypesApiService);
  private readonly reservationApi = inject(ReservationApiService);
  private readonly paymentService = inject(PaymentService);
  private readonly toaster = inject(ToasterService);
  private readonly subscriptions = new Subscription();

  readonly durationOptions = [10, 15];
  readonly minReservationDate = this.getTodayDate();
  readonly reservationDateFilter = (date: Date | null): boolean => {
    if (!date) {
      return true;
    }

    const selectedDate = new Date(date);
    selectedDate.setHours(0, 0, 0, 0);

    return selectedDate >= this.minReservationDate;
  };
  timeOptions: string[] = [];
  availability: GetReservationAvailabilityDto | null = null;
  readonly steps = ['Detalji rezervacije', 'Dodatne informacije', 'Pregled i potvrda'];

  tracks: ReservationOption[] = [];
  karts: ReservationOption[] = [];
  paymentTypes: ListPaymentTypesQueryDto[] = [];
  tracksSource: OptionSource = 'mock';
  kartsSource: OptionSource = 'mock';
  isLoadingOptions = false;
  isLoadingPaymentTypes = false;
  isSubmitting = false;
  isRedirectingToStripe = false;
  currentStep = 0;
  cartItems: ReservationCartItem[] = [];
  savedItems: ReservationCartItem[] = [];

  private readonly notPastDateValidator = (control: AbstractControl): ValidationErrors | null => {
    const rawValue = control.value;

    if (!rawValue) {
      return null;
    }

    const selectedDate = rawValue instanceof Date ? new Date(rawValue) : new Date(rawValue);

    if (Number.isNaN(selectedDate.getTime())) {
      return { minDate: true };
    }

    selectedDate.setHours(0, 0, 0, 0);

    return selectedDate < this.minReservationDate ? { minDate: true } : null;
  };

  readonly form = this.fb.group({
    date: [null as Date | null, [Validators.required, this.notPastDateValidator]],
    startTime: ['', Validators.required],
    duration: [10, [Validators.required, Validators.pattern(/^(10|15)$/)]],
    endTime: [{ value: '', disabled: true }, Validators.required],
    trackId: [null as number | null, Validators.required],
    kartId: [null as number | null, Validators.required],
  });

  readonly customerForm = this.fb.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    phone: ['', [Validators.required, Validators.pattern(/^(\+387|00387|0)?\s?(6[0-9]|3[0-9]|5[0-9])[\s-]?[0-9]{3}[\s-]?[0-9]{3}$/)]],
    note: [''],
  });

  readonly paymentForm = this.fb.group({
    paymentTypeId: [null as number | null, Validators.required],
    amount: [{ value: 0, disabled: true }, [Validators.required, Validators.min(0.01)]],
    paymentNote: [''],
  });

  ngOnInit(): void {
    this.loadCart();
    this.loadSavedItems();
    this.syncPaymentAmount();
    
    this.loadPaymentTypes();

   this.subscriptions.add(
  this.form.controls.date.valueChanges.subscribe(() => this.loadAvailability())
);

this.subscriptions.add(
  this.form.controls.duration.valueChanges.subscribe(() => this.loadAvailability())
);

this.subscriptions.add(
  this.form.controls.startTime.valueChanges.subscribe(() => {
    this.updateEndTime();
    this.applyTracksForSelectedTime();
  })
);

this.subscriptions.add(
  this.form.controls.trackId.valueChanges.subscribe(() => {
    this.applyKartsForSelectedTrack();
  })
);


  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  get selectedTrackName(): string {
    return this.tracks.find((track) => track.id === this.form.controls.trackId.value)?.name ?? '-';
  }

  get selectedKartName(): string {
    return this.karts.find((kart) => kart.id === this.form.controls.kartId.value)?.name ?? '-';
  }

  get formattedDate(): string {
    const value = this.form.controls.date.value;
    return value ? this.formatDate(value) : '-';
  }

  get timeRange(): string {
    const startTime = this.form.controls.startTime.value;
    const endTime = this.form.controls.endTime.value;
    return startTime && endTime ? `${startTime} - ${endTime}` : '-';
  }

  get durationLabel(): string {
    return `${this.form.controls.duration.value ?? 10} minuta`;
  }

  get cartCount(): number {
    return this.cartItems.length;
  }

  get cartTotal(): number {
    return this.cartItems.reduce((total, item) => total + this.getCartItemSubtotal(item), 0);
  }

  addToCart(): void {
    this.updateEndTime();
    this.form.markAllAsTouched();

    if (this.form.invalid) {
      return;
    }

    const raw = this.form.getRawValue();
    const date = raw.date ? this.formatDate(raw.date) : '';
    const startTime = raw.startTime ?? '';
    const endTime = raw.endTime ?? '';
    const kart = this.karts.find((item) => item.id === Number(raw.kartId));

    const cartItem: ReservationCartItem = {
      id: `${Date.now()}-${Math.random().toString(36).slice(2, 9)}`,
      date,
      startTime,
      endTime,
      duration: Number(raw.duration),
      trackId: Number(raw.trackId),
      trackName: this.selectedTrackName,
      kartId: Number(raw.kartId),
      kartName: this.selectedKartName,
      price: kart?.price ?? null,
    };

    this.cartItems = [...this.cartItems, cartItem];
    this.saveCart();
    this.syncPaymentAmount();
    this.applyKartsForSelectedTrack();

    this.toaster.success('Rezervacija je dodana u korpu.');
    this.resetForm();
  }

  removeCartItem(id: string): void {
    this.cartItems = this.cartItems.filter((item) => item.id !== id);
    this.saveCart();
    this.handleActiveCartChanged();
  }

  saveForLater(id: string): void {
    const item = this.cartItems.find((cartItem) => cartItem.id === id);

    if (!item) {
      return;
    }

    this.cartItems = this.cartItems.filter((cartItem) => cartItem.id !== id);
    this.savedItems = [item, ...this.savedItems];
    this.saveCart();
    this.saveSavedItems();
    this.handleActiveCartChanged();

    this.toaster.success('Rezervacija je sacuvana za kasnije.');
  }

  moveSavedItemToCart(id: string): void {
    const item = this.savedItems.find((savedItem) => savedItem.id === id);

    if (!item) {
      return;
    }

    this.savedItems = this.savedItems.filter((savedItem) => savedItem.id !== id);
    this.cartItems = [item, ...this.cartItems];
    this.saveSavedItems();
    this.saveCart();
    this.syncPaymentAmount();

    this.toaster.success('Rezervacija je vracena u korpu.');
  }

  removeSavedItem(id: string): void {
    this.savedItems = this.savedItems.filter((item) => item.id !== id);
    this.saveSavedItems();

    this.toaster.success('Sacuvana rezervacija je uklonjena.');
  }

  clearCart(): void {
    this.cartItems = [];
    this.saveCart();
    this.handleActiveCartChanged();
  }

  goToStep(step: number): void {
    if (step === 0 || this.canOpenStep(step)) {
      this.currentStep = step;
    }
  }

  canOpenStep(step: number): boolean {
    if (step === 0) {
      return true;
    }

    if (step === 1) {
      return this.cartItems.length > 0;
    }

    return this.cartItems.length > 0 && this.customerForm.valid;
  }

  continueToCustomer(): void {
    if (this.cartItems.length === 0) {
      this.toaster.error('Dodajte barem jedan termin u korpu.');
      return;
    }

    this.currentStep = 1;
  }

  continueToReview(): void {
    this.customerForm.markAllAsTouched();

    if (this.customerForm.invalid) {
      return;
    }

    this.syncPaymentAmount();
    this.currentStep = 2;
  }

  backToRideDetails(): void {
    this.currentStep = 0;
  }

  backToCustomerDetails(): void {
    this.currentStep = 1;
  }

  checkout(): void {
    if (this.cartItems.length === 0 || this.isSubmitting || this.isRedirectingToStripe) {
      return;
    }

    this.customerForm.markAllAsTouched();
    this.paymentForm.markAllAsTouched();

    if (this.customerForm.invalid) {
      this.currentStep = 1;
      return;
    }

    if (this.paymentForm.invalid) {
      this.currentStep = 2;
      return;
    }

    const isStripePayment = this.isStripePaymentSelected();

    if (isStripePayment && this.cartItems.length !== 1) {
      this.toaster.error('Stripe plaćanje trenutno podržava jednu rezervaciju po transakciji.');
      return;
    }

    const payload = this.buildCheckoutPayload();

    this.isSubmitting = true;

    this.reservationApi
      .checkout(payload)
      .pipe(
        switchMap((reservationIds) => {
          if (!isStripePayment) {
            return of(null);
          }

          const reservationId = reservationIds[0];

          if (!reservationId) {
            return throwError(() => new Error('Backend nije vratio ID kreirane rezervacije.'));
          }

          return this.paymentService.createCheckoutSession(reservationId).pipe(
            catchError((checkoutError) =>
              this.reservationApi.delete(reservationId).pipe(
                catchError((cleanupError) => {
                  console.error('Failed to remove reservation after Stripe session error:', cleanupError);
                  return of(undefined);
                }),
                switchMap(() => throwError(() => checkoutError))
              )
            )
          );
        }),
        finalize(() => (this.isSubmitting = false))
      )
      .subscribe({
        next: (response) => {
          if (response?.checkoutUrl) {
            this.isRedirectingToStripe = true;
            window.location.assign(response.checkoutUrl);
            return;
          }

          this.toaster.success('Reservations have been submitted and are waiting for confirmation.');
          this.resetCheckoutState();
        },
        error: (err) => {
          console.error('Public reservation checkout error:', err);
          this.toaster.error(
            err?.error?.message ??
              err?.message ??
              'Reservations could not be submitted. Please try again.'
          );
        },
      });
  }

  getCartItemTimeRange(item: ReservationCartItem): string {
    return `${item.startTime} - ${item.endTime}`;
  }

  getCartItemSubtotal(item: ReservationCartItem): number {
    return item.price ?? 0;
  }

  private buildCheckoutPayload(): CheckoutReservationsRequest {
    const customer = this.customerForm.getRawValue();
    const payment = this.paymentForm.getRawValue();

    return {
      userId: null,

      customerFirstName: customer.firstName?.trim() ?? '',
      customerLastName: customer.lastName?.trim() ?? '',
      customerEmail: customer.email?.trim() ?? '',
      customerPhone: customer.phone?.trim() ?? '',
      customerNote: customer.note?.trim() || null,

      paymentTypeId: payment.paymentTypeId == null ? null : Number(payment.paymentTypeId),
      amount: Number(payment.amount ?? this.cartTotal ?? 0),
      paymentNote: payment.paymentNote?.trim() || null,

      items: this.cartItems.map((item) => ({
        reservationDate: item.date,
        startTime: this.toIsoDateTime(item.date, item.startTime),
        endTime: this.toIsoDateTime(item.date, item.endTime),
        trackId: item.trackId,
        kartId: item.kartId,
      })),
    };
  }

  private syncPaymentAmount(): void {
    this.paymentForm.patchValue({
      amount: this.cartTotal,
    });
  }

  private isStripePaymentSelected(): boolean {
    const paymentTypeId = this.paymentForm.controls.paymentTypeId.value;

    return this.paymentTypes.some(
      (paymentType) => paymentType.id === paymentTypeId && paymentType.code === 'STRIPE'
    );
  }

  private resetCheckoutState(): void {
    this.clearCart();
    this.customerForm.reset({
      firstName: '',
      lastName: '',
      email: '',
      phone: '',
      note: '',
    });
    this.paymentForm.reset({ paymentTypeId: null, amount: 0, paymentNote: '' });
    this.paymentForm.controls.amount.disable();
    this.currentStep = 0;
  }

  private handleActiveCartChanged(): void {
    this.syncPaymentAmount();
    this.applyKartsForSelectedTrack();

    if (this.cartItems.length === 0) {
      this.currentStep = 0;
      this.paymentForm.reset({
        paymentTypeId: null,
        amount: 0,
        paymentNote: '',
      });
      this.paymentForm.controls.amount.disable();
    }
  }

  private resetForm(): void {
    this.form.reset({
      date: null,
      startTime: '',
      duration: 10,
      endTime: '',
      trackId: null,
      kartId: null,
    });

    this.form.controls.endTime.disable();
  }

  // private loadOptions(): void {
  //   this.isLoadingOptions = true;

  //   const tracks$ = of(this.getMockTracks());
  //   const kartsRequest = new ListKartsRequest();

  //   const karts$ = this.kartsApi.list(kartsRequest).pipe(
  //     tap((res) => {
  //       console.log('Karts options response:', res);

  //       if (!res.items?.length) {
  //         console.warn('Karts options list is empty.', res);
  //       }
  //     }),
  //     map((res) => (res.items ?? []).map((kart) => this.mapKartOption(kart))),
  //     catchError((err) => {
  //       console.error('Load karts options error:', err);
  //       this.toaster.error('Kartove nije moguce ucitati. Pokusajte ponovo.');
  //       return of([] as ReservationOption[]);
  //     })
  //   );

  //   forkJoin({ tracks: tracks$, karts: karts$ })
  //     .pipe(finalize(() => (this.isLoadingOptions = false)))
  //     .subscribe(({ tracks, karts }) => {
  //       this.tracks = tracks;
  //       this.karts = karts;
  //       this.kartsSource = 'backend';

  //       this.syncPaymentAmount();
  //     });
  // }

  private loadPaymentTypes(): void {
    this.isLoadingPaymentTypes = true;

    const request = new ListPaymentTypesRequest();
    request.onlyEnabled = true;

    this.paymentTypesApi
      .list(request)
      .pipe(finalize(() => (this.isLoadingPaymentTypes = false)))
      .subscribe({
        next: (res) => {
          this.paymentTypes = (res.items ?? []).filter(
            (item) => item.isEnabled && item.allowedOnline
          );

          if (this.paymentTypes.length === 0) {
            console.warn('Public payment type list is empty.', res);
          }

          const stripePaymentType = this.paymentTypes.find((item) => item.code === 'STRIPE');
          const selectedPaymentType =
            stripePaymentType ?? (this.paymentTypes.length === 1 ? this.paymentTypes[0] : null);

          if (selectedPaymentType) {
            this.paymentForm.patchValue({ paymentTypeId: selectedPaymentType.id });
          }
        },
        error: (err) => {
          console.error('Load payment types error:', err);
          this.paymentTypes = [];
          this.toaster.error('Nacine placanja nije moguce ucitati. Pokusajte ponovo.');
        },
      });
  }

  private mapKartOption(kart: ListKartsQueryDto): ReservationOption {
    const detail = [kart.manufacturer, kart.colour].filter(Boolean).join(' / ');

    return {
      id: kart.id,
      name: kart.name || `Kart #${kart.id}`,
      detail,
      price: kart.pricePerSession ?? null,
    };
  }

  private updateEndTime(): void {
    const startTime = this.form.controls.startTime.value;
    const duration = Number(this.form.controls.duration.value);

    if (!startTime || !this.durationOptions.includes(duration)) {
      this.form.controls.endTime.setValue('');
      return;
    }

    this.form.controls.endTime.setValue(this.addMinutes(startTime, duration));
  }

  private buildTimeOptions(): string[] {
    const options: string[] = [];

    for (let hour = 10; hour <= 22; hour += 1) {
      for (const minute of [0, 15, 30, 45]) {
        if (hour === 22 && minute > 0) {
          continue;
        }

        options.push(`${hour.toString().padStart(2, '0')}:${minute
          .toString()
          .padStart(2, '0')}`);
      }
    }

    return options;
  }

  private addMinutes(time: string, minutesToAdd: number): string {
    const [hours, minutes] = time.split(':').map(Number);
    const totalMinutes = hours * 60 + minutes + minutesToAdd;
    const nextHours = Math.floor(totalMinutes / 60) % 24;
    const nextMinutes = totalMinutes % 60;

    return `${nextHours.toString().padStart(2, '0')}:${nextMinutes
      .toString()
      .padStart(2, '0')}`;
  }

  private formatDate(date: Date): string {
    const year = date.getFullYear();
    const month = `${date.getMonth() + 1}`.padStart(2, '0');
    const day = `${date.getDate()}`.padStart(2, '0');

    return `${year}-${month}-${day}`;
  }

  private getTodayDate(): Date {
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    return today;
  }

  private toIsoDateTime(date: string, time: string): string {
    return `${date}T${time}:00`;
  }

  private loadCart(): void {
    const raw = localStorage.getItem(this.cartStorageKey);

    if (!raw) {
      this.cartItems = [];
      return;
    }

    try {
      const parsed = JSON.parse(raw) as ReservationCartItem[];
      this.cartItems = Array.isArray(parsed) ? parsed : [];
    } catch {
      this.cartItems = [];
      localStorage.removeItem(this.cartStorageKey);
    }
  }

  private saveCart(): void {
    if (this.cartItems.length === 0) {
      localStorage.removeItem(this.cartStorageKey);
      return;
    }

    localStorage.setItem(this.cartStorageKey, JSON.stringify(this.cartItems));
  }

  private loadSavedItems(): void {
    const raw = localStorage.getItem(this.savedCartStorageKey);

    if (!raw) {
      this.savedItems = [];
      return;
    }

    try {
      const parsed = JSON.parse(raw) as ReservationCartItem[];
      this.savedItems = Array.isArray(parsed) ? parsed : [];
    } catch {
      this.savedItems = [];
      localStorage.removeItem(this.savedCartStorageKey);
    }
  }

  private saveSavedItems(): void {
    if (this.savedItems.length === 0) {
      localStorage.removeItem(this.savedCartStorageKey);
      return;
    }

    localStorage.setItem(this.savedCartStorageKey, JSON.stringify(this.savedItems));
  }

  // private getMockTracks(): ReservationOption[] {
  //   // Temporary mock data: replace with a TracksApiService when a public tracks endpoint exists.
  //   return [
  //     { id: 1, name: 'Indoor Sprint', detail: 'Tehnicka staza' },
  //     { id: 2, name: 'Outdoor GP', detail: 'Brza staza' },
  //   ];
  // }

  private loadAvailability(): void {
  const raw = this.form.getRawValue();

  if (!raw.date || !raw.duration || this.form.controls.date.invalid) {
    this.availability = null;
    this.timeOptions = [];
    this.tracks = [];
    this.karts = [];

    this.form.patchValue({
      startTime: '',
      endTime: '',
      trackId: null,
      kartId: null,
    }, { emitEvent: false });

    return;
  }

  const date = this.formatDate(raw.date);
  const duration = Number(raw.duration);

  this.form.patchValue({
    startTime: '',
    endTime: '',
    trackId: null,
    kartId: null,
  }, { emitEvent: false });

  this.tracks = [];
  this.karts = [];
  this.timeOptions = [];
  this.availability = null;
  this.isLoadingOptions = true;

  this.reservationApi
    .getAvailability(date, duration)
    .pipe(finalize(() => (this.isLoadingOptions = false)))
    .subscribe({
      next: (availability) => {
        const availableTimes = availability.availableTimes.filter((slot) =>
          this.isFutureStartTime(date, slot.startTime)
        );

        this.availability = { ...availability, availableTimes };
        this.timeOptions = availableTimes.map((slot) => slot.startTime);
      },
      error: (err) => {
        console.error('Load availability error:', err);
        this.toaster.error('Dostupni termini se nisu mogli učitati.');
      },
    });
}

private isFutureStartTime(date: string, startTime: string): boolean {
  const today = this.formatDate(new Date());

  if (date !== today) {
    return true;
  }

  const normalizedTime = startTime.includes('T')
    ? startTime.split('T')[1]?.split('.')[0]
    : startTime.split('.')[0];
  const slotStart = new Date(`${date}T${normalizedTime}`);

  return !Number.isNaN(slotStart.getTime()) && slotStart.getTime() > Date.now();
}

private applyTracksForSelectedTime(): void {
  const slot = this.getSelectedAvailabilitySlot();

  this.form.patchValue({
    trackId: null,
    kartId: null,
  }, { emitEvent: false });

  this.karts = [];

  if (!slot) {
    this.tracks = [];
    return;
  }

  this.tracks = slot.tracks.map((track) => ({
    id: track.trackId,
    name: track.trackName,
    detail: `${track.availableSlots} slobodnih mjesta`,
  }));
}

private applyKartsForSelectedTrack(): void {
  const slot = this.getSelectedAvailabilitySlot();
  const trackId = this.form.controls.trackId.value;

  this.form.patchValue({
    kartId: null,
  }, { emitEvent: false });

  if (!slot || !trackId) {
    this.karts = [];
    return;
  }

  const selectedTrack = slot.tracks.find((track) => track.trackId === Number(trackId));

  if (!selectedTrack) {
    this.karts = [];
    return;
  }

  const raw = this.form.getRawValue();
  const date = raw.date ? this.formatDate(raw.date) : '';
  const startTime = raw.startTime ?? '';
  const endTime = raw.endTime ?? '';

  this.karts = selectedTrack.availableKarts
    .filter((kart) => !this.isKartAlreadyInCart(date, startTime, endTime, kart.kartId))
    .map((kart) => ({
      id: kart.kartId,
      name: kart.kartName,
      price: kart.pricePerSession ?? null,
    }));
}

private getSelectedAvailabilitySlot(): AvailableTimeDto | null {
  const startTime = this.form.controls.startTime.value;

  if (!this.availability || !startTime) {
    return null;
  }

  return this.availability.availableTimes.find((slot) => slot.startTime === startTime) ?? null;
}

private isKartAlreadyInCart(
  date: string,
  startTime: string,
  endTime: string,
  kartId: number
): boolean {
  return this.cartItems.some((item) =>
    item.date === date &&
    item.kartId === kartId &&
    item.startTime < endTime &&
    item.endTime > startTime
  );
}
}
