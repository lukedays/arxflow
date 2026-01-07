import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { DatePicker } from '@/components/ui/date-picker';
import { useCalculateLTNPU, useCalculateLTNTaxa } from '@/api/generated/calculadora/calculadora';

// Tipos para as respostas da API
interface CalculatePUResponse {
  pu: number;
  taxaAno: number;
  diasUteis: number;
  diasCorridos: number;
}

interface CalculateTaxaResponse {
  taxaAno: number;
  pu: number;
  diasUteis: number;
  diasCorridos: number;
}

export default function CalculadoraTitulosPage() {
  const [formData, setFormData] = useState({
    dataCotacao: new Date().toISOString().split('T')[0],
    dataVencimento: '',
    taxaAno: '',
    pu: '',
  });

  // Hooks de mutacao Orval
  const calculatePU = useCalculateLTNPU();
  const calculateTaxa = useCalculateLTNTaxa();

  const resultado = (calculatePU.data || calculateTaxa.data) as
    | CalculatePUResponse
    | CalculateTaxaResponse
    | undefined;
  const calculando = calculatePU.isPending || calculateTaxa.isPending;

  const handleCalcularPU = () => {
    calculatePU.mutate({
      data: {
        dataCotacao: new Date(formData.dataCotacao).toISOString(),
        dataVencimento: new Date(formData.dataVencimento).toISOString(),
        taxaAno: parseFloat(formData.taxaAno),
      },
    });
  };

  const handleCalcularTaxa = () => {
    calculateTaxa.mutate({
      data: {
        dataCotacao: new Date(formData.dataCotacao).toISOString(),
        dataVencimento: new Date(formData.dataVencimento).toISOString(),
        pu: parseFloat(formData.pu),
      },
    });
  };

  return (
    <div>
      <h1 className="text-3xl font-bold mb-6">Calculadora de Titulos - LTN</h1>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <Card>
          <CardHeader>
            <CardTitle>Dados do Titulo</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              <div className="space-y-2">
                <Label>Data de Cotacao</Label>
                <DatePicker
                  value={formData.dataCotacao}
                  onChange={(v) => setFormData({ ...formData, dataCotacao: v })}
                  placeholder="Data de cotacao"
                />
              </div>

              <div className="space-y-2">
                <Label>Data de Vencimento</Label>
                <DatePicker
                  value={formData.dataVencimento}
                  onChange={(v) => setFormData({ ...formData, dataVencimento: v })}
                  placeholder="Data de vencimento"
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="taxaAno">Taxa ao Ano (%)</Label>
                <Input
                  id="taxaAno"
                  type="number"
                  step="0.01"
                  placeholder="Ex: 12.50"
                  value={formData.taxaAno}
                  onChange={(e) => setFormData({ ...formData, taxaAno: e.target.value })}
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="pu">PU (Preco Unitario)</Label>
                <Input
                  id="pu"
                  type="number"
                  step="0.01"
                  placeholder="Ex: 850.00"
                  value={formData.pu}
                  onChange={(e) => setFormData({ ...formData, pu: e.target.value })}
                />
              </div>

              <div className="flex gap-4 pt-4">
                <Button
                  onClick={handleCalcularPU}
                  disabled={calculando || !formData.dataVencimento || !formData.taxaAno}
                  className="flex-1"
                >
                  Calcular PU
                </Button>

                <Button
                  onClick={handleCalcularTaxa}
                  disabled={calculando || !formData.dataVencimento || !formData.pu}
                  className="flex-1"
                >
                  Calcular Taxa
                </Button>
              </div>
            </div>
          </CardContent>
        </Card>

        {resultado && (
          <Card>
            <CardHeader>
              <CardTitle>Resultado</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {'pu' in resultado && resultado.pu !== undefined && (
                  <div className="p-4 bg-muted rounded-lg">
                    <p className="text-sm text-muted-foreground">PU Calculado</p>
                    <p className="text-2xl font-bold">{resultado.pu.toFixed(6)}</p>
                  </div>
                )}

                {'taxaAno' in resultado && resultado.taxaAno !== undefined && (
                  <div className="p-4 bg-muted rounded-lg">
                    <p className="text-sm text-muted-foreground">Taxa ao Ano</p>
                    <p className="text-2xl font-bold">{resultado.taxaAno.toFixed(4)}%</p>
                  </div>
                )}

                <div className="grid grid-cols-2 gap-4">
                  <div className="p-4 bg-muted rounded-lg">
                    <p className="text-sm text-muted-foreground">Dias Uteis</p>
                    <p className="text-xl font-semibold">{resultado.diasUteis}</p>
                  </div>

                  <div className="p-4 bg-muted rounded-lg">
                    <p className="text-sm text-muted-foreground">Dias Corridos</p>
                    <p className="text-xl font-semibold">{resultado.diasCorridos}</p>
                  </div>
                </div>

                <div className="text-sm text-muted-foreground pt-2">
                  <p>
                    Data Referencia:{' '}
                    {new Date(formData.dataCotacao).toLocaleDateString('pt-BR')}
                  </p>
                  <p>
                    Data Vencimento:{' '}
                    {new Date(formData.dataVencimento).toLocaleDateString('pt-BR')}
                  </p>
                </div>
              </div>
            </CardContent>
          </Card>
        )}
      </div>
    </div>
  );
}
