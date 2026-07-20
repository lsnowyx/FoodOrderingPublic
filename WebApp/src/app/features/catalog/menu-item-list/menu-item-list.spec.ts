import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MenuItemList } from './menu-item-list';

describe('MenuItemList', () => {
  let component: MenuItemList;
  let fixture: ComponentFixture<MenuItemList>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MenuItemList],
    }).compileComponents();

    fixture = TestBed.createComponent(MenuItemList);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
