export interface Product {
    id: number;
    name: string;
    description: string;
    price: number;
    createdAt: string;
}

export interface ProductFormData {
    name: string;
    description: string;
    price: number;
}

export interface ProductSearchParams {
    name?: string;
    minPrice?: number;
    maxPrice?: number;
}
