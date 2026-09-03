import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PageHeaderComponent } from '../../components/shared/page-header/page-header';

@Component({
  selector: 'app-dashboard-screen',
  imports: [RouterLink, PageHeaderComponent],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class DashboardScreen {}
