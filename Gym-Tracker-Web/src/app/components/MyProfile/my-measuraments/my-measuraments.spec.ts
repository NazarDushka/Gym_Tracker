import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MyMeasuraments } from './my-measuraments';

describe('MyMeasuraments', () => {
  let component: MyMeasuraments;
  let fixture: ComponentFixture<MyMeasuraments>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MyMeasuraments]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MyMeasuraments);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
