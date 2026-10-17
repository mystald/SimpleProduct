import { useState } from 'react';
import './App.css';
import { isAuthenticated, logout } from './api/auth';
import { LoginForm } from './components/LoginForm';
import { ProductForm } from './components/ProductForm';
import { ProductList } from './components/ProductList';
import { SearchBar } from './components/SearchBar';
import { useProducts } from './hooks/useProducts';
import type { Product, ProductFormData } from './types/product';

function App() {
    const [authenticated, setAuthenticated] = useState<boolean>(isAuthenticated());
    const [isFormOpen, setIsFormOpen] = useState(false);
    const [editingProduct, setEditingProduct] = useState<Product | null>(null);

    const {
        products,
        loading,
        error,
        loadProducts,
        filterProducts,
        handleCreate,
        handleUpdate,
        handleDelete
    } = useProducts();

    const handleLogout = () => {
        logout();
        setAuthenticated(false);
    };

    const handleOpenAdd = () => {
        setEditingProduct(null);
        setIsFormOpen(true);
    };

    const handleOpenEdit = (product: Product) => {
        setEditingProduct(product);
        setIsFormOpen(true);
    };

    const handleFormSubmit = async (data: ProductFormData) => {
        if (editingProduct) {
            await handleUpdate(editingProduct.id, data);
        } else {
            await handleCreate(data);
        }
        setIsFormOpen(false);
    };

    const handleLoginSuccess = () => {
        setAuthenticated(true);
        loadProducts();
    };

    if (!authenticated) {
        return (
            <div className="app-container">
                <header className="app-header">
                    <h1>Products</h1>
                </header>
                <LoginForm onLoginSuccess={handleLoginSuccess} />
            </div>
        );
    }

    return (
        <div className="app-container">
            <header className="app-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <div>
                    <h1 style={{ margin: '0', fontSize: '32px' }}>Products</h1>
                </div>
                <div>
                    <button
                        onClick={handleLogout}
                        style={{ padding: '8px 16px', background: '#e0e0e0', color: '#333', border: 'none', borderRadius: '4px', cursor: 'pointer' }}
                    >
                        Sign Out
                    </button>
                </div>
            </header>

            <main style={{ marginTop: '24px', textAlign: 'left' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px', flexWrap: 'wrap', gap: '8px' }}>
                    <SearchBar onSearch={filterProducts} onReset={loadProducts} />
                    <button
                        onClick={handleOpenAdd}
                        style={{ padding: '8px 16px', background: '#4caf50', color: '#fff', border: 'none', borderRadius: '4px', cursor: 'pointer', fontWeight: 'bold' }}
                    >
                        + Add Product
                    </button>
                </div>

                {error && (
                    <div style={{ padding: '12px', background: '#ffebee', color: '#c62828', borderRadius: '4px', marginBottom: '16px' }}>
                        {error}
                    </div>
                )}

                <ProductList
                    products={products}
                    loading={loading}
                    onEdit={handleOpenEdit}
                    onDelete={handleDelete}
                />

                {isFormOpen && (
                    <ProductForm
                        productToEdit={editingProduct}
                        onSubmit={handleFormSubmit}
                        onCancel={() => setIsFormOpen(false)}
                    />
                )}
            </main>
        </div>
    );
}

export default App;