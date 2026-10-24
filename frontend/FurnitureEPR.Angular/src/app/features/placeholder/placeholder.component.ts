import { Component, inject } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-placeholder',
  standalone: true,
  imports: [MatCardModule],
  templateUrl: './placeholder.component.html',
  styleUrl: './placeholder.component.scss'
})
export class PlaceholderComponent {
  private readonly route = inject(ActivatedRoute);
  get title(): string { return this.route.snapshot.data['title'] ?? 'بخش سامانه'; }
  get description(): string { return this.route.snapshot.data['description'] ?? ''; }
}
