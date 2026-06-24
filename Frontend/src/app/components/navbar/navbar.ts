import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RealEstateStore } from '../../services/real-estate-store.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './navbar.html',
  styleUrl: './navbar.css'
})
export class NavbarComponent {
  public store = inject(RealEstateStore);
}
