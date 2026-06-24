import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RealEstateStore } from './services/real-estate-store.service';
import { NavbarComponent } from './components/navbar/navbar';
import { PropertyCardComponent } from './components/property-card/property-card';
import { LoginComponent } from './components/login/login';
import { RegisterComponent } from './components/register/register';
import { DashboardComponent } from './components/dashboard/dashboard';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NavbarComponent,
    PropertyCardComponent,
    LoginComponent,
    RegisterComponent,
    DashboardComponent
  ],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  public store = inject(RealEstateStore);
}
