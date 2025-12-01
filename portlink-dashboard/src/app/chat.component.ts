import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DashboardService, ChatResponse } from './services/dashboard.service';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.scss']
})
export class ChatComponent {
  question = '';
  lastResponse: ChatResponse | null = null;
  loading = false;
  error: string | null = null;

  constructor(
    private dashboard: DashboardService,
    public authService: AuthService
  ) {}

  send(): void {
    const trimmed = this.question.trim();
    if (!trimmed || this.loading) {
      return;
    }

    this.loading = true;
    this.error = null;

    this.dashboard.chat(trimmed).subscribe({
      next: (response) => {
        this.lastResponse = response;
        this.loading = false;
      },
      error: () => {
        this.error = 'AI service is unavailable. Check LM Studio and the API.';
        this.loading = false;
      }
    });
  }
}
