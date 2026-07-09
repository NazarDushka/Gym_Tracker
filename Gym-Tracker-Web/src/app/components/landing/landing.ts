import { Component, inject, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { WarmUpService } from '../../data/service/warm-up.service';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './landing.html',
  styleUrl: './landing.scss'
})
export class LandingComponent implements OnInit {
  private warmUpService = inject(WarmUpService);

  ngOnInit(): void {
    this.warmUpService.warmUp();
  }
}
