import { useCallback, useEffect, useState } from 'react';
import {
    createProduct,
    deleteProduct,
    getProducts,
    searchProducts,
    updateProduct
} from '../api/products';
import { isAuthenticated } from '../api/auth';
import type { Product, ProductFormData, ProductSearchParams } from '../types/product';

export const useProducts = () => {
    const [products, setProducts] = useState<Product[]>([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const loadProducts = useCallback(async () => {
        setLoading(true);
        setError(null);
        try {
            const data = await getProducts();
            setProducts(data);
        } catch (err: unknown) {
            setError(err instanceof Error ? err.message : 'Failed to fetch products');
        } finally {
            setLoading(false);
        }
    }, []);

    const filterProducts = useCallback(async (params: ProductSearchParams) => {
        setLoading(true);
        setError(null);
        try {
            const data = await searchProducts(params);
            setProducts(data);
        } catch (err: unknown) {
            setError(err instanceof Error ? err.message : 'Search failed');
        } finally {
            setLoading(false);
        }
    }, []);

    const handleCreate = async (dto: ProductFormData) => {
        await createProduct(dto);
        await loadProducts();
    };

    const handleUpdate = async (id: number, dto: ProductFormData) => {
        await updateProduct(id, dto);
        await loadProducts();
    };

    const handleDelete = async (id: number) => {
        await deleteProduct(id);
        await loadProducts();
    };

    useEffect(() => {
        if (isAuthenticated()) {
            loadProducts();
        }
    }, [loadProducts]);

    return {
        products,
        loading,
        error,
        loadProducts,
        filterProducts,
        handleCreate,
        handleUpdate,
        handleDelete
    };
};
