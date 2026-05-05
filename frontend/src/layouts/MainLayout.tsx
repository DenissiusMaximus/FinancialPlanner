import React from 'react';
import { Outlet, Navigate, NavLink, useNavigate } from 'react-router-dom';
import { useSelector, useDispatch } from 'react-redux';
import { LayoutDashboard, Receipt, Wallet, Target, LogOut, Folder, CalendarClock } from 'lucide-react';
import { logout } from '../store/authSlice';
import { useGetUserMeQuery } from '../store/apiSlice';
import { cn } from '../utils/cn';
import { Toaster } from 'react-hot-toast';
import { type AppDispatch, type RootState } from '../store';

const navigation = [
  { name: 'Dashboard', href: '/', icon: LayoutDashboard },
  { name: 'Transactions', href: '/transactions', icon: Receipt },
  { name: 'Subscriptions', href: '/planned', icon: CalendarClock },
  { name: 'Sources', href: '/sources', icon: Wallet },
  { name: 'Aims', href: '/aims', icon: Target },
  { name: 'Categories', href: '/categories', icon: Folder },
];

export function MainLayout() {
  const isAuthenticated = useSelector((state: RootState) => state.auth.isAuthenticated);
  const dispatch = useDispatch<AppDispatch>();
  const navigate = useNavigate();
  const { data: user, isLoading } = useGetUserMeQuery(undefined, {
    skip: !isAuthenticated,
  });

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  const handleLogout = () => {
    dispatch(logout());
    navigate('/login');
  };

  return (
    <div className="flex h-screen bg-slate-50">
      <Toaster position="top-right" />
      {/* Sidebar */}
      <div className="hidden w-64 bg-slate-900 flex-col md:flex">
        <div className="flex h-16 shrink-0 items-center px-6">
          <span className="text-xl font-bold text-white tracking-tight">FinPlanner</span>
        </div>
        <div className="flex flex-1 flex-col overflow-y-auto">
          <nav className="flex-1 space-y-1 px-4 py-4">
            {navigation.map((item) => {
              const Icon = item.icon;
              return (
                <NavLink
                  key={item.name}
                  to={item.href}
                  className={({ isActive }) =>
                    cn(
                      isActive
                        ? 'bg-slate-800 text-white'
                        : 'text-slate-300 hover:bg-slate-800 hover:text-white',
                      'group flex items-center px-3 py-2 text-sm font-medium rounded-md transition-colors'
                    )
                  }
                >
                  <Icon className="mr-3 h-5 w-5 flex-shrink-0" aria-hidden="true" />
                  {item.name}
                </NavLink>
              );
            })}
          </nav>
        </div>
        <div className="flex shrink-0 bg-slate-800 p-4">
          <div className="flex items-center w-full">
            <div className="flex-1 flex flex-col min-w-0">
              <span className="text-sm font-medium text-white truncate">
                {isLoading ? 'Loading...' : user?.accessToken ? 'Authenticated' : 'User'}
              </span>
            </div>
            <button
              onClick={handleLogout}
              className="ml-auto bg-transparent p-1 text-slate-400 hover:text-white focus:outline-none transition-colors"
              title="Logout"
            >
              <LogOut className="h-5 w-5" />
            </button>
          </div>
        </div>
      </div>

      {/* Main content area */}
      <div className="flex flex-1 flex-col overflow-hidden">
        {/* Mobile header could go here */}
        <main className="flex-1 overflow-y-auto bg-slate-50 p-8">
          <div className="mx-auto max-w-7xl">
            <Outlet />
          </div>
        </main>
      </div>
    </div>
  );
}
