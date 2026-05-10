import { Outlet } from 'react-router-dom';
import { Sidebar } from '../components/Sidebar';

export const MainLayout = () => {
  return (
    <div className="flex min-h-screen flex-col bg-[#f5f5f7] lg:flex-row">
      {/* Sidebar handles its own sticky/fixed positioning */}
      <Sidebar />
      {/* On mobile: normal flow below sticky top bar.
          On desktop: offset right to leave room for the fixed 256px sidebar */}
      <main className="flex-1 lg:mr-64">
        <div className="min-h-screen px-4 py-6 sm:px-6 lg:px-8 lg:py-8 bg-[#fafafc]">
          <Outlet />
        </div>
      </main>
    </div>
  );
};