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

  // Server-computed lease summary (stored in DB, calculated server-side)
  tenantLeaseSummary = signal<{ activeLeaseCount: number; totalMonthlyObligation: number } | null>(null);

  // Server-computed transactions total (calculated server-side in DB)
  dbTotalPayments = signal<number>(0);

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
  showVerificationScreen = signal<boolean>(false);
  verificationEmail = signal<string>('');
  
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
    type: 0,
    method: 0,
    reference: ''
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
    if (role === 2) {
      // Tenant sees leased properties OR properties they have acquired via approved payment
      return this.properties().filter(p => 
        this.leases().some(l => l.propertyId === p.id && l.tenantId === user.id) ||
        this.applications().some(a => a.propertyId === p.id && a.tenantId === user.id && a.status === 3)
      );
    }
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

  myTotalMonthlyObligation = computed(() => {
    return this.myLeases().reduce((sum, lease) => sum + (lease.monthlyRent || 0), 0);
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

  myTotalPayments = computed(() => {
    return this.dbTotalPayments();
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
    const user = this.currentUser();
    if (!user) return;

    if (user.role === 0) {
      // ── Admin: fetch everything ──────────────────────────────────────────
      this.apiService.getProperties().subscribe({
        next: (data) => this.properties.set(data.map((p: any) => ({ ...p, currentImageIndex: 0 }))),
        error: (err) => console.error('Error fetching properties', err)
      });
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
      this.apiService.getPeople().subscribe({
        next: (data) => this.people.set(data),
        error: (err) => console.error('Error fetching people', err)
      });

    } else if (user.role === 1) {
      // ── Landlord: fetch properties they own, then chain secondary fetches ──
      this.apiService.getPropertiesByLandlord(user.id).subscribe({
        next: (data) => {
          this.properties.set(data.map((p: any) => ({ ...p, currentImageIndex: 0 })));
          const propIds = data.map((p: any) => p.id as string);
          if (propIds.length === 0) return;

          // Leases for those properties
          this.apiService.getLeases().subscribe({
            next: (leases) => {
              const filtered = leases.filter((l: any) => propIds.includes(l.propertyId));
              this.leases.set(filtered);
              const leaseIds = filtered.map((l: any) => l.id as string);

              // Payments for those leases
              if (leaseIds.length > 0) {
                this.apiService.getPaymentsByLeaseIds(leaseIds).subscribe({
                  next: (p) => this.payments.set(p),
                  error: (err) => console.error('Error fetching landlord payments', err)
                });
                this.apiService.getPaymentsSummary(leaseIds).subscribe({
                  next: (summary) => this.dbTotalPayments.set(summary.totalAmount || 0),
                  error: (err) => console.error('Error fetching landlord payments summary', err)
                });
              }
            },
            error: (err) => console.error('Error fetching landlord leases', err)
          });

          // Maintenance requests for those properties
          this.apiService.getMaintenanceByPropertyIds(propIds).subscribe({
            next: (data) => this.maintenanceRequests.set(data),
            error: (err) => console.error('Error fetching landlord maintenance', err)
          });

          // Applications for those properties
          this.apiService.getApplicationsByPropertyIds(propIds).subscribe({
            next: (data) => this.applications.set(data),
            error: (err) => console.error('Error fetching landlord applications', err)
          });

          // People — needed so the Application Detail modal can resolve applicant names/contact info
          this.apiService.getPeople().subscribe({
            next: (data) => this.people.set(data),
            error: (err) => console.error('Error fetching people for landlord', err)
          });
        },
        error: (err) => console.error('Error fetching landlord properties', err)
      });

    } else if (user.role === 2) {
      // ── Tenant: fetch all data scoped to their ID ─────────────────────────
      // Keep browsable properties available (all properties for browsing)
      this.fetchProperties();

      // Leases directly from DB by tenant
      this.apiService.getLeasesByTenant(user.id).subscribe({
        next: (leases) => {
          this.leases.set(leases);
          // Payments for those leases
          const leaseIds = leases.map((l: any) => l.id as string);
          if (leaseIds.length > 0) {
            this.apiService.getPaymentsByLeaseIds(leaseIds).subscribe({
              next: (p) => this.payments.set(p),
              error: (err) => console.error('Error fetching tenant payments', err)
            });
            this.apiService.getPaymentsSummary(leaseIds).subscribe({
              next: (summary) => this.dbTotalPayments.set(summary.totalAmount || 0),
              error: (err) => console.error('Error fetching tenant payments summary', err)
            });
          }
        },
        error: (err) => console.error('Error fetching tenant leases', err)
      });

      // DB-computed lease summary (totalMonthlyObligation)
      this.apiService.getTenantLeaseSummary(user.id).subscribe({
        next: (summary) => this.tenantLeaseSummary.set(summary),
        error: (err) => console.error('Error fetching tenant lease summary', err)
      });

      // Maintenance requests by tenant
      this.apiService.getMaintenanceByTenant(user.id).subscribe({
        next: (data) => this.maintenanceRequests.set(data),
        error: (err) => console.error('Error fetching tenant maintenance', err)
      });

      // Applications by tenant
      this.apiService.getApplicationsByTenant(user.id).subscribe({
        next: (data) => this.applications.set(data),
        error: (err) => console.error('Error fetching tenant applications', err)
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
        if (err.error?.requiresVerification) {
          this.verificationEmail.set(err.error.email);
          this.showVerificationScreen.set(true);
          this.navigateTo('register');
          this.authError.set(err.error.message);
        } else {
          this.authError.set(err.error?.message || 'Invalid credentials. Please try again.');
        }
      }
    });
  }

  onRegister() {
    this.authError.set('');
    this.authSuccess.set('');
    
    const payload = {
      ...this.registerData(),
      role: Number(this.registerData().role)
    };

    this.apiService.register(payload).subscribe({
      next: (res: any) => {
        this.verificationEmail.set(res.email);
        this.showVerificationScreen.set(true);
        this.authSuccess.set('Registration successful! Please enter the confirmation code sent to your email.');
        this.registerData.update(d => ({ ...d, password: '' }));
      },
      error: (err) => {
        this.authError.set(err.error?.message || 'Error occurred during registration. Please try again.');
      }
    });
  }

  onVerifyEmail(code: string) {
    this.authError.set('');
    this.authSuccess.set('');
    
    const payload = {
      email: this.verificationEmail(),
      code: code
    };

    this.apiService.verifyEmail(payload).subscribe({
      next: (res: any) => {
        this.apiService.setSession(res);
        this.authSuccess.set('Email verified successfully! Logging you in...');
        setTimeout(() => {
          this.showVerificationScreen.set(false);
          this.navigateTo('dashboard');
        }, 1000);
      },
      error: (err) => {
        this.authError.set(err.error?.message || 'Verification failed. Please check the code.');
      }
    });
  }

  onResendVerification() {
    this.authError.set('');
    this.authSuccess.set('');

    const payload = {
      email: this.verificationEmail()
    };

    this.apiService.resendVerification(payload).subscribe({
      next: (res: any) => {
        this.authSuccess.set('A new verification code has been sent to your email.');
      },
      error: (err) => {
        this.authError.set(err.error?.message || 'Failed to resend verification code.');
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

    console.log('[submitPropertyForm] payload:', JSON.stringify(payload, null, 2));
    console.log('[submitPropertyForm] currentUser:', this.currentUser());

    if (this.isEditingProperty() && this.editingPropertyId()) {
      this.apiService.updateProperty(this.editingPropertyId()!, payload).subscribe({
        next: () => {
          this.dashboardSuccess.set('Property updated successfully!');
          this.fetchProperties();
          this.cancelEdit();
        },
        error: (err) => {
          console.error('[updateProperty] error:', err);
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
        error: (err) => {
          console.error('[createProperty] error:', err);
          console.error('[createProperty] error body:', err?.error);
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

  updateMaintenanceRequestStatus(id: string, status: number) {
    this.dashboardError.set('');
    this.dashboardSuccess.set('');

    this.apiService.updateMaintenanceRequest(id, status).subscribe({
      next: () => {
        this.dashboardSuccess.set('Maintenance request status updated!');
        this.loadDashboardData();
      },
      error: () => {
        this.dashboardError.set('Failed to update maintenance request status.');
      }
    });
  }

  rateMaintenanceRequest(id: string, isSatisfied: boolean) {
    this.dashboardError.set('');
    this.dashboardSuccess.set('');

    this.apiService.rateMaintenanceRequest(id, isSatisfied).subscribe({
      next: () => {
        this.dashboardSuccess.set('Feedback submitted successfully!');
        this.loadDashboardData();
      },
      error: () => {
        this.dashboardError.set('Failed to submit feedback.');
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
      method: Number(this.newPaymentData().method),
      reference: this.newPaymentData().reference,
      paymentDate: new Date().toISOString()
    };

    this.apiService.createPayment(payload).subscribe({
      next: (res: any) => {
        this.dashboardSuccess.set('Rent payment processed successfully!');
        this.downloadReceipt(payload, res.id);
        this.loadDashboardData();
        this.newPaymentData.update(state => ({ ...state, amount: 0.0, reference: '' }));
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

  proceedAfterViewing(app: any, type: number) {
    // type: 0 = Rent, 1 = Purchase, 2 = Not Interested (reject the application)
    this.dashboardError.set('');
    this.dashboardSuccess.set('');

    if (type === 2) {
      // Tenant chose "Not Interested" - reject the viewing application
      this.apiService.updateApplicationStatus(app.id, 2).subscribe({
        next: () => {
          this.dashboardSuccess.set('You have declined the property. No further action needed.');
          this.loadDashboardData();
        },
        error: () => {
          this.dashboardError.set('Failed to update application.');
        }
      });
      return;
    }

    // Submit a new Rental (0) or Purchase (1) application for the same property
    const payload = {
      propertyId: app.propertyId,
      tenantId: this.currentUser().id,
      type: type
    };

    // First update the viewing application status to 3 (Acquired / Completed) to disable the action buttons
    this.apiService.updateApplicationStatus(app.id, 3).subscribe({
      next: () => {
        this.apiService.createApplication(payload).subscribe({
          next: () => {
            const label = type === 0 ? 'Rental' : 'Purchase';
            this.dashboardSuccess.set(`${label} application submitted! The owner will review it.`);
            this.loadDashboardData();
          },
          error: (err) => {
            this.dashboardError.set(err.error?.message || 'Failed to submit application.');
          }
        });
      },
      error: () => {
        this.dashboardError.set('Failed to update viewing application status.');
      }
    });
  }
  submitPaymentFromApplication(app: any, amount: number, methodLabel: string, reference: string) {
    this.dashboardError.set('');
    this.dashboardSuccess.set('');

    // Map method label string from the modal to the PaymentMethod enum int
    const methodMap: { [key: string]: number } = {
      'EcoCash': 5, 'OneMoney': 5, 'TeleCash': 5,
      'Bank Transfer': 1, 'ZIPIT': 1, 'Swipe/POS': 0,
      'Credit Card': 0, 'Cash': 2, 'PayPal': 3,
      'Receipt Upload': 4
    };
    const methodInt = methodMap[methodLabel] ?? 4;

    // Update Application Status to Acquired (3)
    this.apiService.updateApplicationStatus(app.id, 3).subscribe({
      next: () => {
        // Wait a tick to let the backend create the purchase/rental lease before querying
        setTimeout(() => {
          this.apiService.getLeasesByTenant(this.currentUser().id).subscribe({
            next: (leases) => {
              this.leases.set(leases);
              const lease = leases.find((l: any) => l.propertyId === app.propertyId);
              const leaseId = lease?.id;

              if (!leaseId) {
                // Backend may still be processing; show success but note payment not linked
                this.dashboardSuccess.set(`✅ Status updated to Acquired. Reference: ${reference}. Payment ledger sync pending.`);
                this.loadDashboardData();
                return;
              }

              // PaymentType: 5 = PurchasePayment for type===1 (Purchase), 0 = Rent for type===0
              const paymentType = app.type === 1 ? 5 : 0;

              const payload = {
                leaseId,
                amount: Number(amount) || 0,
                type: paymentType,
                method: methodInt,
                reference: reference,
                paymentDate: new Date().toISOString()
              };

              this.apiService.createPayment(payload).subscribe({
                next: (res: any) => {
                  this.dashboardSuccess.set(`✅ Payment of $${amount} via ${methodLabel} submitted. Ref: ${reference}. Status updated to Acquired!`);
                  this.downloadReceipt(payload, res.id);
                  this.loadDashboardData();
                },
                error: () => {
                  this.dashboardError.set('Payment registration in ledger failed, but application status updated.');
                  this.loadDashboardData();
                }
              });
            },
            error: () => {
              this.dashboardSuccess.set(`✅ Status updated to Acquired, but could not fetch agreement to record payment.`);
              this.loadDashboardData();
            }
          });
        }, 800); // allow backend time to create the purchase lease record
      },
      error: () => {
        this.dashboardError.set('Failed to update application status to Acquired.');
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

  getPaymentMethodName(method: number): string {
    switch (method) {
      case 0: return 'Credit Card';
      case 1: return 'Bank Transfer';
      case 2: return 'Cash';
      case 3: return 'PayPal';
      case 4: return 'Other';
      case 5: return 'EcoCash';
      default: return 'Other';
    }
  }

  getPropertyStatusName(status: number): string {
    switch (status) {
      case 0: return 'Available';
      case 1: return 'Rented';
      case 2: return 'Maintenance';
      case 3: return 'Off Market';
      case 4: return 'Sold';
      default: return 'Available';
    }
  }

  downloadReceipt(paymentPayload: any, generatedId: string) {
    const user = this.currentUser();
    const tenantName = user ? `${user.firstName} ${user.lastName}` : 'Tenant';
    const lease = this.leases().find(l => l.id === paymentPayload.leaseId);
    const property = lease ? this.properties().find(p => p.id === lease.propertyId) : null;
    const address = property ? property.address : 'Leased Property';
    const methodStr = this.getPaymentMethodName(paymentPayload.method);
    const dateStr = new Date(paymentPayload.paymentDate).toLocaleString();

    const receiptContent = `=============================================
             OFFICIAL RECEIPT
=============================================
Transaction ID : ${generatedId || 'PENDING'}
Date           : ${dateStr}
Received From  : ${tenantName}
Property       : ${address}
---------------------------------------------
Payment Method : ${methodStr}
Reference / POP: ${paymentPayload.reference || 'N/A'}
Amount Paid    : $${paymentPayload.amount.toFixed(2)}
---------------------------------------------
Thank you for your payment!
=============================================`;

    const blob = new Blob([receiptContent], { type: 'text/plain' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `Receipt_${generatedId ? generatedId.substring(0,8) : Date.now()}.txt`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
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
      case 1: return 'Security Deposit';
      case 2: return 'Late Fee';
      case 3: return 'Maintenance Fee';
      case 4: return 'Other';
      case 5: return 'Purchase Payment';
      default: return 'Other';
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
      case 3: return 'Acquired';
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

  getAcquiredApplicationForProperty(property: any) {
    return this.myApplications().find(a => a.propertyId === property.id && a.status === 3 && (a.type === 0 || a.type === 1)) || null;
  }

  hasPendingOrApprovedViewingRequest(property: any): boolean {
    return this.myApplications().some(a => 
      a.propertyId === property.id && 
      a.type === 2 && 
      (a.status === 0 || a.status === 1 || a.status === 3)
    );
  }

  getPropertyListingType(propertyId: string): number {
    const p = this.properties().find(prop => prop.id === propertyId);
    return p ? p.listingType : 0; // default to 0 (Rent) if not found
  }

  getPropertyDetails(propertyId: string): any | null {
    return this.properties().find(p => p.id === propertyId) || null;
  }
}
