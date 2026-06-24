import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RealEstateStore } from '../../services/real-estate-store.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class LoginComponent {
  public store = inject(RealEstateStore);

  loginData = { email: '', password: '' };

  onLogin() {
    this.store.loginData.set({ ...this.loginData });
    this.store.onLogin();
  }
}
