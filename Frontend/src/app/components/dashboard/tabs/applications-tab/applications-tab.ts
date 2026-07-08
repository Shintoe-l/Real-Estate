import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RealEstateStore } from '../../../../services/real-estate-store.service';

@Component({
  selector: 'app-applications-tab',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './applications-tab.html'
})
export class ApplicationsTabComponent {
  public store = inject(RealEstateStore);

  // Payment Modal State
  showPaymentModal = false;
  showReceiptModal = false;
  activeApplication: any = null;

  paymentData = {
    method: '',
    phoneNumber: '',
    accountName: '',
    bankName: '',
    referenceNumber: '',
    amount: 0
  };

  receiptData = {
    referenceNumber: '',
    method: '',
    note: ''
  };

  paymentMethods = [
    { id: 'ecocash',   label: 'EcoCash',       icon: '📱', hint: 'Enter your EcoCash number' },
    { id: 'onemoney',  label: 'OneMoney',       icon: '📲', hint: 'Enter your NetOne number' },
    { id: 'telecash',  label: 'TeleCash',       icon: '📡', hint: 'Enter your Telecel number' },
    { id: 'bank',      label: 'Bank Transfer',  icon: '🏦', hint: 'Enter your bank account details' },
    { id: 'zipit',     label: 'ZIPIT',          icon: '⚡', hint: 'Enter your ZIPIT/bank number' },
    { id: 'swipe',     label: 'Swipe/POS',      icon: '💳', hint: 'Confirm POS transaction reference' }
  ];

  get selectedMethod() {
    return this.paymentMethods.find(m => m.id === this.paymentData.method) || null;
  }

  openPaymentModal(application: any) {
    this.activeApplication = application;
    this.paymentData = { method: '', phoneNumber: '', accountName: '', bankName: '', referenceNumber: '', amount: 0 };
    this.showPaymentModal = true;
    this.showReceiptModal = false;
  }

  openReceiptModal(application: any) {
    this.activeApplication = application;
    this.receiptData = { referenceNumber: '', method: '', note: '' };
    this.showReceiptModal = true;
    this.showPaymentModal = false;
  }

  closeModals() {
    this.showPaymentModal = false;
    this.showReceiptModal = false;
    this.activeApplication = null;
  }

  isMobileMethod(): boolean {
    return ['ecocash', 'onemoney', 'telecash'].includes(this.paymentData.method);
  }

  isBankMethod(): boolean {
    return ['bank', 'zipit', 'swipe'].includes(this.paymentData.method);
  }

  submitPayment() {
    if (!this.paymentData.method || !this.paymentData.amount) return;
    const label = this.selectedMethod?.label || 'Unknown';
    const refParts = [label, this.paymentData.phoneNumber || this.paymentData.accountName, Date.now()].filter(Boolean);
    const ref = 'PAY-' + refParts.join('-').replace(/\s+/g, '').toUpperCase().substring(0, 24);

    this.store.submitPaymentFromApplication(this.activeApplication, this.paymentData.amount, label, ref);
    this.closeModals();
  }

  submitReceipt() {
    if (!this.receiptData.referenceNumber) return;
    this.store.submitPaymentFromApplication(
      this.activeApplication,
      0, // amount not required for receipt upload
      this.receiptData.method || 'Receipt Upload',
      this.receiptData.referenceNumber
    );
    this.closeModals();
  }
}
