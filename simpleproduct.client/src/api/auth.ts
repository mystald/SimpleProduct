const TOKEN_KEY = 'auth_token';

export interface LoginResponse {
    token?: string;
    accessToken?: string;
}

export const getToken = (): string | null => {
    return sessionStorage.getItem(TOKEN_KEY);
};

export const setToken = (token: string): void => {
    sessionStorage.setItem(TOKEN_KEY, token);
};

export const removeToken = (): void => {
    sessionStorage.removeItem(TOKEN_KEY);
};

export const isAuthenticated = (): boolean => {
    return !!getToken();
};

export const login = async (email: string, password: string): Promise<string> => {
    const response = await fetch('/api/auth/login', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ email, password })
    });

    if (!response.ok) {
        let errorMsg = 'Login failed';
        try {
            const err = await response.json();
            errorMsg = err.detail || err.message || err.title || errorMsg;
        } catch {
            // fallback
        }
        throw new Error(errorMsg);
    }

    const data: LoginResponse = await response.json();
    const token = data.token || data.accessToken;
    if (!token) {
        throw new Error('No authentication token returned by the server.');
    }

    setToken(token);
    return token;
};

export const logout = (): void => {
    removeToken();
};
