import { Component, inject, computed, signal } from '@angular/core';
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

  // ── Filter State ──────────────────────────────────────────────────
  filterStatus = signal<string>('all');
  filterType   = signal<string>('all');
  filterSearch = signal<string>('');

  filteredApplications = computed(() => {
    let apps = this.store.myApplications();
    const status  = this.filterStatus();
    const type    = this.filterType();
    const search  = this.filterSearch().toLowerCase().trim();

    if (status !== 'all') apps = apps.filter((a: any) => a.status === Number(status));
    if (type   !== 'all') apps = apps.filter((a: any) => a.type   === Number(type));
    if (search) {
      apps = apps.filter((a: any) =>
        a.id.toLowerCase().includes(search) ||
        a.propertyId.toLowerCase().includes(search) ||
        a.tenantId?.toLowerCase().includes(search)
      );
    }
    return apps;
  });

  clearFilters() {
    this.filterStatus.set('all');
    this.filterType.set('all');
    this.filterSearch.set('');
  }

  get activeFilterCount(): number {
    let c = 0;
    if (this.filterStatus() !== 'all') c++;
    if (this.filterType()   !== 'all') c++;
    if (this.filterSearch())            c++;
    return c;
  }

  // ── Application Detail Modal ──────────────────────────────────────
  detailApplication: any = null;

  openDetailModal(app: any) {
    this.detailApplication = app;
  }

  closeDetailModal() {
    this.detailApplication = null;
  }

  getApplicant(tenantId: string): any {
    return this.store.people().find((p: any) => p.id === tenantId) || null;
  }

  getPropertyForApp(propertyId: string): any {
    return this.store.properties().find((p: any) => p.id === propertyId) || null;
  }

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
