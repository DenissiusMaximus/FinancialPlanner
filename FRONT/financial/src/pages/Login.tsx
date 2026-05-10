import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Button } from '../components/Button';
import { Card } from '../components/Card';
import { usePostApiUserLogin, usePostApiUserRegister } from '../api/generated/endpoints';
import { useAuthStore } from '../store/authStore';

type Mode = 'login' | 'register';

export const Login: React.FC = () => {
  const navigate = useNavigate();
  const setTokens = useAuthStore((state) => state.setTokens);
  const loginMutation = usePostApiUserLogin();
  const registerMutation = usePostApiUserRegister();

  const [mode, setMode] = useState<Mode>('login');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [name, setName] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState('');

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    try {
      const result = await loginMutation.mutateAsync({ data: { email, password } });
      if (result?.accessToken && result?.refreshToken) {
        setTokens(result.accessToken, result.refreshToken);
        navigate('/', { replace: true });
        return;
      }
      setError('Невірна відповідь сервера');
    } catch {
      setError('Невірний email або пароль');
    }
  };

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    if (password !== confirmPassword) {
      setError('Паролі не співпадають');
      return;
    }
    if (password.length < 6) {
      setError('Пароль має бути не менше 6 символів');
      return;
    }
    try {
      const result = await registerMutation.mutateAsync({
        data: { email, password, name: name || undefined },
      });
      if (result?.accessToken && result?.refreshToken) {
        setTokens(result.accessToken, result.refreshToken);
        navigate('/', { replace: true });
        return;
      }
      setError('Невірна відповідь сервера');
    } catch (err: any) {
      const msg = err?.response?.data?.message ?? err?.response?.data ?? null;
      setError(typeof msg === 'string' ? msg : 'Помилка реєстрації. Спробуйте ще раз.');
    }
  };

  const switchMode = (next: Mode) => {
    setMode(next);
    setError('');
    setEmail('');
    setPassword('');
    setName('');
    setConfirmPassword('');
  };

  const isPending = loginMutation.isPending || registerMutation.isPending;

  return (
    <div className="min-h-screen bg-[#f5f5f7] flex items-center justify-center px-4 py-8">
      <div className="w-full max-w-5xl grid grid-cols-1 lg:grid-cols-2 gap-8 items-stretch">
        {/* Left branding panel */}
        <div className="hidden lg:flex flex-col justify-between rounded-3xl bg-gradient-to-br from-surface-tile-1 to-ink text-white p-10 shadow-2xl">
          <div>
            <div className="h-12 w-12 rounded-2xl bg-white/15 flex items-center justify-center mb-6">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="white" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
                <line x1="12" y1="1" x2="12" y2="23" />
                <path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6" />
              </svg>
            </div>
            <h1 className="font-display text-4xl font-semibold leading-tight mb-4">
              Financial Planner
            </h1>
            <p style={{ color: 'rgba(255,255,255,0.8)', fontSize: '1.1rem', lineHeight: 1.75, maxWidth: 380, display: 'block', width: '100%', whiteSpace: 'normal' }}>
              Контролюй доходи, витрати, цілі та рахунки в одному чистому, зручному інтерфейсі.
            </p>
          </div>
          <div className="space-y-3">
            {['Джерела та баланси', 'Фінансові цілі', 'Транзакції та аналітика', 'Планові платежі'].map((f) => (
              <div key={f} className="flex items-center gap-3 text-sm text-white/80">
                <div className="h-1.5 w-1.5 rounded-full bg-white/60" />
                {f}
              </div>
            ))}
          </div>
          <div className="text-sm text-white/40">
            React · TanStack Query · Zustand · Tailwind
          </div>
        </div>

        {/* Right auth panel */}
        <Card className="p-0 overflow-hidden shadow-xl border-0">
          <div className="p-8 md:p-10 bg-white">
            <div className="mb-8">
              <p className="text-sm font-semibold text-primary uppercase tracking-[0.2em] mb-2">
                {mode === 'login' ? 'Welcome back' : 'Get started'}
              </p>
              <h2 className="font-display text-3xl font-semibold text-ink mb-3">
                {mode === 'login' ? 'Увійти в акаунт' : 'Створити акаунт'}
              </h2>
              <p style={{ color: '#7a7a7a', display: 'block', width: '100%', whiteSpace: 'normal' }}>
                {mode === 'login'
                  ? 'Використай свої дані для доступу до фінансової панелі.'
                  : 'Зареєструйся та почни керувати своїми фінансами.'}
              </p>
            </div>

            {/* Mode tabs */}
            <div className="flex rounded-xl bg-[#f5f5f7] p-1 mb-6">
              <button
                type="button"
                onClick={() => switchMode('login')}
                className={`flex-1 py-2 rounded-lg text-sm font-semibold transition-all ${
                  mode === 'login' ? 'bg-white text-ink shadow-sm' : 'text-[#7a7a7a] hover:text-ink'
                }`}
              >
                Вхід
              </button>
              <button
                type="button"
                onClick={() => switchMode('register')}
                className={`flex-1 py-2 rounded-lg text-sm font-semibold transition-all ${
                  mode === 'register' ? 'bg-white text-ink shadow-sm' : 'text-[#7a7a7a] hover:text-ink'
                }`}
              >
                Реєстрація
              </button>
            </div>

            <form onSubmit={mode === 'login' ? handleLogin : handleRegister} className="space-y-4">
              {mode === 'register' && (
                <div>
                  <label className="block text-sm font-semibold text-ink mb-2">
                    Ім'я <span className="font-normal text-[#7a7a7a]">(опціонально)</span>
                  </label>
                  <input
                    type="text"
                    value={name}
                    onChange={(e) => setName(e.target.value)}
                    placeholder="Іван Петренко"
                    className="w-full px-4 py-3 rounded-xl border border-hairline bg-white text-ink focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary transition-colors"
                    autoComplete="name"
                  />
                </div>
              )}

              <div>
                <label className="block text-sm font-semibold text-ink mb-2">Email</label>
                <input
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder="you@example.com"
                  className="w-full px-4 py-3 rounded-xl border border-hairline bg-white text-ink focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary transition-colors"
                  autoComplete="email"
                  required
                />
              </div>

              <div>
                <label className="block text-sm font-semibold text-ink mb-2">Пароль</label>
                <input
                  type="password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="••••••••"
                  className="w-full px-4 py-3 rounded-xl border border-hairline bg-white text-ink focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary transition-colors"
                  autoComplete={mode === 'login' ? 'current-password' : 'new-password'}
                  required
                />
              </div>

              {mode === 'register' && (
                <div>
                  <label className="block text-sm font-semibold text-ink mb-2">Підтвердити пароль</label>
                  <input
                    type="password"
                    value={confirmPassword}
                    onChange={(e) => setConfirmPassword(e.target.value)}
                    placeholder="••••••••"
                    className="w-full px-4 py-3 rounded-xl border border-hairline bg-white text-ink focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary transition-colors"
                    autoComplete="new-password"
                    required
                  />
                </div>
              )}

              {error && (
                <div className="rounded-xl bg-red-50 border border-red-200 px-4 py-3 text-sm text-red-600">
                  {error}
                </div>
              )}

              <div className="pt-2">
                <Button type="submit" isLoading={isPending} className="w-full">
                  {mode === 'login' ? 'Увійти' : 'Зареєструватися'}
                </Button>
              </div>

              <div style={{ textAlign: 'center', fontSize: '0.875rem', color: '#7a7a7a' }}>
                {mode === 'login' ? (
                  <>
                    Немає акаунту?{' '}
                    <button
                      type="button"
                      onClick={() => switchMode('register')}
                      className="text-primary font-semibold hover:underline"
                    >
                      Зареєструватися
                    </button>
                  </>
                ) : (
                  <>
                    Вже є акаунт?{' '}
                    <button
                      type="button"
                      onClick={() => switchMode('login')}
                      className="text-primary font-semibold hover:underline"
                    >
                      Увійти
                    </button>
                  </>
                )}
              </div>
            </form>
          </div>
        </Card>
      </div>
    </div>
  );
};