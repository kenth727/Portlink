import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { PortOperationsHubService, LoadSimulatorMetrics } from './apprentice-hub.service';

export interface ChatResponse {
  question: string;
  answer: string;
  timestamp: string;
}

interface PortCallDto {
  id: number;
  status: string;
}

interface BerthDto {
  id: number;
  status: string;
}

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly apiBase = 'http://localhost:5159';

  readonly activeVesselsCount$ = new BehaviorSubject<number>(0);
  readonly availableBerthsCount$ = new BehaviorSubject<number>(0);
  readonly loadMetrics$ = new BehaviorSubject<LoadSimulatorMetrics | null>(null);
  readonly lastAiResponseTimeMs$ = new BehaviorSubject<number | null>(null);

  constructor(
    private http: HttpClient,
    private hub: PortOperationsHubService
  ) {
    this.hub.loadSimulatorMetrics$.subscribe((metrics) => {
      if (metrics) {
        this.loadMetrics$.next(metrics);
      }
    });

    this.hub.portCall$.subscribe(() => this.refreshSnapshot());
    this.hub.berth$.subscribe(() => this.refreshSnapshot());
  }

  init(): void {
    this.refreshSnapshot();
  }

  refreshSnapshot(): void {
    this.http
      .get<PortCallDto[]>(`${this.apiBase}/api/portcalls/active`)
      .subscribe({
        next: (portCalls) => this.activeVesselsCount$.next(portCalls.length),
        error: () => this.activeVesselsCount$.next(0)
      });

    this.http
      .get<BerthDto[]>(`${this.apiBase}/api/berths/available`)
      .subscribe({
        next: (berths) => this.availableBerthsCount$.next(berths.length),
        error: () => this.availableBerthsCount$.next(0)
      });
  }

  chat(question: string): Observable<ChatResponse> {
    const started = performance.now();

    return this.http
      .post<ChatResponse>(`${this.apiBase}/api/ai/chat`, { question })
      .pipe(
        tap(() => {
          const elapsed = performance.now() - started;
          this.lastAiResponseTimeMs$.next(elapsed);
        })
      );
  }

  startSimulator(operationsPerSecond: number): Observable<unknown> {
    return this.http.post(
      `${this.apiBase}/api/load-simulator/start`,
      {},
      { params: { operationsPerSecond } }
    );
  }

  stopSimulator(): Observable<unknown> {
    return this.http.post(`${this.apiBase}/api/load-simulator/stop`, {});
  }
}
