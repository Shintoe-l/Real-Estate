import { Component, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RealEstateStore } from '../../../../services/real-estate-store.service';

@Component({
  selector: 'app-overview-tab',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './overview-tab.html'
})
export class OverviewTabComponent {
  public store = inject(RealEstateStore);

  pendingApplications = computed(() =>
    this.store.myApplications().filter((a: any) => a.status === 0).length
  );
}
