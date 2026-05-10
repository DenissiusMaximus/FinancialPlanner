import React from 'react';
import { Outlet } from 'react-router-dom';
import { Sidebar } from './Sidebar';

export const MainLayout: React.FC = () => {
  return (
    <div className="flex w-full h-screen bg-[#f5f5f7]">
      <main className="flex-1 overflow-y-auto">
        <div className="max-w-7xl mx-auto px-8 py-8 mr-72">
          <Outlet />
        </div>
      </main>
      <Sidebar />
    </div>
  );
};
