import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5000/api';

  // Auth State
  currentUser = signal<any>(this.getStoredUser());
  token = signal<string>(localStorage.getItem('token') || '');

  private getStoredUser() {
    const user = localStorage.getItem('user');
    return user ? JSON.parse(user) : null;
  }

  setSession(authResult: any) {
    localStorage.setItem('token', authResult.token);
    localStorage.setItem('user', JSON.stringify(authResult.user));
    this.token.set(authResult.token);
    this.currentUser.set(authResult.user);
  }

  clearSession() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.token.set('');
    this.currentUser.set(null);
  }

  // Auth API
  login(credentials: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/auth/login`, credentials);
  }

  register(details: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/auth/register`, details);
  }

  // Property API
  getProperties(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/property`);
  }

  createProperty(property: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/property`, property);
  }

  updateProperty(id: string, property: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/property/${id}`, property);
  }

  deleteProperty(id: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/property/${id}`);
  }

  // Person API
  getPeople(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/person`);
  }

  // Lease API
  getLeases(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/lease`);
  }

  createLease(lease: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/lease`, lease);
  }

  // Payment API
  getPayments(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/payment`);
  }

  createPayment(payment: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/payment`, payment);
  }

  // Maintenance API
  getMaintenanceRequests(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/maintenancerequest`);
  }

  createMaintenanceRequest(request: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/maintenancerequest`, request);
  }

  // Application API
  getApplications(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/application`);
  }

  createApplication(application: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/application`, application);
  }

  updateApplicationStatus(id: string, status: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/application/${id}/status`, { status });
  }
}
