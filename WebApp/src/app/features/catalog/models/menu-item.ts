export interface MenuItem {
  id: string;
  name: string;
  description: string | null;
  price: number;
  totalCalories: number;
  categoryId: string;
  displayPictureUrl: string | null;
}
