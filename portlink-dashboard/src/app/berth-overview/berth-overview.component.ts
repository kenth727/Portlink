import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject } from 'rxjs';
import { filter, takeUntil } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import { PortOperationsHubService } from '../services/apprentice-hub.service';
import { AuthService } from '../services/auth.service';

interface Berth {
  id: number;
  berthCode: string;
  status: string | number;
  maxVesselLength: number;
  maxDraft: number;
  facilities?: string;
  location?: string;
}

interface PortCall {
  id: number;
  vesselId: number;
  vesselName: string;
  berthId: number;
  status: string | number;
  estimatedTimeOfArrival: string;
  estimatedTimeOfDeparture: string;
}

@Component({
  selector: 'app-berth-overview',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './berth-overview.component.html',
  styleUrls: ['./berth-overview.component.scss']
})
export class BerthOverviewComponent implements OnInit, OnDestroy {
  berths: Berth[] = [];
  activePortCalls: PortCall[] = [];
  queuedPortCalls: PortCall[] = [];
  private destroy$ = new Subject<void>();
  selectedBerthId: number | null = null;
  private readonly API_URL = 'http://localhost:5159/api';

  constructor(
    private http: HttpClient,
    private hubService: PortOperationsHubService,
    public authService: AuthService
  ) {}

  ngOnInit(): void {
    // When the user logs in (or if already logged in), load data
    this.authService.currentUser$
      .pipe(
        takeUntil(this.destroy$),
        filter(user => !!user)
      )
      .subscribe(() => {
        this.loadBerths();
        this.loadActivePortCalls();
        this.loadQueuedPortCalls();
      });

    this.setupSignalR();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadBerths(): void {
    this.http.get<any>(`${this.API_URL}/berths?pageSize=100`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.berths = response.items || [];
        },
        error: (err) => console.error('Failed to load berths:', err)
      });
  }

  loadActivePortCalls(): void {
    this.http.get<PortCall[]>(`${this.API_URL}/portcalls/active`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (portCalls) => {
          this.activePortCalls = portCalls;
        },
        error: (err) => console.error('Failed to load active port calls:', err)
      });
  }

  loadQueuedPortCalls(): void {
    // Load only approved upcoming port calls (status = Approaching)
    this.http.get<any>(`${this.API_URL}/portcalls?status=Approaching&pageSize=200`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.queuedPortCalls = response.items || [];
        },
        error: (err) => console.error('Failed to load queued port calls:', err)
      });
  }

  setupSignalR(): void {
    this.hubService.berthChanged$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.loadBerths());

    this.hubService.portCallChanged$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.loadActivePortCalls();
        this.loadQueuedPortCalls();
      });
  }

  getVesselForBerth(berthId: number): string {
    const current = this.getCurrentPortCallForBerth(berthId);
    return current?.vesselName || '';
  }

  getCurrentPortCallForBerth(berthId: number): PortCall | undefined {
    const active = this.activePortCalls.find(pc => pc.berthId === berthId);
    if (active) {
      return active;
    }

    const queue = this.getQueueForBerth(berthId);
    return queue.length > 0 ? queue[0] : undefined;
  }

  getQueueForBerth(berthId: number): PortCall[] {
    return this.queuedPortCalls
      .filter(pc => pc.berthId === berthId)
      .sort((a, b) =>
        new Date(a.estimatedTimeOfArrival).getTime() -
        new Date(b.estimatedTimeOfArrival).getTime());
  }

  toggleBerthDetails(berthId: number): void {
    this.selectedBerthId = this.selectedBerthId === berthId ? null : berthId;
  }

  getActiveCurrentPortCallForBerth(berthId: number): PortCall | undefined {
    return this.activePortCalls.find(pc => pc.berthId === berthId);
  }

  removeQueuedPortCall(portCall: PortCall, event: MouseEvent): void {
    // Prevent card click toggle when clicking the remove button
    event.stopPropagation();

    this.http.delete(`${this.API_URL}/portcalls/${portCall.id}`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => this.loadQueuedPortCalls(),
        error: (err) => console.error('Failed to remove queued port call:', err)
      });
  }

  removeCurrentPortCall(portCall: PortCall, event: MouseEvent): void {
    // Prevent card click toggle when clicking the remove button
    event.stopPropagation();

    this.http.delete(`${this.API_URL}/portcalls/${portCall.id}`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.loadActivePortCalls();
          this.loadQueuedPortCalls();
        },
        error: (err) => console.error('Failed to remove current port call:', err)
      });
  }

  private normalizeStatus(status: string | number | undefined | null): string {
    if (status === null || status === undefined) {
      return '';
    }

    if (typeof status === 'string') {
      return status.toLowerCase();
    }

    // Map numeric enum values from the backend
    switch (status) {
      case 0:
        return 'available';
      case 1:
        return 'occupied';
      case 2:
        return 'reserved';
      case 3:
        return 'undermaintenance';
      case 4:
        return 'closed';
      default:
        return '';
    }
  }

  getBerthStatusClass(status: string | number): string {
    const normalized = this.normalizeStatus(status);

    switch (normalized) {
      case 'available':
        return 'status-available';
      case 'occupied':
      case 'reserved':
        return 'status-occupied';
      case 'undermaintenance':
      case 'maintenance':
      case 'closed':
        return 'status-maintenance';
      default:
        return '';
    }
  }

  getBerthStatusIcon(status: string | number): string {
    const normalized = this.normalizeStatus(status);

    switch (normalized) {
      case 'available':
        return 'A';
      case 'occupied':
      case 'reserved':
        return 'O';
      case 'undermaintenance':
      case 'maintenance':
      case 'closed':
        return 'M';
      default:
        return '?';
    }
  }
}
