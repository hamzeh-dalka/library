import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LibrarianBorrows } from './librarian-borrows';

describe('LibrarianBorrows', () => {
  let component: LibrarianBorrows;
  let fixture: ComponentFixture<LibrarianBorrows>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LibrarianBorrows]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LibrarianBorrows);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
