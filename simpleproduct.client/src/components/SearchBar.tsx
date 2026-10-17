import { useState } from 'react';
import type { ProductSearchParams } from '../types/product';

interface SearchBarProps {
    onSearch: (params: ProductSearchParams) => void;
    onReset: () => void;
}

export const SearchBar = ({ onSearch, onReset }: SearchBarProps) => {
    const [name, setName] = useState('');
    const [minPrice, setMinPrice] = useState('');
    const [maxPrice, setMaxPrice] = useState('');

    const handleSearch = (e: React.FormEvent) => {
        e.preventDefault();
        onSearch({
            name: name.trim() || undefined,
            minPrice: minPrice ? parseFloat(minPrice) : undefined,
            maxPrice: maxPrice ? parseFloat(maxPrice) : undefined
        });
    };

    const handleClear = () => {
        setName('');
        setMinPrice('');
        setMaxPrice('');
        onReset();
    };

    return (
        <form onSubmit={handleSearch} style={{ display: 'flex', gap: '8px', flexWrap: 'wrap', alignItems: 'center', marginBottom: '16px' }}>
            <input
                type="text"
                placeholder="Search by name..."
                value={name}
                onChange={(e) => setName(e.target.value)}
                style={{ padding: '8px', flex: '1 1 180px' }}
            />
            <input
                type="number"
                placeholder="Min price"
                value={minPrice}
                onChange={(e) => setMinPrice(e.target.value)}
                style={{ padding: '8px', width: '100px' }}
                step="0.01"
            />
            <input
                type="number"
                placeholder="Max price"
                value={maxPrice}
                onChange={(e) => setMaxPrice(e.target.value)}
                style={{ padding: '8px', width: '100px' }}
                step="0.01"
            />
            <button type="submit" style={{ padding: '8px 16px', background: '#aa3bff', color: '#fff', border: 'none', borderRadius: '4px', cursor: 'pointer' }}>
                Search
            </button>
            <button type="button" onClick={handleClear} style={{ padding: '8px 16px', background: '#6b6375', color: '#fff', border: 'none', borderRadius: '4px', cursor: 'pointer' }}>
                Clear
            </button>
        </form>
    );
};
