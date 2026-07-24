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

  selectedRequest: any = null;
  statusFilter: string = 'all';

  get filteredRequests() {
    let requests = this.store.myMaintenanceRequests();
    
    if (this.statusFilter === 'all') {
      return requests;
    }
    
    if (this.statusFilter === 'outstanding') {
      return requests.filter(r => this.isOutstanding(r));
    }

    const statusNum = Number(this.statusFilter);
    if (!isNaN(statusNum)) {
      return requests.filter(r => r.status === statusNum);
    }
    
    return requests;
  }

  constructor() {
    // Sync local form state when store modifications occur (e.g. resets)
    effect(() => {
      const data = this.store.newPropertyData();
      this.selectedRequest = null;
    });
    effect(() => {
      const data = this.store.newMaintenanceData();
      this.newMaintenanceData = { ...data };
    });
  }

  isOutstanding(req: any): boolean {
    if (req.status === 2) return false;
    const requestDate = new Date(req.requestDate);
    const now = new Date();
    const diffTime = now.getTime() - requestDate.getTime();
    const diffDays = diffTime / (1000 * 60 * 60 * 24);
    return diffDays > 7;
  }

  submitMaintenance() {
    this.store.newMaintenanceData.set(this.newMaintenanceData);
    this.store.submitMaintenance();
  }

  selectRequest(req: any) {
    this.selectedRequest = req;
  }

  closeDetails() {
    this.selectedRequest = null;
  }

  updateStatus(status: number) {
    if (this.selectedRequest) {
      this.store.updateMaintenanceRequestStatus(this.selectedRequest.id, status);
      // Close the modal or update local state status
      this.selectedRequest.status = status;
    }
  }

  rateMaintenance(isSatisfied: boolean) {
    if (this.selectedRequest) {
      this.store.rateMaintenanceRequest(this.selectedRequest.id, isSatisfied);
      this.selectedRequest.isSatisfied = isSatisfied;
    }
  }
}
