import { Component, OnDestroy, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { PortOperationsHubService } from './services/apprentice-hub.service';
import { DashboardService } from './services/dashboard.service';
import { AuthService } from './services/auth.service';
import { ChatComponent } from './chat.component';
import { BerthOverviewComponent } from './berth-overview/berth-overview.component';
import { PortCallApprovalComponent } from './port-call-approval/port-call-approval.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, CommonModule, FormsModule, ChatComponent, BerthOverviewComponent, PortCallApprovalComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit, OnDestroy {
  title = 'portlink-maritime-dashboard';

  maritimeEvents: { type: string; payload: any }[] = [];
  activeVesselsCount$;
  availableBerthsCount$;
  loadMetrics$;
  lastAiResponseTimeMs$;

  // Auth
  loginEmail = '';
  loginPassword = '';
  currentUser$;

  private subs: Subscription[] = [];

  constructor(
    private hub: PortOperationsHubService,
    private dashboardService: DashboardService,
    public authService: AuthService
  ) {
    this.activeVesselsCount$ = this.dashboardService.activeVesselsCount$;
    this.availableBerthsCount$ = this.dashboardService.availableBerthsCount$;
    this.loadMetrics$ = this.dashboardService.loadMetrics$;
    this.lastAiResponseTimeMs$ = this.dashboardService.lastAiResponseTimeMs$;
    this.currentUser$ = this.authService.currentUser$;
  }

  async ngOnInit() {
    await this.hub.start('http://localhost:5159');
    this.dashboardService.init();

    this.subs.push(
      this.hub.vessel$.subscribe((v) => {
        if (v) {
          this.maritimeEvents.unshift({ type: 'Vessel', payload: v });
        }
      }),
      this.hub.berth$.subscribe((b) => {
        if (b) {
          this.maritimeEvents.unshift({ type: 'Berth', payload: b });
        }
      }),
      this.hub.portCall$.subscribe((pc) => {
        if (pc) {
          this.maritimeEvents.unshift({ type: 'PortCall', payload: pc });
        }
      }),
      this.hub.loadSimulatorMetrics$.subscribe((m) => {
        if (m) {
          this.maritimeEvents.unshift({ type: 'LoadSimulatorMetrics', payload: m });
        }
      })
    );
  }

  ngOnDestroy(): void {
    this.subs.forEach((s) => s.unsubscribe());
    this.hub.stop();
  }

  startLoadSimulator(): void {
    this.dashboardService.startSimulator(2).subscribe();
  }

  stopLoadSimulator(): void {
    this.dashboardService.stopSimulator().subscribe();
  }

  login(): void {
    if (this.loginEmail && this.loginPassword) {
      this.authService.login(this.loginEmail, this.loginPassword).subscribe({
        next: () => {
          console.log('Login successful');
          this.loginPassword = '';
        },
        error: (err) => {
          console.error('Login failed:', err);
          alert('Login failed. Please check your credentials.');
        }
      });
    }
  }

  logout(): void {
    this.authService.logout();
  }
}
