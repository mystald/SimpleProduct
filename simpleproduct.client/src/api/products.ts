import type { Product, ProductFormData, ProductSearchParams } from '../types/product';
import { getToken, removeToken } from './auth';

const getAuthHeaders = (): HeadersInit => {
    const token = getToken();
    const headers: Record<string, string> = {
        'Content-Type': 'application/json'
    };
    if (token) {
        headers['Authorization'] = `Bearer ${token}`;
    }
    return headers;
};

const handleResponse = async <T>(response: Response): Promise<T> => {
    if (response.status === 401) {
        removeToken();
        throw new Error('Unauthorized or session expired. Please log in again.');
    }

    if (!response.ok) {
        let errorMsg = `Request failed (${response.status})`;
        try {
            const err = await response.json();
            errorMsg = err.message || JSON.stringify(err);
        } catch {
            // fallback
        }
        throw new Error(errorMsg);
    }

    if (response.status === 204) {
        return {} as T;
    }

    return await response.json();
};

export const getProducts = async (): Promise<Product[]> => {
    const res = await fetch('/api/products', {
        headers: getAuthHeaders()
    });
    return handleResponse<Product[]>(res);
};

export const getProductById = async (id: number): Promise<Product> => {
    const res = await fetch(`/api/products/${id}`, {
        headers: getAuthHeaders()
    });
    return handleResponse<Product>(res);
};

export const createProduct = async (dto: ProductFormData): Promise<Product> => {
    const res = await fetch('/api/products', {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify(dto)
    });
    return handleResponse<Product>(res);
};

export const updateProduct = async (id: number, dto: ProductFormData): Promise<Product> => {
    const res = await fetch(`/api/products/${id}`, {
        method: 'PUT',
        headers: getAuthHeaders(),
        body: JSON.stringify(dto)
    });
    return handleResponse<Product>(res);
};

export const deleteProduct = async (id: number): Promise<void> => {
    const res = await fetch(`/api/products/${id}`, {
        method: 'DELETE',
        headers: getAuthHeaders()
    });
    await handleResponse<void>(res);
};

export const searchProducts = async (params: ProductSearchParams): Promise<Product[]> => {
    const query = new URLSearchParams();
    if (params.name) query.append('name', params.name);
    if (params.minPrice !== undefined && params.minPrice !== null && !isNaN(params.minPrice)) {
        query.append('minPrice', params.minPrice.toString());
    }
    if (params.maxPrice !== undefined && params.maxPrice !== null && !isNaN(params.maxPrice)) {
        query.append('maxPrice', params.maxPrice.toString());
    }

    const res = await fetch(`/api/products/search?${query.toString()}`, {
        headers: getAuthHeaders()
    });
    return handleResponse<Product[]>(res);
};
