import MegaMenu from './MegaMenu';

export default function TopBar() {
  return (
    <header className="fixed top-0 left-0 right-0 z-50 bg-primary text-primary-foreground shadow-md">
      <div className="flex h-16 items-center px-4">
        {/* Logo e nome */}
        <div className="flex items-center gap-3 mr-8">
          <img
            src="/logo-arx-branco.png"
            alt="Arx Capital"
            className="h-8"
          />
          <span className="text-2xl tracking-wide" style={{ color: '#AB8433' }}>Flow</span>
        </div>

        {/* Mega Menu */}
        <MegaMenu />
      </div>
    </header>
  );
}
