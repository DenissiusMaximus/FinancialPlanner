import React from 'react';
import { createBrowserRouter } from 'react-router-dom';
import { MainLayout } from './layouts/MainLayout';
import { Login } from './features/auth/Login';
import { Register } from './features/auth/Register';
import { Dashboard } from './features/dashboard/Dashboard';
import { Transactions } from './features/transactions/Transactions';
import { Sources } from './features/sources/Sources';
import { Aims } from './features/aims/Aims';
import { Categories } from './features/categories/Categories';
import { PlannedTransactions } from './features/planned-transactions/PlannedTransactions';

export const router = createBrowserRouter([
  {
    path: '/login',
    element: <Login />,
  },
  {
    path: '/register',
    element: <Register />,
  },
  {
    path: '/',
    element: <MainLayout />,
    children: [
      {
        index: true,
        element: <Dashboard />,
      },
      {
        path: 'transactions',
        element: <Transactions />,
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
        path: 'categories',
        element: <Categories />,
      },
      {
        path: 'planned',
        element: <PlannedTransactions />,
      },
    ],
  },
]);
