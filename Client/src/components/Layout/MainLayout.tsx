import { Outlet } from 'react-router-dom';
import TopBar from './TopBar';

export default function MainLayout() {
  return (
    <div className="flex min-h-screen">
      <TopBar />
      <main className="flex-grow bg-background p-6 pt-20">
        <div className="container mx-auto">
          <Outlet />
        </div>
      </main>
    </div>
  );
}
