import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';

interface Resultado {
  pu?: number;
  taxaAno?: number;
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

  const [resultado, setResultado] = useState<Resultado | null>(null);
  const [calculando, setCalculando] = useState(false);

  const handleCalcularPU = async () => {
    setCalculando(true);
    try {
      const response = await fetch('/api/calculadora/ltn/pu', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          dataCotacao: new Date(formData.dataCotacao).toISOString(),
          dataVencimento: new Date(formData.dataVencimento).toISOString(),
          taxaAno: parseFloat(formData.taxaAno),
        }),
      });

      const data = await response.json();
      setResultado(data);
    } catch (error) {
      console.error('Erro ao calcular PU:', error);
    } finally {
      setCalculando(false);
    }
  };

  const handleCalcularTaxa = async () => {
    setCalculando(true);
    try {
      const response = await fetch('/api/calculadora/ltn/taxa', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          dataCotacao: new Date(formData.dataCotacao).toISOString(),
          dataVencimento: new Date(formData.dataVencimento).toISOString(),
          pu: parseFloat(formData.pu),
        }),
      });

      const data = await response.json();
      setResultado(data);
    } catch (error) {
      console.error('Erro ao calcular Taxa:', error);
    } finally {
      setCalculando(false);
    }
  };

  return (
    <div>
      <h1 className="text-3xl font-bold mb-6">Calculadora de Títulos - LTN</h1>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <Card>
          <CardHeader>
            <CardTitle>Dados do Título</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="dataCotacao">Data de Cotação</Label>
                <Input
                  id="dataCotacao"
                  type="date"
                  value={formData.dataCotacao}
                  onChange={(e) => setFormData({ ...formData, dataCotacao: e.target.value })}
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="dataVencimento">Data de Vencimento</Label>
                <Input
                  id="dataVencimento"
                  type="date"
                  value={formData.dataVencimento}
                  onChange={(e) => setFormData({ ...formData, dataVencimento: e.target.value })}
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="taxaAno">Taxa ao Ano (%)</Label>
                <Input
                  id="taxaAno"
                  type="number"
                  step="0.01"
                  value={formData.taxaAno}
                  onChange={(e) => setFormData({ ...formData, taxaAno: e.target.value })}
                />
              </div>

              <div className="space-y-2">
                <Label htmlFor="pu">PU (Preço Unitário)</Label>
                <Input
                  id="pu"
                  type="number"
                  step="0.01"
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
              <div className="space-y-3">
                {resultado.pu !== undefined && (
                  <p>
                    <span className="font-semibold">PU:</span> {resultado.pu.toFixed(6)}
                  </p>
                )}

                {resultado.taxaAno !== undefined && (
                  <p>
                    <span className="font-semibold">Taxa ao Ano:</span> {resultado.taxaAno.toFixed(4)}%
                  </p>
                )}

                <p>
                  <span className="font-semibold">Dias Úteis:</span> {resultado.diasUteis}
                </p>

                <p>
                  <span className="font-semibold">Dias Corridos:</span> {resultado.diasCorridos}
                </p>
              </div>
            </CardContent>
          </Card>
        )}
      </div>
    </div>
  );
}
