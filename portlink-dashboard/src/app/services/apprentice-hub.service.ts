import { Injectable, NgZone } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';

export interface LoadSimulatorMetrics {
  operationsPerSecond: number;
  timestamp: string;
  lastOperation: string;
}

@Injectable({ providedIn: 'root' })
export class PortOperationsHubService {
  private hub?: signalR.HubConnection;

  vessel$ = new BehaviorSubject<any | null>(null);
  berth$ = new BehaviorSubject<any | null>(null);
  portCall$ = new BehaviorSubject<any | null>(null);
  loadSimulatorMetrics$ = new BehaviorSubject<LoadSimulatorMetrics | null>(null);

  // Backwards compatible streams for components expecting "*Changed$" and "portCallDeleted$"
  berthChanged$ = this.berth$;
  portCallChanged$ = this.portCall$;
  portCallDeleted$ = new BehaviorSubject<number | null>(null);

  constructor(private zone: NgZone) {}

  async start(baseUrl = 'http://localhost:5159') {
    if (this.hub) {
      return;
    }

    this.hub = new signalR.HubConnectionBuilder()
      .withUrl(`${baseUrl}/hubs/port-operations`)
      .withAutomaticReconnect()
      .build();

    this.hub.on('VesselChanged', (dto) =>
      this.zone.run(() => this.vessel$.next(dto))
    );
    this.hub.on('BerthChanged', (dto) =>
      this.zone.run(() => this.berth$.next(dto))
    );
    this.hub.on('PortCallChanged', (dto) =>
      this.zone.run(() => this.portCall$.next(dto))
    );
    this.hub.on('LoadSimulatorMetrics', (metrics: LoadSimulatorMetrics) =>
      this.zone.run(() => this.loadSimulatorMetrics$.next(metrics))
    );
    this.hub.on('PortCallDeleted', (id: number) =>
      this.zone.run(() => this.portCallDeleted$.next(id))
    );

    await this.hub.start();
  }

  stop() {
    return this.hub?.stop();
  }
}
