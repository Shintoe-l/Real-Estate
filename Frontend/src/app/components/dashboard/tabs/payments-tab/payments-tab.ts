import { Component, inject, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RealEstateStore } from '../../../../services/real-estate-store.service';

@Component({
  selector: 'app-payments-tab',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './payments-tab.html'
})
export class PaymentsTabComponent {
  public store = inject(RealEstateStore);

  newPaymentData = {
    leaseId: '',
    amount: 0.0,
    type: 0
  };

  constructor() {
    // Sync local form state when store modifications occur (e.g. resets)
    effect(() => {
      const data = this.store.newPaymentData();
      this.newPaymentData = { ...data };
    });
  }

  submitPayment() {
    this.store.newPaymentData.set(this.newPaymentData);
    this.store.submitPayment();
  }
}
