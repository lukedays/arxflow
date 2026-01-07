import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from '@/components/ui/card';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Progress } from '@/components/ui/progress';
import { DatePicker } from '@/components/ui/date-picker';
import { Download, AlertCircle, CheckCircle2 } from 'lucide-react';
import {
  useDownloadB3Precos,
  useDownloadB3Instrumentos,
  useDownloadB3RendaFixa,
  useDownloadAnbimaTpf,
  useDownloadAnbimaVna,
  useDownloadBcbExpectativas,
} from '@/api/generated/downloads/downloads';
import type { DownloadRequest } from '@/api/generated/model';

// Tipo de resposta dos endpoints de download
interface DownloadResponse {
  success: boolean;
  message: string;
  recordsProcessed: number;
  errors?: string[] | null;
}

export default function DownloadsPage() {
  const [dateRange, setDateRange] = useState({
    startDate: new Date(Date.now() - 5 * 24 * 60 * 60 * 1000).toISOString().split('T')[0],
    endDate: new Date().toISOString().split('T')[0],
  });

  // Hooks de mutacao Orval
  const b3Precos = useDownloadB3Precos();
  const b3Instrumentos = useDownloadB3Instrumentos();
  const b3RendaFixa = useDownloadB3RendaFixa();
  const anbimaTpf = useDownloadAnbimaTpf();
  const anbimaVna = useDownloadAnbimaVna();
  const bcbExpectativas = useDownloadBcbExpectativas();

  const getRequest = (): DownloadRequest => ({
    startDate: dateRange.startDate,
    endDate: dateRange.endDate,
  });

  const renderDownloadCard = (
    title: string,
    description: string,
    mutation: {
      mutate: (params: { data: DownloadRequest }) => void;
      isPending: boolean;
      data?: unknown;
      error?: unknown;
    }
  ) => {
    const result = mutation.data as DownloadResponse | undefined;

    return (
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">{title}</CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground mb-4">{description}</p>

          {mutation.isPending && <Progress value={50} className="mb-4" />}

          {result && (
            <Alert variant={result.success ? 'default' : 'destructive'} className="mb-4">
              {result.success ? (
                <CheckCircle2 className="h-4 w-4" />
              ) : (
                <AlertCircle className="h-4 w-4" />
              )}
              <AlertDescription>
                {result.message}
                {result.recordsProcessed > 0 && (
                  <span className="block text-xs mt-1">
                    Registros processados: {result.recordsProcessed}
                  </span>
                )}
              </AlertDescription>
            </Alert>
          )}

          {mutation.error && (
            <Alert variant="destructive" className="mb-4">
              <AlertCircle className="h-4 w-4" />
              <AlertDescription>Erro ao processar download</AlertDescription>
            </Alert>
          )}
        </CardContent>
        <CardFooter>
          <Button
            onClick={() => mutation.mutate({ data: getRequest() })}
            disabled={mutation.isPending}
            className="w-full"
          >
            <Download className="h-4 w-4 mr-2" />
            {mutation.isPending ? 'Processando...' : 'Iniciar Download'}
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
                onChange={(v) => setDateRange({ ...dateRange, startDate: v })}
                placeholder="Data inicial"
              />
            </div>
            <div className="space-y-2">
              <Label>Data Final</Label>
              <DatePicker
                value={dateRange.endDate}
                onChange={(v) => setDateRange({ ...dateRange, endDate: v })}
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
          b3Precos
        )}
        {renderDownloadCard(
          'Instrumentos B3',
          'Download de instrumentos de derivativos disponiveis',
          b3Instrumentos
        )}
        {renderDownloadCard(
          'Renda Fixa B3',
          'Download de dados de renda fixa da B3',
          b3RendaFixa
        )}
        {renderDownloadCard(
          'Taxas TPF - ANBIMA',
          'Download de taxas indicativas de Titulos Publicos Federais',
          anbimaTpf
        )}
        {renderDownloadCard(
          'VNA - ANBIMA',
          'Download de Valores Nominais Atualizados',
          anbimaVna
        )}
        {renderDownloadCard(
          'Expectativas de Mercado',
          'Download de expectativas do Relatorio Focus do BCB',
          bcbExpectativas
        )}
      </div>
    </div>
  );
}
