import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject } from 'rxjs';
import { filter, takeUntil } from 'rxjs/operators';
import { HttpClient } from '@angular/common/http';
import { PortOperationsHubService } from '../services/apprentice-hub.service';
import { AuthService } from '../services/auth.service';

// Keep this in sync with the backend enum PortlinkApp.Core.Entities.PortCallStatus
enum PortCallStatus {
  Scheduled = 0,
  Approaching = 1,
  Arrived = 2,
  Berthed = 3,
  InProgress = 4,
  Completed = 5,
  Cancelled = 6,
  Delayed = 7
}

interface PortCall {
  id: number;
  vesselId: number;
  vesselName: string;
  berthId: number;
  berthCode: string;
  status: PortCallStatus | string | number;
  estimatedTimeOfArrival: string;
  estimatedTimeOfDeparture: string;
  cargoDescription?: string;
  cargoQuantity?: number;
  cargoUnit?: string;
}

@Component({
  selector: 'app-port-call-approval',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './port-call-approval.component.html',
  styleUrls: ['./port-call-approval.component.scss']
})
export class PortCallApprovalComponent implements OnInit, OnDestroy {
  pendingPortCalls: PortCall[] = [];
  private destroy$ = new Subject<void>();
  private readonly API_URL = 'http://localhost:5159/api';

  constructor(
    private http: HttpClient,
    private hubService: PortOperationsHubService,
    public authService: AuthService
  ) {}

  ngOnInit(): void {
    // Load queue when user logs in or token is restored
    this.authService.currentUser$
      .pipe(
        takeUntil(this.destroy$),
        filter(user => !!user)
      )
      .subscribe(() => this.loadPendingPortCalls());

    this.setupSignalR();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadPendingPortCalls(): void {
    this.http.get<any>(`${this.API_URL}/portcalls?status=Scheduled&pageSize=100`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.pendingPortCalls = response.items || [];
        },
        error: (err) => console.error('Failed to load pending port calls:', err)
      });
  }

  setupSignalR(): void {
    this.hubService.portCallChanged$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.loadPendingPortCalls());

    this.hubService.portCallDeleted$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.loadPendingPortCalls());
  }

  approvePortCall(portCall: PortCall): void {
    // Treat "Approve" as moving the port call into a berthed/active state
    const approvedStatus = PortCallStatus.Approaching;

    this.http.put(`${this.API_URL}/portcalls/${portCall.id}`, {
      vesselId: portCall.vesselId,
      berthId: portCall.berthId,
      estimatedTimeOfArrival: portCall.estimatedTimeOfArrival,
      estimatedTimeOfDeparture: portCall.estimatedTimeOfDeparture,
      status: approvedStatus,
      cargoDescription: portCall.cargoDescription,
      cargoQuantity: portCall.cargoQuantity,
      cargoUnit: portCall.cargoUnit
    })
    .pipe(takeUntil(this.destroy$))
    .subscribe({
      next: () => {
        console.log('Port call approved:', portCall.id);
        this.loadPendingPortCalls();
      },
      error: (err) => console.error('Failed to approve port call:', err)
    });
  }

  denyPortCall(portCall: PortCall): void {
    const name = portCall.vesselName || 'this vessel';
    if (confirm(`Are you sure you want to deny the port call for ${name}?`)) {
      this.http.delete(`${this.API_URL}/portcalls/${portCall.id}`)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            console.log('Port call denied and deleted:', portCall.id);
            this.loadPendingPortCalls();
          },
          error: (err) => console.error('Failed to deny port call:', err)
        });
    }
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleString('en-US', {
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  getBerthCodes(): string[] {
    const codes = Array.from(
      new Set(
        this.pendingPortCalls
          .map(pc => pc.berthCode)
          .filter(code => !!code)
      )
    );

    return codes.sort((a, b) => a.localeCompare(b));
  }

  getPortCallsForBerth(berthCode: string): PortCall[] {
    return this.pendingPortCalls
      .filter(pc => pc.berthCode === berthCode)
      .sort((a, b) =>
        new Date(a.estimatedTimeOfArrival).getTime() -
        new Date(b.estimatedTimeOfArrival).getTime()
      );
  }
}
