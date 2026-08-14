import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LibrarianBooks } from './librarian-books';

describe('LibrarianBooks', () => {
  let component: LibrarianBooks;
  let fixture: ComponentFixture<LibrarianBooks>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LibrarianBooks]
    })
    .compileComponents();

    fixture = TestBed.createComponent(LibrarianBooks);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
