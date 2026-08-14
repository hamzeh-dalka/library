import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RegisterStudent } from './register-student';

describe('RegisterStudent', () => {
  let component: RegisterStudent;
  let fixture: ComponentFixture<RegisterStudent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RegisterStudent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RegisterStudent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
