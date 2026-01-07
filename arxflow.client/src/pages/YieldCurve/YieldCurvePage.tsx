import { useState, useEffect } from 'react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import Chart from 'react-apexcharts';
import type { ApexOptions } from 'apexcharts';

interface YieldCurvePoint {
  days: number;
  rate: number;
  maturity: string;
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
    interpolation: 'Linear',
  });

  const [curveData, setCurveData] = useState<YieldCurveData | null>(null);
  const [loading, setLoading] = useState(false);

  const handleLoadCurve = async () => {
    setLoading(true);
    try {
      const params = new URLSearchParams({
        date: new Date(formData.date).toISOString(),
        curveType: formData.curveType,
        interpolation: formData.interpolation,
      });

      const response = await fetch(`/api/yield-curve?${params}`);
      const data = await response.json();
      setCurveData(data);
    } catch (error) {
      console.error('Erro ao carregar curva:', error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    handleLoadCurve();
  }, []);

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
      categories: curveData?.points.map((p) => p.days.toString()) || [],
      title: {
        text: 'Dias Úteis',
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
  };

  const series = [
    {
      name: 'Taxa',
      data: curveData?.points.map((p) => p.rate) || [],
    },
  ];

  return (
    <div>
      <h1 className="text-3xl font-bold mb-6">Curva de Juros</h1>

      <div className="space-y-6">
        <Card>
          <CardContent className="p-6">
            <div className="grid grid-cols-1 sm:grid-cols-4 gap-4 items-end">
              <div className="space-y-2">
                <Label htmlFor="date">Data</Label>
                <Input
                  id="date"
                  type="date"
                  value={formData.date}
                  onChange={(e) => setFormData({ ...formData, date: e.target.value })}
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
                    <SelectItem value="PRE">PRE</SelectItem>
                    <SelectItem value="IPCA">IPCA</SelectItem>
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <Label>Interpolação</Label>
                <Select
                  value={formData.interpolation}
                  onValueChange={(value) => setFormData({ ...formData, interpolation: value })}
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Linear">Linear</SelectItem>
                    <SelectItem value="FlatForward">Flat Forward</SelectItem>
                  </SelectContent>
                </Select>
              </div>

              <Button onClick={handleLoadCurve} disabled={loading} className="h-10">
                {loading ? 'Carregando...' : 'Carregar Curva'}
              </Button>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            {curveData && <Chart options={chartOptions} series={series} type="line" height={400} />}
            {!curveData && !loading && (
              <div className="text-center py-16">
                <p className="text-muted-foreground">
                  Selecione uma data e clique em "Carregar Curva"
                </p>
              </div>
            )}
          </CardContent>
        </Card>

        {curveData && (
          <Card>
            <CardHeader>
              <CardTitle>Pontos da Curva</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-4">
                {curveData.points.map((point, index) => (
                  <div key={index} className="p-3 bg-muted rounded-lg">
                    <p className="text-sm text-muted-foreground">{point.days} dias úteis</p>
                    <p className="text-lg font-semibold">{point.rate.toFixed(4)}%</p>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        )}
      </div>
    </div>
  );
}
