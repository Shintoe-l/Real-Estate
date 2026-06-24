import { Component, inject, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RealEstateStore } from '../../../../services/real-estate-store.service';

@Component({
  selector: 'app-maintenance-tab',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './maintenance-tab.html'
})
export class MaintenanceTabComponent {
  public store = inject(RealEstateStore);

  newMaintenanceData = {
    propertyId: '',
    description: ''
  };

  constructor() {
    // Sync local form state when store modifications occur (e.g. resets)
    effect(() => {
      const data = this.store.newMaintenanceData();
      this.newMaintenanceData = { ...data };
    });
  }

  submitMaintenance() {
    this.store.newMaintenanceData.set(this.newMaintenanceData);
    this.store.submitMaintenance();
  }
}
