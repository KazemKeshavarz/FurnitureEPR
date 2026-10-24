import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
@Component({
 selector:'app-admin-home',standalone:true,imports:[RouterLink,MatCardModule,MatIconModule],
 templateUrl: './admin-home.component.html',
  styleUrl: './admin-home.component.scss'
}) export class AdminHomeComponent {}