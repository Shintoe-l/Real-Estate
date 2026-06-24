import { Component, inject, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RealEstateStore } from '../../../../services/real-estate-store.service';

@Component({
  selector: 'app-properties-tab',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './properties-tab.html'
})
export class PropertiesTabComponent {
  public store = inject(RealEstateStore);

  newPropertyData = {
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
  };

  constructor() {
    // Automatically sync form values when edit action or resets trigger state changes in the store
    effect(() => {
      const data = this.store.newPropertyData();
      this.newPropertyData = JSON.parse(JSON.stringify(data));
    });
  }

  submitPropertyForm() {
    this.store.newPropertyData.set(this.newPropertyData);
    this.store.submitPropertyForm();
  }

  onFileSelected(event: any) {
    const files = event.target.files;
    if (files && files.length > 0) {
      this.newPropertyData.imageUrls = [];
      Array.from(files).forEach((file: any) => {
        const reader = new FileReader();
        reader.onload = (e: any) => {
          this.newPropertyData.imageUrls.push(e.target.result);
        };
        reader.readAsDataURL(file);
      });
    }
  }
}
