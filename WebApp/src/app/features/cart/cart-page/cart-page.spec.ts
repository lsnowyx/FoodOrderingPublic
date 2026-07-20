import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { CartPage } from './cart-page';

describe('CartPage', () => {
  let fixture: ComponentFixture<CartPage>;

  beforeEach(async () => {
    localStorage.clear();
    await TestBed.configureTestingModule({
      imports: [CartPage],
      providers: [provideRouter([])]
    }).compileComponents();

    fixture = TestBed.createComponent(CartPage);
    fixture.detectChanges();
  });

  afterEach(() => {
    localStorage.clear();
  });

  it('creates the cart page', () => {
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('shows the empty-cart state', () => {
    const element = fixture.nativeElement as HTMLElement;

    expect(element.querySelector('.empty-cart')?.textContent)
      .toContain('Your cart is empty');
  });
});
