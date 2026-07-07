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
  confirmPassword = '';

  get passwordRequirements() {
    const p = this.registerData.password || '';
    return {
      length: p.length >= 8,
      uppercase: /[A-Z]/.test(p),
      numeric: /[0-9]/.test(p),
      special: /[^A-Za-z0-9]/.test(p)
    };
  }

  get isPasswordValid(): boolean {
    const reqs = this.passwordRequirements;
    return reqs.length && reqs.uppercase && reqs.numeric && reqs.special;
  }

  get doPasswordsMatch(): boolean {
    return this.registerData.password === this.confirmPassword;
  }

  onRegister() {
    if (!this.isPasswordValid || !this.doPasswordsMatch) return;
    this.store.registerData.set({ ...this.registerData });
    this.store.onRegister();
  }

  verificationCode = '';

  onVerifyCode() {
    if (!this.verificationCode) return;
    this.store.onVerifyEmail(this.verificationCode);
  }

  onResendCode() {
    this.store.onResendVerification();
  }

  cancelVerification() {
    this.store.showVerificationScreen.set(false);
    this.store.authError.set('');
    this.store.authSuccess.set('');
  }
}
