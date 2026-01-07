import { useNavigate } from 'react-router-dom';
import { Card, CardContent } from '@/components/ui/card';
import {
  FileText,
  LineChart,
  CloudDownload,
  Landmark,
  Building2,
  Users,
  TrendingUp,
  Calculator,
} from 'lucide-react';
import { useGetBoletas } from '../api/generated/boletas/boletas';
import { useGetAtivos } from '../api/generated/ativos/ativos';
import { useGetEmissores } from '../api/generated/emissores/emissores';
import { useGetFundos } from '../api/generated/fundos/fundos';
import { useGetContrapartes } from '../api/generated/contrapartes/contrapartes';

interface StatCardProps {
  title: string;
  value: number | string;
  icon: React.ReactNode;
  color: string;
}

function StatCard({ title, value, icon, color }: StatCardProps) {
  return (
    <Card>
      <CardContent className="p-6">
        <div className="flex items-center justify-between">
          <div>
            <p className="text-sm text-muted-foreground mb-1">{title}</p>
            <p className="text-3xl font-bold">{value}</p>
          </div>
          <div
            className="rounded-lg p-3 text-white flex items-center justify-center"
            style={{ backgroundColor: color }}
          >
            {icon}
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

export default function HomePage() {
  const navigate = useNavigate();
  const { data: boletas = [] } = useGetBoletas();
  const { data: ativos = [] } = useGetAtivos();
  const { data: emissores = [] } = useGetEmissores();
  const { data: fundos = [] } = useGetFundos();
  const { data: contrapartes = [] } = useGetContrapartes();

  return (
    <div>
      <h1 className="text-3xl font-bold mb-6">Dashboard ArxFlow</h1>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6 mb-6">
        {/* Estatisticas */}
        <StatCard
          title="Boletas"
          value={boletas.length}
          icon={<FileText className="h-6 w-6" />}
          color="#1976d2"
        />

        <StatCard
          title="Ativos"
          value={ativos.length}
          icon={<LineChart className="h-6 w-6" />}
          color="#2e7d32"
        />

        <StatCard
          title="Emissores"
          value={emissores.length}
          icon={<Building2 className="h-6 w-6" />}
          color="#ed6c02"
        />

        <StatCard
          title="Fundos"
          value={fundos.length}
          icon={<Landmark className="h-6 w-6" />}
          color="#9c27b0"
        />
      </div>

      {/* Links Rapidos */}
      <Card className="mb-6">
        <CardContent className="p-6">
          <h2 className="text-xl font-semibold mb-4">Acesso Rapido</h2>
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
            <Card
              className="cursor-pointer hover:shadow-lg transition-shadow"
              onClick={() => navigate('/boletas')}
            >
              <CardContent className="p-6 text-center">
                <FileText className="h-8 w-8 mx-auto text-primary mb-2" />
                <h3 className="text-lg font-semibold">Boletas</h3>
                <p className="text-sm text-muted-foreground">Gerenciar boletas</p>
              </CardContent>
            </Card>

            <Card
              className="cursor-pointer hover:shadow-lg transition-shadow"
              onClick={() => navigate('/calculadora')}
            >
              <CardContent className="p-6 text-center">
                <Calculator className="h-8 w-8 mx-auto text-green-600 mb-2" />
                <h3 className="text-lg font-semibold">Calculadora</h3>
                <p className="text-sm text-muted-foreground">Calculo de titulos</p>
              </CardContent>
            </Card>

            <Card
              className="cursor-pointer hover:shadow-lg transition-shadow"
              onClick={() => navigate('/yield-curve')}
            >
              <CardContent className="p-6 text-center">
                <TrendingUp className="h-8 w-8 mx-auto text-orange-500 mb-2" />
                <h3 className="text-lg font-semibold">Curva de Juros</h3>
                <p className="text-sm text-muted-foreground">Visualizar curvas</p>
              </CardContent>
            </Card>

            <Card
              className="cursor-pointer hover:shadow-lg transition-shadow"
              onClick={() => navigate('/downloads')}
            >
              <CardContent className="p-6 text-center">
                <CloudDownload className="h-8 w-8 mx-auto text-blue-500 mb-2" />
                <h3 className="text-lg font-semibold">Downloads</h3>
                <p className="text-sm text-muted-foreground">B3, ANBIMA, BCB</p>
              </CardContent>
            </Card>
          </div>
        </CardContent>
      </Card>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Informacoes de Cadastros */}
        <Card>
          <CardContent className="p-6">
            <h2 className="text-xl font-semibold mb-4">Cadastros</h2>
            <div className="space-y-4">
              <div className="flex justify-between items-center">
                <div className="flex items-center gap-2">
                  <Users className="h-5 w-5 text-muted-foreground" />
                  <span>Contrapartes</span>
                </div>
                <span className="text-xl font-semibold">{contrapartes.length}</span>
              </div>
              <div className="flex justify-between items-center">
                <div className="flex items-center gap-2">
                  <Building2 className="h-5 w-5 text-muted-foreground" />
                  <span>Emissores</span>
                </div>
                <span className="text-xl font-semibold">{emissores.length}</span>
              </div>
              <div className="flex justify-between items-center">
                <div className="flex items-center gap-2">
                  <Landmark className="h-5 w-5 text-muted-foreground" />
                  <span>Fundos</span>
                </div>
                <span className="text-xl font-semibold">{fundos.length}</span>
              </div>
              <div className="flex justify-between items-center">
                <div className="flex items-center gap-2">
                  <LineChart className="h-5 w-5 text-muted-foreground" />
                  <span>Ativos</span>
                </div>
                <span className="text-xl font-semibold">{ativos.length}</span>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Sobre o Sistema */}
        <Card>
          <CardContent className="p-6">
            <h2 className="text-xl font-semibold mb-4">Sobre o Sistema</h2>
            <p className="text-sm text-muted-foreground mb-4">
              ArxFlow e um sistema completo para gestao de operacoes de renda fixa, integrando dados
              de multiplas fontes (B3, ANBIMA, BCB) e oferecendo ferramentas avancadas de calculo e
              analise.
            </p>
            <p className="text-sm text-muted-foreground font-semibold mb-2">Funcionalidades:</p>
            <ul className="list-disc list-inside text-sm text-muted-foreground space-y-1">
              <li>Registro e gestao de boletas de operacoes</li>
              <li>Calculadora de titulos publicos (LTN, NTN-B, LFT, etc.)</li>
              <li>Visualizacao de curvas de juros (DI1, PRE, IPCA)</li>
              <li>Download automatico de dados de mercado</li>
              <li>Cadastro de ativos, emissores, fundos e contrapartes</li>
            </ul>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
