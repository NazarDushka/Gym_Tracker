//Временная заглушка





import { Component } from '@angular/core';
// Импортируем недостающие пайпы (решение ошибки NG8004)
import { UpperCasePipe, DatePipe } from '@angular/common'; 

@Component({
  selector: 'app-my-profile',
  standalone: true,
  imports: [UpperCasePipe, DatePipe], // Обязательно добавляем их сюда
  templateUrl: './my-profile.html',
  styleUrls: ['./my-profile.css'] // или .scss, в зависимости от твоих настроек
})
export class MyProfile {
  // 1. Решение ошибки TS2339: Добавляем свойство user
  user = {
    name: 'Имя Пользователя',
    joinDate: new Date('2025-01-01') // Заглушка даты
  };

  // 2. Решение ошибки TS2339: Добавляем метрики
  metrics = {
    height: 180,
    weight: 85.5,
    bodyFat: 12.5 // Цель 8% всё ближе!
  };

  // 3. Решение ошибки TS2339: Массив для @for
  recentRecords = [
    { id: 1, exercise: 'Жим лежа', value: '100 кг', date: '2026-03-08' },
    { id: 2, exercise: 'Присед', value: '140 кг', date: '2026-03-09' }
  ];

  // 4. Решение ошибки TS2339: Обработчики кликов
  viewAllMeasurements(): void {
    console.log('Клик по кнопке: Показать все замеры');
    // Здесь позже будет логика навигации, например: this.router.navigate(['/measurements']);
  }

  viewAllRecords(): void {
    console.log('Клик по кнопке: Показать все рекорды');
  }
}