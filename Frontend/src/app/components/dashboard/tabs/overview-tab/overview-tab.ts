import { Component, inject } from '@angular/core';
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
}
