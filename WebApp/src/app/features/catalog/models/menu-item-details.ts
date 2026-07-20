export interface CatalogCategorySummary {
  id: string;
  name: string;
}

export interface MenuItemPicture {
  id: string;
  imageUrl: string;
  caption: string | null;
}

export interface MenuItemIngredient {
  ingredientId: string;
  name: string;
  allergenInfo: string | null;
}

export interface MenuItemDetails {
  id: string;
  name: string;
  description: string | null;
  price: number;
  totalCalories: number;
  category: CatalogCategorySummary;
  pictures: MenuItemPicture[];
  ingredients: MenuItemIngredient[];
}
