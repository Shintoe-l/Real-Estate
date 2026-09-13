import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RealEstateStore } from '../../../../services/real-estate-store.service';

@Component({
  selector: 'app-reports-tab',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './reports-tab.html',
  styleUrls: ['./reports-tab.css']
})
export class ReportsTabComponent {
  public store = inject(RealEstateStore);
  public currentDate = new Date();

  printReport() {
    window.print();
  }

  get soldProperties() {
    // Status 4 is Sold
    return this.store.myProperties().filter(p => p.status === 4);
  }

  get rentedProperties() {
    // Status 1 is Occupied/Rented
    return this.store.myProperties().filter(p => p.status === 1);
  }

  get totalSoldRevenue() {
    return this.soldProperties.reduce((sum, p) => sum + (p.monthlyRent || 0), 0);
  }

  get totalRentedRevenue() {
    return this.rentedProperties.reduce((sum, p) => sum + (p.monthlyRent || 0), 0);
  }
}
