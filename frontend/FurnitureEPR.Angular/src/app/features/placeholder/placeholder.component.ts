import { Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-placeholder',
  standalone: true,
  imports: [MatCardModule],
  template: `
    <section class="placeholder">
      <span>Furniture EPR</span>
      <h1>{{ title }}</h1>
      <mat-card>
        <strong>این بخش در حال آماده‌سازی است</strong>
        <p>{{ description }}</p>
      </mat-card>
    </section>
  `,
  styles: [`
    .placeholder { max-width: 900px; margin: 0 auto; }
    .placeholder > span { color: #286f93; font-size: 12px; font-weight: 700; }
    h1 { margin: 6px 0 18px; color: #102a35; font-size: 25px; }
    mat-card { padding: 24px; border-radius: 18px; }
    strong { color: #102a35; }
    p { color: #68777d; line-height: 2; margin: 8px 0 0; }
  `]
})
export class PlaceholderComponent {
  private readonly route = inject(ActivatedRoute);
  get title(): string { return this.route.snapshot.data['title'] ?? 'بخش سامانه'; }
  get description(): string { return this.route.snapshot.data['description'] ?? ''; }
}
