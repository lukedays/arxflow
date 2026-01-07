import { createBrowserRouter } from 'react-router-dom';
import MainLayout from '../components/Layout/MainLayout';
import EmissoresPage from '../pages/Emissores/EmissoresPage';
import FundosPage from '../pages/Fundos/FundosPage';
import ContrapartesPage from '../pages/Contrapartes/ContrapartesPage';
import AtivosPage from '../pages/Ativos/AtivosPage';

// Placeholder pages - serão criadas nas próximas fases
const HomePage = () => <div>Home - Em desenvolvimento</div>;
const BoletasPage = () => <div>Boletas - Em desenvolvimento</div>;
const CalculadoraTitulosPage = () => <div>Calculadora de Títulos - Em desenvolvimento</div>;
const YieldCurvePage = () => <div>Curva de Juros - Em desenvolvimento</div>;
const DownloadsPage = () => <div>Downloads - Em desenvolvimento</div>;

export const router = createBrowserRouter([
  {
    path: '/',
    element: <MainLayout />,
    children: [
      {
        index: true,
        element: <HomePage />,
      },
      {
        path: 'boletas',
        element: <BoletasPage />,
      },
      {
        path: 'calculadora',
        element: <CalculadoraTitulosPage />,
      },
      {
        path: 'yield-curve',
        element: <YieldCurvePage />,
      },
      {
        path: 'downloads',
        element: <DownloadsPage />,
      },
      {
        path: 'ativos',
        element: <AtivosPage />,
      },
      {
        path: 'emissores',
        element: <EmissoresPage />,
      },
      {
        path: 'fundos',
        element: <FundosPage />,
      },
      {
        path: 'contrapartes',
        element: <ContrapartesPage />,
      },
    ],
  },
]);
