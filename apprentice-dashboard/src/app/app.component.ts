import { Component, OnDestroy, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { ApprenticeHubService, ApprenticeEvent, AssignmentEvent } from './services/apprentice-hub.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, CommonModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit, OnDestroy {
  title = 'apprentice-dashboard';
  events: (ApprenticeEvent | AssignmentEvent)[] = [];
  private subs: Subscription[] = [];

  constructor(private hub: ApprenticeHubService) {}

  async ngOnInit() {
    await this.hub.start();
    this.subs.push(
      this.hub.apprentice$.subscribe(evt => evt && this.events.unshift(evt)),
      this.hub.assignment$.subscribe(evt => evt && this.events.unshift(evt))
    );
  }

  ngOnDestroy(): void {
    this.subs.forEach(s => s.unsubscribe());
    this.hub.stop();
  }
}
