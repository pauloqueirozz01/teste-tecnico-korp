import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { EmptyStateComponent } from '../../components/shared/empty-state/empty-state';

@Component({
  selector: 'app-not-found-screen',
  imports: [RouterLink, EmptyStateComponent],
  templateUrl: './not-found.html',
  styleUrl: './not-found.css'
})
export class NotFoundScreen {}
