import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { DatePicker } from '@/components/ui/date-picker';
import Chart from 'react-apexcharts';
import type { ApexOptions } from 'apexcharts';
import { useGetYieldCurve } from '@/api/generated/yield-curve/yield-curve';

// Tipos para a resposta da API
interface YieldCurvePoint {
  days: number;
  rate: number;
  maturity: string;
  ticker?: string;
  isInterpolated: boolean;
}

interface YieldCurveData {
  date: string;
  curveType: string;
  points: YieldCurvePoint[];
}

export default function YieldCurvePage() {
  const [formData, setFormData] = useState({
    date: new Date().toISOString().split('T')[0],
    curveType: 'DI1',
    interpolation: 'FlatForward',
  });

  const [showInterpolated, setShowInterpolated] = useState(true);

  // Query para buscar curva - usa data no formato yyyy-MM-dd para evitar problemas de timezone
  const {
    data: curveData,
    isLoading,
    refetch,
  } = useGetYieldCurve({
    date: formData.date,
    curveType: formData.curveType,
    interpolation: formData.interpolation,
  }) as { data: YieldCurveData | undefined; isLoading: boolean; refetch: () => void };

  // Filtra pontos se necessario
  const filteredPoints = showInterpolated
    ? curveData?.points
    : curveData?.points?.filter((p) => !p.isInterpolated);

  const chartOptions: ApexOptions = {
    chart: {
      type: 'line',
      height: 400,
      toolbar: {
        show: true,
      },
    },
    stroke: {
      curve: 'smooth',
      width: 2,
    },
    xaxis: {
      categories: filteredPoints?.map((p) => p.days.toString()) || [],
      title: {
        text: 'Dias Uteis',
      },
    },
    yaxis: {
      title: {
        text: 'Taxa (%)',
      },
      labels: {
        formatter: (value) => value.toFixed(2) + '%',
      },
    },
    tooltip: {
      y: {
        formatter: (value) => value.toFixed(4) + '%',
      },
    },
    title: {
      text: `Curva ${curveData?.curveType || 'DI1'}`,
      align: 'center',
    },
    markers: {
      size: 4,
    },
  };

  const series = [
    {
      name: 'Taxa',
      data: filteredPoints?.map((p) => p.rate) || [],
    },
  ];

  return (
    <div>
      <h1 className="text-3xl font-bold mb-6">Curva de Juros</h1>

      <div className="space-y-6">
        <Card>
          <CardContent className="p-6">
            <div className="grid grid-cols-1 sm:grid-cols-5 gap-4 items-end">
              <div className="space-y-2">
                <Label>Data</Label>
                <DatePicker
                  value={formData.date}
                  onChange={(v) => setFormData({ ...formData, date: v })}
                  placeholder="Data de referencia"
                />
              </div>

              <div className="space-y-2">
                <Label>Tipo de Curva</Label>
                <Select
                  value={formData.curveType}
                  onValueChange={(value) => setFormData({ ...formData, curveType: value })}
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="DI1">DI1</SelectItem>
                    <SelectItem value="DAP">DAP (Cupom IPCA)</SelectItem>
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <Label>Interpolacao</Label>
                <Select
                  value={formData.interpolation}
                  onValueChange={(value) => setFormData({ ...formData, interpolation: value })}
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="FlatForward">Flat Forward 252</SelectItem>
                    <SelectItem value="Linear">Linear</SelectItem>
                    <SelectItem value="None">Sem interpolacao</SelectItem>
                  </SelectContent>
                </Select>
              </div>

              <div className="flex items-center gap-2">
                <input
                  type="checkbox"
                  id="showInterpolated"
                  checked={showInterpolated}
                  onChange={(e) => setShowInterpolated(e.target.checked)}
                  className="h-4 w-4"
                />
                <Label htmlFor="showInterpolated" className="cursor-pointer">
                  Mostrar interpolados
                </Label>
              </div>

              <Button onClick={() => refetch()} disabled={isLoading} className="h-10">
                {isLoading ? 'Carregando...' : 'Carregar Curva'}
              </Button>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            {filteredPoints && filteredPoints.length > 0 && (
              <Chart options={chartOptions} series={series} type="line" height={400} />
            )}
            {(!filteredPoints || filteredPoints.length === 0) && !isLoading && (
              <div className="text-center py-16">
                <p className="text-muted-foreground">
                  Selecione uma data e clique em "Carregar Curva"
                </p>
              </div>
            )}
          </CardContent>
        </Card>

        {filteredPoints && filteredPoints.length > 0 && (
          <Card>
            <CardHeader>
              <CardTitle>Pontos da Curva</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b">
                      <th className="text-left p-2">Ticker</th>
                      <th className="text-left p-2">Vencimento</th>
                      <th className="text-right p-2">DU</th>
                      <th className="text-right p-2">Taxa (%)</th>
                      <th className="text-center p-2">Tipo</th>
                    </tr>
                  </thead>
                  <tbody>
                    {filteredPoints.map((point, index) => (
                      <tr key={index} className="border-b hover:bg-muted/50">
                        <td className="p-2">{point.ticker || '-'}</td>
                        <td className="p-2">
                          {new Date(point.maturity).toLocaleDateString('pt-BR')}
                        </td>
                        <td className="text-right p-2">{point.days}</td>
                        <td className="text-right p-2">{point.rate.toFixed(4)}</td>
                        <td className="text-center p-2">
                          <span
                            className={`px-2 py-1 rounded text-xs ${
                              point.isInterpolated
                                ? 'bg-blue-100 text-blue-800'
                                : 'bg-green-100 text-green-800'
                            }`}
                          >
                            {point.isInterpolated ? 'Interpolado' : 'Vertice'}
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </CardContent>
          </Card>
        )}
      </div>
    </div>
  );
}
