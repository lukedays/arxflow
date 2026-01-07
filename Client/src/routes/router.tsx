import { createBrowserRouter } from 'react-router-dom';
import MainLayout from '../components/Layout/MainLayout';
import HomePage from '../pages/HomePage';
import EmissoresPage from '../pages/EmissoresPage';
import FundosPage from '../pages/FundosPage';
import ContrapartesPage from '../pages/ContrapartesPage';
import AtivosPage from '../pages/AtivosPage';
import BoletasPage from '../pages/BoletasPage';
import CalculadoraTitulosPage from '../pages/CalculadoraTitulosPage';
import YieldCurvePage from '../pages/YieldCurvePage';
import DownloadsPage from '../pages/DownloadsPage';
import ValidacaoCalculadorasPage from '../pages/ValidacaoCalculadorasPage';

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
        path: 'validacao',
        element: <ValidacaoCalculadorasPage />,
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
