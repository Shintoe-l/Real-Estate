import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RealEstateStore } from '../../services/real-estate-store.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class RegisterComponent {
  public store = inject(RealEstateStore);

  registerData = { firstName: '', lastName: '', email: '', phoneNumber: '', password: '', role: 2 };

  onRegister() {
    this.store.registerData.set({ ...this.registerData });
    this.store.onRegister();
  }
}
