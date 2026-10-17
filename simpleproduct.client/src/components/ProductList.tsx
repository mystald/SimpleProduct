import { useState } from 'react';
import type { Product } from '../types/product';

interface ProductListProps {
    products: Product[];
    loading: boolean;
    onEdit: (product: Product) => void;
    onDelete: (id: number) => Promise<void>;
}

export const ProductList = ({ products, loading, onEdit, onDelete }: ProductListProps) => {
    const [deletingId, setDeletingId] = useState<number | null>(null);

    const handleDelete = async (id: number) => {
        if (!window.confirm('Are you sure you want to delete this product?')) {
            return;
        }

        setDeletingId(id);
        try {
            await onDelete(id);
        } finally {
            setDeletingId(null);
        }
    };

    if (loading) {
        return <p><em>Loading products...</em></p>;
    }

    if (products.length === 0) {
        return <p style={{ padding: '24px', background: 'var(--code-bg, #f5f5f5)', borderRadius: '4px' }}>No products found.</p>;
    }

    return (
        <div style={{ overflowX: 'auto' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', marginTop: '16px' }}>
                <thead>
                    <tr style={{ borderBottom: '2px solid var(--border, #ccc)' }}>
                        <th style={{ padding: '10px' }}>ID</th>
                        <th style={{ padding: '10px' }}>Name</th>
                        <th style={{ padding: '10px' }}>Description</th>
                        <th style={{ padding: '10px' }}>Price</th>
                        <th style={{ padding: '10px' }}>Created At</th>
                        <th style={{ padding: '10px', textAlign: 'center' }}>Actions</th>
                    </tr>
                </thead>
                <tbody>
                    {products.map((p) => (
                        <tr key={p.id} style={{ borderBottom: '1px solid var(--border, #eee)' }}>
                            <td style={{ padding: '10px' }}>{p.id}</td>
                            <td style={{ padding: '10px', fontWeight: 'bold' }}>{p.name}</td>
                            <td style={{ padding: '10px' }}>{p.description || '-'}</td>
                            <td style={{ padding: '10px' }}>${p.price.toFixed(2)}</td>
                            <td style={{ padding: '10px', fontSize: '13px' }}>
                                {new Date(p.createdAt).toLocaleString()}
                            </td>
                            <td style={{ padding: '10px', textAlign: 'center' }}>
                                <button
                                    onClick={() => onEdit(p)}
                                    style={{
                                        marginRight: '8px',
                                        padding: '4px 10px',
                                        background: '#2196f3',
                                        color: '#fff',
                                        border: 'none',
                                        borderRadius: '3px',
                                        cursor: 'pointer'
                                    }}
                                >
                                    Edit
                                </button>
                                <button
                                    onClick={() => handleDelete(p.id)}
                                    disabled={deletingId === p.id}
                                    style={{
                                        padding: '4px 10px',
                                        background: '#f44336',
                                        color: '#fff',
                                        border: 'none',
                                        borderRadius: '3px',
                                        cursor: deletingId === p.id ? 'not-allowed' : 'pointer'
                                    }}
                                >
                                    {deletingId === p.id ? 'Deleting...' : 'Delete'}
                                </button>
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
};
