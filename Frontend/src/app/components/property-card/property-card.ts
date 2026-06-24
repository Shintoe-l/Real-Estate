import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RealEstateStore } from '../../services/real-estate-store.service';

@Component({
  selector: 'app-property-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './property-card.html',
  styleUrl: './property-card.css'
})
export class PropertyCardComponent {
  @Input() property!: any;
  public store = inject(RealEstateStore);
}
