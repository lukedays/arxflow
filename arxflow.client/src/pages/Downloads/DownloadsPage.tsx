import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from '@/components/ui/card';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Progress } from '@/components/ui/progress';
import { DatePicker } from '@/components/ui/date-picker';
import { Download, AlertCircle, CheckCircle2 } from 'lucide-react';

export default function DownloadsPage() {
  const [dateRange, setDateRange] = useState({
    startDate: new Date(Date.now() - 5 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
    endDate: new Date().toISOString().split('T')[0],
  });

  const [downloadState, setDownloadState] = useState<{
    [key: string]: { loading: boolean; result: { success?: boolean; message?: string; recordsProcessed?: number } | null };
  }>({});

  const handleDownload = async (
    endpoint: string,
    key: string,
    requiresDateRange: boolean = false
  ) => {
    setDownloadState((prev) => ({ ...prev, [key]: { loading: true, result: null } }));

    try {
      const url = `/api/downloads/${endpoint}`;
      const options: RequestInit = {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
      };

      if (requiresDateRange) {
        options.body = JSON.stringify({
          startDate: new Date(dateRange.startDate).toISOString(),
          endDate: new Date(dateRange.endDate).toISOString(),
        });
      }

      const response = await fetch(url, options);
      const data = await response.json();

      setDownloadState((prev) => ({ ...prev, [key]: { loading: false, result: data } }));
    } catch (error) {
      console.error('Erro no download:', error);
      setDownloadState((prev) => ({
        ...prev,
        [key]: {
          loading: false,
          result: { success: false, message: 'Erro ao processar download' },
        },
      }));
    }
  };

  const renderDownloadCard = (
    title: string,
    description: string,
    endpoint: string,
    key: string,
    requiresDateRange: boolean = false
  ) => {
    const state = downloadState[key] || { loading: false, result: null };

    return (
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">{title}</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground mb-4">{description}</p>

          {state.loading && <Progress value={50} className="mb-4" />}

          {state.result && (
            <Alert
              variant={state.result.success ? 'default' : 'destructive'}
              className="mb-4"
            >
              {state.result.success ? (
                <CheckCircle2 className="h-4 w-4" />
              ) : (
                <AlertCircle className="h-4 w-4" />
              )}
              <AlertDescription>
                {state.result.message}
                {state.result.recordsProcessed !== undefined &&
                  state.result.recordsProcessed > 0 && (
                    <span className="block text-xs mt-1">
                      Registros processados: {state.result.recordsProcessed}
                    </span>
                  )}
              </AlertDescription>
            </Alert>
          )}
        </CardContent>
        <CardFooter>
          <Button
            onClick={() => handleDownload(endpoint, key, requiresDateRange)}
            disabled={state.loading}
            className="w-full"
          >
            <Download className="h-4 w-4 mr-2" />
            {state.loading ? 'Processando...' : 'Iniciar Download'}
          </Button>
        </CardFooter>
      </Card>
    );
  };

  return (
    <div>
      <h1 className="text-3xl font-bold mb-6">Downloads de Dados</h1>

      <Card className="mb-6">
        <CardContent className="p-6">
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 items-end">
            <div className="space-y-2">
              <Label>Data Inicial</Label>
              <DatePicker
                value={dateRange.startDate}
                onChange={v => setDateRange({ ...dateRange, startDate: v })}
                placeholder="Data inicial"
              />
            </div>
            <div className="space-y-2">
              <Label>Data Final</Label>
              <DatePicker
                value={dateRange.endDate}
                onChange={v => setDateRange({ ...dateRange, endDate: v })}
                placeholder="Data final"
              />
            </div>
            <Alert>
              <AlertCircle className="h-4 w-4" />
              <AlertDescription>Intervalo selecionado para downloads historicos</AlertDescription>
            </Alert>
          </div>
        </CardContent>
      </Card>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {renderDownloadCard(
          'Precos B3',
          'Download de precos historicos de derivativos da B3',
          'b3/precos',
          'b3-precos',
          true
        )}
        {renderDownloadCard(
          'Instrumentos B3',
          'Download de instrumentos de derivativos disponiveis',
          'b3/instrumentos',
          'b3-instrumentos',
          false
        )}
        {renderDownloadCard(
          'Renda Fixa B3',
          'Download de dados de renda fixa da B3',
          'b3/rendafixa',
          'b3-rendafixa',
          false
        )}
        {renderDownloadCard(
          'Taxas TPF - ANBIMA',
          'Download de taxas indicativas de Titulos Publicos Federais',
          'anbima/tpf',
          'anbima-tpf',
          false
        )}
        {renderDownloadCard(
          'VNA - ANBIMA',
          'Download de Valores Nominais Atualizados',
          'anbima/vna',
          'anbima-vna',
          false
        )}
        {renderDownloadCard(
          'Expectativas de Mercado',
          'Download de expectativas do Relatorio Focus do BCB',
          'bcb/expectativas',
          'bcb-expectativas',
          false
        )}
      </div>
    </div>
  );
}
