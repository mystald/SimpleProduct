import { useState } from 'react';
import { login } from '../api/auth';

interface LoginFormProps {
    onLoginSuccess: () => void;
}

export const LoginForm = ({ onLoginSuccess }: LoginFormProps) => {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState<string | null>(null);
    const [submitting, setSubmitting] = useState(false);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError(null);
        setSubmitting(true);

        try {
            await login(email, password);
            onLoginSuccess();
        } catch (err: unknown) {
            setError(err instanceof Error ? err.message : 'Login failed');
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <div style={{ maxWidth: '400px', margin: '40px auto', padding: '24px', border: '1px solid var(--border)', borderRadius: '8px', textAlign: 'left' }}>
            <h2 style={{ marginTop: 0 }}>Login</h2>
            <p style={{ fontSize: '14px', marginBottom: '16px', color: 'var(--text)' }}>
                Please log in to manage products. (Default: <code>admin@simpleproduct.com</code> / <code>Admin@123</code>)
            </p>

            {error && (
                <div style={{ padding: '8px 12px', background: '#ffebee', color: '#c62828', borderRadius: '4px', marginBottom: '16px' }}>
                    {error}
                </div>
            )}

            <form onSubmit={handleSubmit}>
                <div style={{ marginBottom: '16px' }}>
                    <label style={{ display: 'block', marginBottom: '6px', fontWeight: 'bold' }}>Email</label>
                    <input
                        type="email"
                        required
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        style={{ width: '100%', padding: '8px', boxSizing: 'border-box' }}
                    />
                </div>

                <div style={{ marginBottom: '20px' }}>
                    <label style={{ display: 'block', marginBottom: '6px', fontWeight: 'bold' }}>Password</label>
                    <input
                        type="password"
                        required
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        style={{ width: '100%', padding: '8px', boxSizing: 'border-box' }}
                    />
                </div>

                <button
                    type="submit"
                    disabled={submitting}
                    style={{
                        width: '100%',
                        padding: '10px',
                        backgroundColor: '#aa3bff',
                        color: '#fff',
                        border: 'none',
                        borderRadius: '4px',
                        cursor: submitting ? 'not-allowed' : 'pointer'
                    }}
                >
                    {submitting ? 'Logging in...' : 'Sign In'}
                </button>
            </form>
        </div>
    );
};
