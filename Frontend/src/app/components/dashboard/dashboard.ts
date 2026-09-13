import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RealEstateStore } from '../../services/real-estate-store.service';
import { OverviewTabComponent } from './tabs/overview-tab/overview-tab';
import { PropertiesTabComponent } from './tabs/properties-tab/properties-tab';
import { LeasesTabComponent } from './tabs/leases-tab/leases-tab';
import { PaymentsTabComponent } from './tabs/payments-tab/payments-tab';
import { MaintenanceTabComponent } from './tabs/maintenance-tab/maintenance-tab';
import { ApplicationsTabComponent } from './tabs/applications-tab/applications-tab';
import { ReportsTabComponent } from './tabs/reports-tab/reports-tab';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    OverviewTabComponent,
    PropertiesTabComponent,
    LeasesTabComponent,
    PaymentsTabComponent,
    MaintenanceTabComponent,
    ApplicationsTabComponent,
    ReportsTabComponent
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class DashboardComponent {
  public store = inject(RealEstateStore);
}
