import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RealEstateStore } from '../../../../services/real-estate-store.service';

@Component({
  selector: 'app-applications-tab',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './applications-tab.html'
})
export class ApplicationsTabComponent {
  public store = inject(RealEstateStore);
}
