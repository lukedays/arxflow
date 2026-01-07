import MegaMenu from './MegaMenu';

export default function TopBar() {
  return (
    <header className="fixed top-0 left-0 right-0 z-50 bg-primary text-primary-foreground shadow-md">
      <div className="flex h-16 items-center px-4">
        {/* Logo e titulo */}
        <h1 className="text-xl font-bold mr-8 cursor-default">ArxFlow</h1>

        {/* Mega Menu */}
        <MegaMenu />

        {/* Espacador para empurrar conteudo para a direita */}
        <div className="flex-grow" />

        {/* Subtitulo do sistema */}
        <div className="flex items-center gap-2">
          <span className="text-sm opacity-80">Sistema de Gestao de Boletas</span>
        </div>
      </div>
    </header>
  );
}
