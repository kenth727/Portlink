import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  email: string;
  fullName?: string;
  roles: string[];
}

export interface UserInfo {
  email: string;
  fullName?: string;
  roles: string[];
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly API_URL = 'http://localhost:5159/api';
  private readonly TOKEN_KEY = 'portlink_token';
  private currentUserSubject = new BehaviorSubject<UserInfo | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    // Check if user is already logged in
    const token = this.getToken();
    if (token) {
      this.loadCurrentUser();
    }
  }

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.API_URL}/auth/login`, { email, password })
      .pipe(
        tap(response => {
          this.setToken(response.token);
          this.currentUserSubject.next({
            email: response.email,
            fullName: response.fullName,
            roles: response.roles
          });
        })
      );
  }

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    this.currentUserSubject.next(null);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  setToken(token: string): void {
    localStorage.setItem(this.TOKEN_KEY, token);
  }

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  getCurrentUser(): UserInfo | null {
    return this.currentUserSubject.value;
  }

  hasRole(role: string): boolean {
    const user = this.getCurrentUser();
    return user?.roles?.includes(role) ?? false;
  }

  private loadCurrentUser(): void {
    this.http.get<UserInfo>(`${this.API_URL}/auth/me`)
      .subscribe({
        next: (user) => this.currentUserSubject.next(user),
        error: () => this.logout()
      });
  }
}
