import React, { useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useGetApiUserMe, usePostApiUserLogout, useGetApiCurrency } from '../api/generated/endpoints';
import { useAuthStore } from '../store/authStore';
import { useCurrencyStore } from '../store/currencyStore';

const IconDashboard = () => (
  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <rect x="3" y="3" width="7" height="7" /><rect x="14" y="3" width="7" height="7" /><rect x="14" y="14" width="7" height="7" /><rect x="3" y="14" width="7" height="7" />
  </svg>
);
const IconSources = () => (
  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <rect x="2" y="5" width="20" height="14" rx="2" /><line x1="2" y1="10" x2="22" y2="10" />
  </svg>
);
const IconAims = () => (
  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <circle cx="12" cy="12" r="10" /><circle cx="12" cy="12" r="6" /><circle cx="12" cy="12" r="2" />
  </svg>
);
const IconTransactions = () => (
  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <polyline points="23 6 13.5 15.5 8.5 10.5 1 18" /><polyline points="17 6 23 6 23 12" />
  </svg>
);
const IconPlanned = () => (
  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <rect x="3" y="4" width="18" height="18" rx="2" /><line x1="16" y1="2" x2="16" y2="6" /><line x1="8" y1="2" x2="8" y2="6" /><line x1="3" y1="10" x2="21" y2="10" />
  </svg>
);
const IconAnalytics = () => (
  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <line x1="18" y1="20" x2="18" y2="10" /><line x1="12" y1="20" x2="12" y2="4" /><line x1="6" y1="20" x2="6" y2="14" />
  </svg>
);
const IconCategories = () => (
  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M22 19a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h5l2 3h9a2 2 0 0 1 2 2z" />
  </svg>
);
const IconPlanning = () => (
  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <circle cx="12" cy="12" r="10"/><path d="m16 10-4 4-4-4"/><path d="M12 14v7"/><path d="M12 3v3"/>
  </svg>
);
const IconUser = () => (
  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" /><circle cx="12" cy="7" r="4" />
  </svg>
);
const IconLogout = () => (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4" /><polyline points="16 17 21 12 16 7" /><line x1="21" y1="12" x2="9" y2="12" />
  </svg>
);
const IconMenu = () => (
  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <line x1="3" y1="6" x2="21" y2="6" /><line x1="3" y1="12" x2="21" y2="12" /><line x1="3" y1="18" x2="21" y2="18" />
  </svg>
);
const IconClose = () => (
  <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <line x1="18" y1="6" x2="6" y2="18" /><line x1="6" y1="6" x2="18" y2="18" />
  </svg>
);

export const Sidebar: React.FC = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const clearAuth = useAuthStore((state) => state.clearAuth);
  const userQuery = useGetApiUserMe();
  const logoutMutation = usePostApiUserLogout();
  const [mobileOpen, setMobileOpen] = useState(false);

  const menuItems = [
    { label: 'Dashboard', path: '/', icon: <IconDashboard /> },
    { label: 'Джерела', path: '/sources', icon: <IconSources /> },
    { label: 'Цілі', path: '/aims', icon: <IconAims /> },
    { label: 'Транзакції', path: '/transactions', icon: <IconTransactions /> },
    { label: 'Категорії', path: '/categories', icon: <IconCategories /> },
    { label: 'Планові транзакції', path: '/planned-transactions', icon: <IconPlanned /> },
    { label: 'Планування', path: '/planning', icon: <IconPlanning /> },
    { label: 'Аналітика', path: '/analytics', icon: <IconAnalytics /> },
  ];

  const user = userQuery.data as any;
  const userName = user?.name ?? user?.email ?? null;
  const userEmail = user?.email ?? null;
  const isProfileLoading = userQuery.isLoading;

  const currenciesQuery = useGetApiCurrency();
  const currencies = (Array.isArray(currenciesQuery.data) ? currenciesQuery.data : []) as any[];
  const selectedCurrency = useCurrencyStore((s) => s.selectedCurrency);
  const setSelectedCurrency = useCurrencyStore((s) => s.setSelectedCurrency);

  const handleLogout = async () => {
    try {
      await logoutMutation.mutateAsync({});
    } finally {
      clearAuth();
      window.location.href = '/login';
    }
  };

  const handleNavClick = () => setMobileOpen(false);

  const NavContent = () => (
    <>
      {/* Logo */}
      <div className="px-5 py-5 border-b border-[#f0f0f0] flex items-center gap-3">
        <div className="h-8 w-8 rounded-lg bg-primary flex items-center justify-center shrink-0">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="white" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
            <line x1="12" y1="1" x2="12" y2="23" /><path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6" />
          </svg>
        </div>
        <div className="font-display text-base font-semibold text-ink">FinPlanner</div>
        {/* Mobile close button */}
        <button
          className="ml-auto lg:hidden text-[#7a7a7a] hover:text-ink transition-colors p-1"
          onClick={() => setMobileOpen(false)}
        >
          <IconClose />
        </button>
      </div>

      {/* Nav */}
      <nav className="flex-1 py-3 flex flex-col overflow-y-auto">
        {menuItems.map((item) => (
          <Link
            key={item.path}
            to={item.path}
            onClick={handleNavClick}
          className={`flex items-center gap-3 px-4 py-2.5 mx-2 rounded-xl transition-all text-sm ${
              location.pathname === item.path
                ? 'text-primary bg-primary/8 font-semibold shadow-sm'
                : 'text-[#6b6b70] hover:text-ink hover:bg-[#f5f5f7]'
            }`}
          >
            <span className={`shrink-0 transition-colors ${location.pathname === item.path ? 'text-primary' : 'text-[#9a9a9a]'}`}>{item.icon}</span>
            <span className="flex-1">{item.label}</span>
          </Link>
        ))}
      </nav>

      {/* Bottom panel */}
      <div className="p-4 border-t border-[#f0f0f0] space-y-3">
        {/* Currency selector */}
        <div className="flex items-center gap-2">
          <span className="text-xs font-semibold text-[#7a7a7a] uppercase tracking-wider flex-1">Валюта</span>
          <select
            value={selectedCurrency}
            onChange={(e) => setSelectedCurrency(e.target.value)}
            className="text-xs font-semibold border border-hairline rounded-lg px-2 py-1.5 bg-white text-ink focus:outline-none focus:border-primary cursor-pointer"
          >
            {currencies.length > 0
              ? currencies.map((c: any) => (
                  <option key={c.id} value={c.name}>{c.name}</option>
                ))
              : (['UAH', 'USD', 'EUR'].map((c) => (
                  <option key={c} value={c}>{c}</option>
                )))
            }
          </select>
        </div>

        {/* User profile */}
        <div className="rounded-xl border border-hairline bg-[#f5f5f7] p-4">
          {isProfileLoading ? (
            <div className="space-y-3">
              <div className="h-10 w-36 rounded-lg bg-[#e8e8ed] animate-pulse" />
              <div className="h-8 w-full rounded-lg bg-[#e8e8ed] animate-pulse" />
            </div>
          ) : (
            <>
              <div className="flex items-center gap-3 mb-3">
                <div className="h-9 w-9 rounded-full bg-primary/10 text-primary flex items-center justify-center shrink-0">
                  <IconUser />
                </div>
                <div className="min-w-0">
                  <div className="font-semibold text-ink text-sm truncate">
                    {userName || 'Профіль'}
                  </div>
                  {userEmail && userName !== userEmail && (
                    <div className="text-xs text-[#7a7a7a] truncate">{userEmail}</div>
                  )}
                </div>
              </div>
            </>
          )}
          <button
            className="w-full flex items-center justify-center gap-2 px-3 py-2 bg-white border border-hairline rounded-lg text-sm font-medium text-ink hover:bg-[#fafafc] transition-colors"
            onClick={handleLogout}
            disabled={logoutMutation.isPending}
          >
            <IconLogout />
            {logoutMutation.isPending ? 'Вихід...' : 'Вийти'}
          </button>
        </div>
      </div>
    </>
  );

  return (
    <>
      {/* ── Mobile top bar ── */}
      <div className="lg:hidden sticky top-0 z-50 flex items-center gap-2 bg-white border-b border-[#f0f0f0] px-4 py-3 shadow-sm">
        <button
          className="flex items-center gap-2 flex-1 min-w-0 touch-manipulation"
          onClick={() => navigate('/')}
          aria-label="Перейти на головну"
        >
          <div className="h-7 w-7 rounded-lg bg-primary flex items-center justify-center shrink-0">
            <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="white" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
              <line x1="12" y1="1" x2="12" y2="23" /><path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6" />
            </svg>
          </div>
          <span className="font-display font-semibold text-ink text-sm truncate">FinPlanner</span>
        </button>
        {/* Currency selector */}
        <select
          value={selectedCurrency ?? ''}
          onChange={(e) => setSelectedCurrency(e.target.value)}
          className="text-xs font-semibold text-primary bg-primary/8 border border-primary/20 rounded-lg px-2 py-1.5 focus:outline-none focus:border-primary touch-manipulation shrink-0"
          aria-label="Валюта"
          style={{ fontSize: 14 }}
        >
          {currencies.map((c: any) => (
            <option key={c.id} value={c.name}>{c.name}</option>
          ))}
        </select>
        <button
          className="p-1.5 rounded-lg text-[#7a7a7a] hover:text-ink hover:bg-[#f5f5f7] transition-colors touch-manipulation"
          onClick={() => setMobileOpen(true)}
          aria-label="Відкрити меню"
        >
          <IconMenu />
        </button>
      </div>

      {/* ── Mobile overlay ── */}
      {mobileOpen && (
        <div
          className="lg:hidden fixed inset-0 bg-black/40 z-40 backdrop-blur-sm"
          onClick={() => setMobileOpen(false)}
        />
      )}

      {/* ── Mobile drawer (slides from left) ── */}
      <aside
        className={`lg:hidden fixed top-0 left-0 h-full w-72 bg-white z-50 flex flex-col shadow-2xl transition-transform duration-300 ${
          mobileOpen ? 'translate-x-0' : '-translate-x-full'
        }`}
      >
        <NavContent />
      </aside>

      {/* ── Desktop sidebar (fixed right) ── */}
      <aside className="hidden lg:flex fixed right-0 top-0 h-screen w-64 bg-white border-l border-[#f0f0f0] flex-col z-50 overflow-y-auto">
        <NavContent />
      </aside>
    </>
  );
};
