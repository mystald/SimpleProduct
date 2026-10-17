import { useEffect, useState } from 'react';
import type { Product, ProductFormData } from '../types/product';

interface ProductFormProps {
    productToEdit?: Product | null;
    onSubmit: (data: ProductFormData) => Promise<void>;
    onCancel: () => void;
}

export const ProductForm = ({ productToEdit, onSubmit, onCancel }: ProductFormProps) => {
    const [name, setName] = useState('');
    const [description, setDescription] = useState('');
    const [price, setPrice] = useState('');
    const [error, setError] = useState<string | null>(null);
    const [saving, setSaving] = useState(false);

    useEffect(() => {
        if (productToEdit) {
            setName(productToEdit.name);
            setDescription(productToEdit.description || '');
            setPrice(productToEdit.price.toString());
        } else {
            setName('');
            setDescription('');
            setPrice('');
        }
        setError(null);
    }, [productToEdit]);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError(null);

        const numericPrice = parseFloat(price);
        if (isNaN(numericPrice) || numericPrice <= 0) {
            setError('Price must be greater than 0.');
            return;
        }

        setSaving(true);
        try {
            await onSubmit({
                name: name.trim(),
                description: description.trim(),
                price: numericPrice
            });
        } catch (err: unknown) {
            setError(err instanceof Error ? err.message : 'Operation failed');
        } finally {
            setSaving(false);
        }
    };

    return (
        <div style={{
            position: 'fixed',
            top: 0,
            left: 0,
            right: 0,
            bottom: 0,
            backgroundColor: 'rgba(0,0,0,0.5)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            zIndex: 1000
        }}>
            <div style={{
                background: 'var(--bg, #fff)',
                color: 'var(--text, #333)',
                padding: '24px',
                borderRadius: '8px',
                width: '100%',
                maxWidth: '450px',
                textAlign: 'left',
                boxShadow: '0 4px 12px rgba(0,0,0,0.2)'
            }}>
                <h3 style={{ marginTop: 0 }}>{productToEdit ? 'Edit Product' : 'Add New Product'}</h3>

                {error && (
                    <div style={{ padding: '8px', background: '#ffebee', color: '#c62828', borderRadius: '4px', marginBottom: '12px' }}>
                        {error}
                    </div>
                )}

                <form onSubmit={handleSubmit}>
                    <div style={{ marginBottom: '12px' }}>
                        <label style={{ display: 'block', marginBottom: '4px', fontWeight: 'bold' }}>Name *</label>
                        <input
                            type="text"
                            required
                            maxLength={200}
                            value={name}
                            onChange={(e) => setName(e.target.value)}
                            style={{ width: '100%', padding: '8px', boxSizing: 'border-box' }}
                        />
                    </div>

                    <div style={{ marginBottom: '12px' }}>
                        <label style={{ display: 'block', marginBottom: '4px', fontWeight: 'bold' }}>Description</label>
                        <textarea
                            rows={3}
                            maxLength={1000}
                            value={description}
                            onChange={(e) => setDescription(e.target.value)}
                            style={{ width: '100%', padding: '8px', boxSizing: 'border-box' }}
                        />
                    </div>

                    <div style={{ marginBottom: '16px' }}>
                        <label style={{ display: 'block', marginBottom: '4px', fontWeight: 'bold' }}>Price ($) *</label>
                        <input
                            type="number"
                            required
                            step="0.01"
                            min="0.01"
                            value={price}
                            onChange={(e) => setPrice(e.target.value)}
                            style={{ width: '100%', padding: '8px', boxSizing: 'border-box' }}
                        />
                    </div>

                    <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '8px' }}>
                        <button
                            type="button"
                            onClick={onCancel}
                            disabled={saving}
                            style={{ padding: '8px 16px', background: '#e0e0e0', color: '#333', border: 'none', borderRadius: '4px', cursor: 'pointer' }}
                        >
                            Cancel
                        </button>
                        <button
                            type="submit"
                            disabled={saving}
                            style={{ padding: '8px 16px', background: '#aa3bff', color: '#fff', border: 'none', borderRadius: '4px', cursor: saving ? 'not-allowed' : 'pointer' }}
                        >
                            {saving ? 'Saving...' : (productToEdit ? 'Save Changes' : 'Create')}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
};
