import { Injectable, inject, signal, computed, effect } from '@angular/core';
import { ApiService } from './api.service';

@Injectable({
  providedIn: 'root'
})
export class RealEstateStore {
  private apiService = inject(ApiService);

  // Expose signals from ApiService
  currentUser = this.apiService.currentUser;

  // Navigation & Page State
  activePage = signal<'home' | 'login' | 'register' | 'dashboard'>('home');
  dashboardTab = signal<'overview' | 'properties' | 'leases' | 'payments' | 'maintenance' | 'applications'>('overview');

  // Master Data Signals
  properties = signal<any[]>([]);
  people = signal<any[]>([]);
  leases = signal<any[]>([]);
  payments = signal<any[]>([]);
  maintenanceRequests = signal<any[]>([]);
  applications = signal<any[]>([]);

  // Search Filter State
  searchCity = signal('');
  searchBedrooms = signal('');
  searchType = signal('');
  searchMaxPrice = signal('');

  // Form States (stored centrally to preserve state on navigation/switching tabs)
  isEditingProperty = signal<boolean>(false);
  editingPropertyId = signal<string | null>(null);

  loginData = signal({ email: '', password: '' });
  registerData = signal({ firstName: '', lastName: '', email: '', phoneNumber: '', password: '', role: 2 });
  
  newPropertyData = signal({
    address: '',
    city: '',
    province: '',
    type: 0,
    bedrooms: 2,
    bathrooms: 2.0,
    monthlyRent: 2000.0,
    landlordId: '',
    imageUrls: [] as string[],
    description: '',
    size: 1200,
    listingType: 0
  });

  newMaintenanceData = signal({
    propertyId: '',
    description: ''
  });

  newPaymentData = signal({
    leaseId: '',
    amount: 0.0,
    type: 0
  });

  // Feedback State
  authError = signal('');
  authSuccess = signal('');
  dashboardError = signal('');
  dashboardSuccess = signal('');

  // Computed Signals for Role-Based Filtering
  myProperties = computed(() => {
    const user = this.currentUser();
    if (!user) return [];
    const role = user.role;
    if (role === 0) return this.properties(); // Admin sees all
    if (role === 1 || role === 3) return this.properties().filter(p => p.landlordId === user.id); // Landlord / PM see owned
    if (role === 2) return this.properties().filter(p => this.leases().some(l => l.propertyId === p.id && l.tenantId === user.id)); // Tenant sees leased
    return [];
  });

  myLeases = computed(() => {
    const user = this.currentUser();
    if (!user) return [];
    const role = user.role;
    if (role === 0) return this.leases(); // Admin sees all
    if (role === 1 || role === 3) return this.leases().filter(l => this.properties().some(p => p.id === l.propertyId && p.landlordId === user.id)); // Landlord / PM see related
    if (role === 2) return this.leases().filter(l => l.tenantId === user.id); // Tenant sees owned
    return [];
  });

  myPayments = computed(() => {
    const user = this.currentUser();
    if (!user) return [];
    const role = user.role;
    if (role === 0) return this.payments(); // Admin sees all
    if (role === 1 || role === 3 || role === 2) {
      const allowedLeaseIds = new Set(this.myLeases().map(l => l.id));
      return this.payments().filter(p => allowedLeaseIds.has(p.leaseId));
    }
    return [];
  });

  myMaintenanceRequests = computed(() => {
    const user = this.currentUser();
    if (!user) return [];
    const role = user.role;
    if (role === 0) return this.maintenanceRequests(); // Admin sees all
    if (role === 1 || role === 3) {
      const allowedPropIds = new Set(this.myProperties().map(p => p.id));
      return this.maintenanceRequests().filter(r => allowedPropIds.has(r.propertyId));
    }
    if (role === 2) return this.maintenanceRequests().filter(r => r.tenantId === user.id);
    return [];
  });

  myApplications = computed(() => {
    const user = this.currentUser();
    if (!user) return [];
    const role = user.role;
    if (role === 0) return this.applications(); // Admin sees all
    if (role === 1 || role === 3) {
      const myPropIds = new Set(this.myProperties().map(p => p.id));
      return this.applications().filter(a => myPropIds.has(a.propertyId));
    }
    if (role === 2) return this.applications().filter(a => a.tenantId === user.id);
    return [];
  });

  // Computed Filtered Properties for home page
  filteredProperties = computed(() => {
    return this.properties().filter(p => {
      const matchesCity = !this.searchCity() || p.city.toLowerCase().includes(this.searchCity().toLowerCase());
      const matchesBeds = !this.searchBedrooms() || p.bedrooms >= parseInt(this.searchBedrooms());
      const matchesType = !this.searchType() || p.type.toString() === this.searchType();
      const matchesPrice = !this.searchMaxPrice() || p.monthlyRent <= parseFloat(this.searchMaxPrice());
      return matchesCity && matchesBeds && matchesType && matchesPrice;
    });
  });

  constructor() {
    // Initial fetch of properties for landing page
    this.fetchProperties();

    // Effect to auto-load dashboard data when user switches to dashboard page
    effect(() => {
      if (this.activePage() === 'dashboard' && this.currentUser()) {
        this.loadDashboardData();
      }
    });
  }

  // API Methods
  fetchProperties() {
    this.apiService.getProperties().subscribe({
      next: (data) => {
        const mapped = data.map((p: any) => ({ ...p, currentImageIndex: 0 }));
        this.properties.set(mapped);
      },
      error: (err) => console.error('Error fetching properties', err)
    });
  }

  nextImage(p: any, event: Event) {
    event.stopPropagation();
    if (p.imageUrls && p.imageUrls.length > 0) {
      p.currentImageIndex = (p.currentImageIndex + 1) % p.imageUrls.length;
    }
  }

  prevImage(p: any, event: Event) {
    event.stopPropagation();
    if (p.imageUrls && p.imageUrls.length > 0) {
      p.currentImageIndex = (p.currentImageIndex - 1 + p.imageUrls.length) % p.imageUrls.length;
    }
  }

  loadDashboardData() {
    this.fetchProperties();
    
    this.apiService.getLeases().subscribe({
      next: (data) => this.leases.set(data),
      error: (err) => console.error('Error fetching leases', err)
    });
    
    this.apiService.getPayments().subscribe({
      next: (data) => this.payments.set(data),
      error: (err) => console.error('Error fetching payments', err)
    });

    this.apiService.getMaintenanceRequests().subscribe({
      next: (data) => this.maintenanceRequests.set(data),
      error: (err) => console.error('Error fetching maintenance', err)
    });

    this.apiService.getApplications().subscribe({
      next: (data) => this.applications.set(data),
      error: (err) => console.error('Error fetching applications', err)
    });

    if (this.currentUser()?.role === 0) {
      this.apiService.getPeople().subscribe({
        next: (data) => this.people.set(data),
        error: (err) => console.error('Error fetching people', err)
      });
    }
  }

  // Navigation Helper
  navigateTo(page: 'home' | 'login' | 'register' | 'dashboard') {
    this.authError.set('');
    this.authSuccess.set('');
    this.dashboardError.set('');
    this.dashboardSuccess.set('');
    this.activePage.set(page);
  }

  // Auth Operations
  onLogin() {
    this.authError.set('');
    this.apiService.login(this.loginData()).subscribe({
      next: (res) => {
        this.apiService.setSession(res);
        this.authSuccess.set('Successfully logged in! Redirecting...');
        setTimeout(() => {
          this.navigateTo('dashboard');
          this.loginData.set({ email: '', password: '' });
        }, 1000);
      },
      error: (err) => {
        this.authError.set(err.error?.message || 'Invalid credentials. Please try again.');
      }
    });
  }

  onRegister() {
    this.authError.set('');
    
    const payload = {
      ...this.registerData(),
      role: Number(this.registerData().role)
    };

    this.apiService.register(payload).subscribe({
      next: (res) => {
        this.apiService.setSession(res);
        this.authSuccess.set('Account created successfully! Redirecting...');
        setTimeout(() => {
          this.navigateTo('dashboard');
          this.registerData.set({ firstName: '', lastName: '', email: '', phoneNumber: '', password: '', role: 2 });
        }, 1000);
      },
      error: (err) => {
        this.authError.set(err.error?.message || 'Error occurred during registration. Please try again.');
      }
    });
  }

  onLogout() {
    this.apiService.clearSession();
    this.navigateTo('home');
  }

  // Landlord Operations
  editProperty(p: any) {
    this.isEditingProperty.set(true);
    this.editingPropertyId.set(p.id);
    this.newPropertyData.set({
      address: p.address,
      city: p.city,
      province: p.province,
      type: p.type,
      bedrooms: p.bedrooms,
      bathrooms: p.bathrooms,
      monthlyRent: p.monthlyRent,
      landlordId: p.landlordId,
      imageUrls: p.imageUrls ? [...p.imageUrls] : [],
      description: p.description,
      size: p.size,
      listingType: p.listingType
    });
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  cancelEdit() {
    this.isEditingProperty.set(false);
    this.editingPropertyId.set(null);
    this.resetPropertyForm();
  }

  resetPropertyForm() {
    this.newPropertyData.set({
      address: '',
      city: '',
      province: '',
      type: 0,
      bedrooms: 2,
      bathrooms: 2.0,
      monthlyRent: 2000.0,
      landlordId: '',
      imageUrls: [] as string[],
      description: '',
      size: 1200,
      listingType: 0
    });
  }

  submitPropertyForm() {
    this.dashboardError.set('');
    this.dashboardSuccess.set('');
    const landlordId = this.currentUser().id;

    const payload = {
      ...this.newPropertyData(),
      type: Number(this.newPropertyData().type),
      bedrooms: Number(this.newPropertyData().bedrooms),
      bathrooms: Number(this.newPropertyData().bathrooms),
      monthlyRent: Number(this.newPropertyData().monthlyRent),
      size: Number(this.newPropertyData().size),
      listingType: Number(this.newPropertyData().listingType),
      landlordId: landlordId
    };

    if (this.isEditingProperty() && this.editingPropertyId()) {
      this.apiService.updateProperty(this.editingPropertyId()!, payload).subscribe({
        next: () => {
          this.dashboardSuccess.set('Property updated successfully!');
          this.fetchProperties();
          this.cancelEdit();
        },
        error: () => {
          this.dashboardError.set('Failed to update property. Check your inputs.');
        }
      });
    } else {
      this.apiService.createProperty(payload).subscribe({
        next: () => {
          this.dashboardSuccess.set('Property listed successfully!');
          this.fetchProperties();
          this.resetPropertyForm();
        },
        error: () => {
          this.dashboardError.set('Failed to list property. Check your inputs.');
        }
      });
    }
  }

  deleteProperty(id: string) {
    this.dashboardError.set('');
    this.dashboardSuccess.set('');

    this.apiService.deleteProperty(id).subscribe({
      next: () => {
        this.dashboardSuccess.set('Property removed successfully.');
        this.fetchProperties();
      },
      error: () => {
        this.dashboardError.set('Failed to remove property.');
      }
    });
  }

  // Tenant Operations
  submitMaintenance() {
    this.dashboardError.set('');
    this.dashboardSuccess.set('');

    if (!this.newMaintenanceData().propertyId) {
      this.dashboardError.set('Please select your leased property.');
      return;
    }

    const payload = {
      propertyId: this.newMaintenanceData().propertyId,
      tenantId: this.currentUser().id,
      description: this.newMaintenanceData().description
    };

    this.apiService.createMaintenanceRequest(payload).subscribe({
      next: () => {
        this.dashboardSuccess.set('Maintenance request submitted!');
        this.loadDashboardData();
        this.newMaintenanceData.update(state => ({ ...state, description: '' }));
      },
      error: () => {
        this.dashboardError.set('Failed to submit maintenance request.');
      }
    });
  }

  submitPayment() {
    this.dashboardError.set('');
    this.dashboardSuccess.set('');

    if (!this.newPaymentData().leaseId) {
      this.dashboardError.set('Please select an active lease to pay.');
      return;
    }

    const payload = {
      leaseId: this.newPaymentData().leaseId,
      amount: Number(this.newPaymentData().amount),
      type: Number(this.newPaymentData().type),
      paymentDate: new Date().toISOString()
    };

    this.apiService.createPayment(payload).subscribe({
      next: () => {
        this.dashboardSuccess.set('Rent payment processed successfully!');
        this.loadDashboardData();
        this.newPaymentData.update(state => ({ ...state, amount: 0.0 }));
      },
      error: () => {
        this.dashboardError.set('Failed to process payment.');
      }
    });
  }

  // Application Operations
  requestViewing(p: any) {
    this.dashboardError.set('');
    this.dashboardSuccess.set('');

    const payload = {
      propertyId: p.id,
      tenantId: this.currentUser().id,
      type: 2 // ViewingRequest
    };

    this.apiService.createApplication(payload).subscribe({
      next: () => {
        this.dashboardSuccess.set('Viewing request submitted! Waiting for owner approval.');
        this.loadDashboardData();
        this.navigateTo('dashboard');
        this.dashboardTab.set('applications');
      },
      error: (err) => {
        this.dashboardError.set(err.error || 'Failed to submit viewing request.');
      }
    });
  }

  applyForPropertyDirect(propertyId: string, type: number) {
    this.dashboardError.set('');
    this.dashboardSuccess.set('');

    const payload = {
      propertyId: propertyId,
      tenantId: this.currentUser().id,
      type: type // 0 = Rental, 1 = Purchase
    };

    this.apiService.createApplication(payload).subscribe({
      next: () => {
        this.dashboardSuccess.set('Application submitted successfully! Waiting for response from property owner.');
        this.loadDashboardData();
        this.navigateTo('dashboard');
        this.dashboardTab.set('applications');
      },
      error: (err) => {
        this.dashboardError.set(err.error || 'Failed to submit application.');
      }
    });
  }

  approveApplication(app: any) {
    this.dashboardError.set('');
    this.dashboardSuccess.set('');

    this.apiService.updateApplicationStatus(app.id, 1).subscribe({ // 1 = Approved
      next: () => {
        if (app.type === 2) {
          this.dashboardSuccess.set('Viewing request approved. You will receive a call from the property owner.');
        } else {
          this.dashboardSuccess.set('Application approved successfully!');
        }
        this.loadDashboardData();
      },
      error: () => {
        this.dashboardError.set('Failed to approve application.');
      }
    });
  }

  rejectApplication(app: any) {
    this.dashboardError.set('');
    this.dashboardSuccess.set('');

    this.apiService.updateApplicationStatus(app.id, 2).subscribe({ // 2 = Rejected
      next: () => {
        this.dashboardSuccess.set('Application rejected successfully.');
        this.loadDashboardData();
      },
      error: () => {
        this.dashboardError.set('Failed to reject application.');
      }
    });
  }

  // Utility Getters
  getRoleName(role: number): string {
    switch (role) {
      case 0: return 'Admin';
      case 1: return 'Landlord/Owner';
      case 2: return 'Tenant';
      case 3: return 'Property Manager';
      default: return 'User';
    }
  }

  getPropertyTypeName(type: number): string {
    switch (type) {
      case 0: return 'Residential';
      case 1: return 'Commercial';
      case 2: return 'Industrial';
      default: return 'Residential';
    }
  }

  getPropertySpecs(p: any): string {
    const type = Number(p.type);
    switch (type) {
      case 1: // Commercial
        return `💼 ${p.bedrooms} Offices • 🚗 ${p.bathrooms} Parkings`;
      case 2: // Industrial
        return `🏭 ${p.bedrooms} Docks • 📏 ${p.bathrooms} ft Height`;
      default: // Residential (0)
        return `🛏️ ${p.bedrooms} Beds • 🚿 ${p.bathrooms} Baths`;
    }
  }

  getPropertyStatusName(status: number): string {
    switch (status) {
      case 0: return 'Available';
      case 1: return 'Rented';
      case 2: return 'Maintenance';
      default: return 'Available';
    }
  }

  getPaymentStatusName(status: number): string {
    switch (status) {
      case 0: return 'Pending';
      case 1: return 'Completed';
      case 2: return 'Failed';
      default: return 'Pending';
    }
  }

  getPaymentTypeName(type: number): string {
    switch (type) {
      case 0: return 'Rent';
      case 1: return 'Deposit';
      case 2: return 'Maintenance';
      case 3: return 'Other';
      default: return 'Rent';
    }
  }

  getMaintenanceStatusName(status: number): string {
    switch (status) {
      case 0: return 'Open';
      case 1: return 'In Progress';
      case 2: return 'Resolved';
      default: return 'Open';
    }
  }

  getApplicationStatusName(status: number): string {
    switch (status) {
      case 0: return 'Pending';
      case 1: return 'Approved';
      case 2: return 'Rejected';
      default: return 'Pending';
    }
  }

  getApplicationTypeName(type: number): string {
    switch (type) {
      case 0:
        return 'Rental';
      case 1:
        return 'Purchase';
      case 2:
        return 'Viewing Request';
      default:
        return 'Rental';
    }
  }
}
