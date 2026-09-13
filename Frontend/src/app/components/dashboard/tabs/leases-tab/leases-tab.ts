import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RealEstateStore } from '../../../../services/real-estate-store.service';

@Component({
  selector: 'app-leases-tab',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './leases-tab.html'
})
export class LeasesTabComponent {
  public store = inject(RealEstateStore);
  public selectedLease: any = null;

  viewLease(lease: any) {
    this.selectedLease = lease;
  }

  closeLeaseDetails() {
    this.selectedLease = null;
  }

  getProperty(propertyId: string) {
    return this.store.properties().find(p => p.id === propertyId);
  }
}
