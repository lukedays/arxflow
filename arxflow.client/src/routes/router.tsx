import { createBrowserRouter } from 'react-router-dom';
import MainLayout from '../components/Layout/MainLayout';
import HomePage from '../pages/Home/HomePage';
import EmissoresPage from '../pages/Emissores/EmissoresPage';
import FundosPage from '../pages/Fundos/FundosPage';
import ContrapartesPage from '../pages/Contrapartes/ContrapartesPage';
import AtivosPage from '../pages/Ativos/AtivosPage';
import BoletasPage from '../pages/Boletas/BoletasPage';
import CalculadoraTitulosPage from '../pages/CalculadoraTitulos/CalculadoraTitulosPage';
import YieldCurvePage from '../pages/YieldCurve/YieldCurvePage';
import DownloadsPage from '../pages/Downloads/DownloadsPage';

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
