import { createBrowserRouter, Navigate } from 'react-router-dom';
import { MainLayout } from '../layouts/MainLayout';
import { Dashboard } from '../pages/Dashboard';
import { Sources } from '../pages/Sources';
import { Aims } from '../pages/Aims';
import { Transactions } from '../pages/Transactions';
import { PlannedTransactions } from '../pages/PlannedTransactions';
import { Analytics } from '../pages/Analytics';
import { Settings } from '../pages/Settings';
import { Planning } from '../pages/Planning';
import { Login } from '../pages/Login';
import { useAuthStore } from '../store/authStore';
import type { JSX } from 'react';

const PrivateRoute = ({ children }: { children: JSX.Element }) => {
  const token = useAuthStore((state) => state.accessToken);
  return token ? children : <Navigate to="/login" replace />;
};
const PublicRoute = ({ children }: { children: JSX.Element }) => {
  const token = useAuthStore((state) => state.accessToken);
  return !token ? children : <Navigate to="/" replace />;
};


export const router = createBrowserRouter([
  {
    path: '/login',
    element: (
      <PublicRoute>
        <Login />
      </PublicRoute>
    ),
  },
  {
    path: '/',
    element: (
      <PrivateRoute>
        <MainLayout />
      </PrivateRoute>
    ),
    children: [
      {
        index: true,
        element: <Dashboard />,
      },
      {
        path: 'sources',
        element: <Sources />,
      },
      {
        path: 'aims',
        element: <Aims />,
      },
      {
        path: 'transactions',
        element: <Transactions />,
      },
      {
        path: 'planned-transactions',
        element: <PlannedTransactions />,
      },
      {
        path: 'analytics',
        element: <Analytics />,
      },
      {
        path: 'settings',
        element: <Settings />,
      },
      {
        path: 'planning',
        element: <Planning />,
      },
    ],
  },
  {
    path: '*',
    element: <Navigate to="/" replace />,
  }
]);