import { Component, inject, OnInit } from '@angular/core';
import { MeasurementService } from '../../../../Data/Services/measurement-service';
import { ReactiveFormsModule, FormGroup, FormControl, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../../Data/Services/auth.service';
import { MeasurementLog } from '../../../../Data/Services/Interfaces/body-measurements';

@Component({
  selector: 'app-add-measurement',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './add-measurement.html',
  styleUrl: './add-measurement.scss'
})
export class AddMeasurement implements OnInit {
  router = inject(Router);
  route = inject(ActivatedRoute); // Добавили для чтения URL
  measurementService = inject(MeasurementService);
  authService = inject(AuthService);
  
  // Переменные для красивого отображения в HTML
  typeName = '';
  unit = '';

  // Форма осталась той же, но measurementTypeId мы заполним скрытно
  measurementForm = new FormGroup({
    measurementTypeId: new FormControl('', Validators.required), 
    date: new FormControl(new Date().toISOString().split('T')[0], Validators.required),
    value: new FormControl(0, [Validators.required, Validators.min(0)])
  });

  ngOnInit() {
    // 1. Читаем параметры из URL (.../add-measurement?typeId=xxxx)
    this.route.queryParams.subscribe(params => {
      const typeId = params['typeId'];
      
      if (typeId) {
        // Скрытно подставляем ID в нашу форму
        this.measurementForm.patchValue({ measurementTypeId: typeId });

        // 2. Находим тип по ID, чтобы красиво вывести название и единицу измерения в HTML
        this.measurementService.getMeasurementTypes().subscribe(types => {
          const type = types.find(t => t.id === typeId);
          if (type) {
            this.typeName = type.name;
            this.unit = type.unit;
          }
        });
      }
    });
  }

  onSubmit() {
      const log: MeasurementLog = {
      userId: this.authService.getUserINFOFromToken().userId!,
      measurementTypeId: this.measurementForm.value.measurementTypeId!,
      value: this.measurementForm.value.value!,
      date: this.measurementForm.value.date!
    };
    if (this.measurementForm.valid) {
      this.measurementService.addMeasurementLog(log).subscribe({
        next: () => {
          this.router.navigate(['/measurements']);
        }
      });
    }
  }

  onCancel() {
    this.router.navigate(['/measurements']);
  }
}