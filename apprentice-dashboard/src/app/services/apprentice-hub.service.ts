import { Injectable, NgZone } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';

export interface ApprenticeEvent {
  action: 'created' | 'updated' | 'deleted';
  apprentice?: any;
  apprenticeId?: number;
}

export interface AssignmentEvent {
  action: 'created' | 'updated';
  apprenticeId: number;
  assignment: any;
}

@Injectable({ providedIn: 'root' })
export class ApprenticeHubService {
  private hub?: signalR.HubConnection;
  apprentice$ = new BehaviorSubject<ApprenticeEvent | null>(null);
  assignment$ = new BehaviorSubject<AssignmentEvent | null>(null);

  constructor(private zone: NgZone) {}

  async start(baseUrl = 'http://localhost:5000') {
    if (this.hub) {
      return;
    }

    this.hub = new signalR.HubConnectionBuilder()
      .withUrl(`${baseUrl}/hubs/apprentices`)
      .withAutomaticReconnect()
      .build();

    this.hub.on('ApprenticeChanged', (evt) => this.zone.run(() => this.apprentice$.next(evt)));
    this.hub.on('AssignmentChanged', (evt) => this.zone.run(() => this.assignment$.next(evt)));

    await this.hub.start();
  }

  stop() {
    return this.hub?.stop();
  }
}
