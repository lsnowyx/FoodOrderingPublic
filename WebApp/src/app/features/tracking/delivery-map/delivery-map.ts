import {
  AfterViewInit,
  Component,
  ElementRef,
  Input,
  OnChanges,
  OnDestroy,
  SimpleChanges,
  ViewChild
} from '@angular/core';
import * as L from 'leaflet';

import { CourierLocation } from '../models/courier-location';
import { TrackingMapResponse } from '../models/tracking-map-response';

function isCoordinate(latitude: number | null, longitude: number | null): boolean {
  return latitude !== null
    && longitude !== null
    && Number.isFinite(latitude)
    && latitude >= -90
    && latitude <= 90
    && Number.isFinite(longitude)
    && longitude >= -180
    && longitude <= 180;
}

@Component({
  selector: 'app-delivery-map',
  standalone: true,
  templateUrl: './delivery-map.html',
  styleUrl: './delivery-map.scss'
})
export class DeliveryMap implements AfterViewInit, OnChanges, OnDestroy {
  @Input({ required: true }) mapData!: TrackingMapResponse;
  @Input() courierLocation: CourierLocation | null = null;

  @ViewChild('mapContainer', { static: true })
  private mapContainer!: ElementRef<HTMLDivElement>;

  private map: L.Map | null = null;
  private staticLayers: L.LayerGroup | null = null;
  private courierMarker: L.CircleMarker | null = null;
  private invalidateTimer: number | null = null;

  ngAfterViewInit(): void {
    this.map = L.map(this.mapContainer.nativeElement, {
      zoomControl: true
    });
    this.staticLayers = L.layerGroup().addTo(this.map);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; OpenStreetMap contributors',
      maxZoom: 19
    }).addTo(this.map);

    this.renderStaticLocations();
    this.updateCourierMarker();
    this.invalidateTimer = window.setTimeout(() => this.map?.invalidateSize(), 0);
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (this.map === null) {
      return;
    }

    if (changes['mapData'] !== undefined) {
      this.renderStaticLocations();
    }

    if (changes['mapData'] !== undefined || changes['courierLocation'] !== undefined) {
      this.updateCourierMarker();
    }
  }

  ngOnDestroy(): void {
    if (this.invalidateTimer !== null) {
      window.clearTimeout(this.invalidateTimer);
    }

    this.map?.remove();
    this.map = null;
    this.staticLayers = null;
    this.courierMarker = null;
  }

  private renderStaticLocations(): void {
    if (this.map === null || this.staticLayers === null || this.mapData === undefined) {
      return;
    }

    this.staticLayers.clearLayers();
    const bounds: L.LatLngExpression[] = [];

    if (isCoordinate(this.mapData.restaurantLatitude, this.mapData.restaurantLongitude)) {
      const restaurant: L.LatLngExpression = [
        this.mapData.restaurantLatitude,
        this.mapData.restaurantLongitude
      ];
      L.circleMarker(restaurant, {
        radius: 8,
        color: '#173f7c',
        fillColor: '#2457a6',
        fillOpacity: 1,
        weight: 2
      }).bindTooltip('Restaurant').addTo(this.staticLayers);
      bounds.push(restaurant);
    }

    if (isCoordinate(this.mapData.destinationLatitude, this.mapData.destinationLongitude)) {
      const destination: L.LatLngExpression = [
        this.mapData.destinationLatitude!,
        this.mapData.destinationLongitude!
      ];
      L.circleMarker(destination, {
        radius: 8,
        color: '#1d5b2c',
        fillColor: '#35a353',
        fillOpacity: 1,
        weight: 2
      }).bindTooltip('Delivery destination').addTo(this.staticLayers);
      bounds.push(destination);
    }

    if (bounds.length === 1) {
      this.map.setView(bounds[0], 14);
    } else if (bounds.length > 1) {
      this.map.fitBounds(L.latLngBounds(bounds), {
        padding: [30, 30],
        maxZoom: 15
      });
    }
  }

  private updateCourierMarker(): void {
    if (this.map === null || this.mapData === undefined) {
      return;
    }

    const liveLocation = this.courierLocation?.orderId === this.mapData.orderId
      ? this.courierLocation
      : null;
    const latitude = liveLocation?.latitude ?? this.mapData.courierLatitude;
    const longitude = liveLocation?.longitude ?? this.mapData.courierLongitude;

    if (!isCoordinate(latitude, longitude)) {
      if (this.courierMarker !== null) {
        this.courierMarker.remove();
        this.courierMarker = null;
      }
      return;
    }

    const position: L.LatLngExpression = [latitude!, longitude!];
    if (this.courierMarker === null) {
      this.courierMarker = L.circleMarker(position, {
        radius: 9,
        color: '#8a4b08',
        fillColor: '#f0a53a',
        fillOpacity: 1,
        weight: 3
      }).bindTooltip('Courier').addTo(this.map);
      return;
    }

    this.courierMarker.setLatLng(position);
  }
}
