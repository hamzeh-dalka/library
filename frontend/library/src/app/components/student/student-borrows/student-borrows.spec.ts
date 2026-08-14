import { ComponentFixture, TestBed } from '@angular/core/testing';

import { StudentBorrows } from './student-borrows';

describe('StudentBorrows', () => {
  let component: StudentBorrows;
  let fixture: ComponentFixture<StudentBorrows>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StudentBorrows]
    })
    .compileComponents();

    fixture = TestBed.createComponent(StudentBorrows);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
