import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { CheckCircle2, XCircle, Loader2 } from 'lucide-react';

// Tipos para a resposta da API
interface ResultadoValidacao {
  titulo: string;
  dataReferencia: string;
  dataVencimento: string;
  taxaMercado: number;
  taxaCalculada: number;
  puMercado: number;
  puCalculado: number;
  diferenciaPU: number;
  diferenciaTaxa: number;
  vna?: number;
  ok: boolean;
}

interface ResumoValidacao {
  ltnOK: number;
  ltnFalhou: number;
  ntnbOK: number;
  ntnbFalhou: number;
  ntnfOK: number;
  ntnfFalhou: number;
  lftOK: number;
  lftFalhou: number;
  ntncOK: number;
  ntncFalhou: number;
}

interface ValidacaoResponse {
  resultados: ResultadoValidacao[];
  resumo: ResumoValidacao;
  erros: string[];
}

export default function ValidacaoCalculadorasPage() {
  const [diasUteis, setDiasUteis] = useState(5);
  const [validando, setValidando] = useState(false);
  const [response, setResponse] = useState<ValidacaoResponse | null>(null);

  const executarValidacao = async () => {
    setValidando(true);
    try {
      const res = await fetch('/api/validacao/calculadoras', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ diasUteis }),
      });
      const data = await res.json();
      setResponse(data);
    } catch (error) {
      console.error('Erro na validacao:', error);
    } finally {
      setValidando(false);
    }
  };

  const renderTabela = (titulo: string) => {
    const resultados = response?.resultados.filter((r) => r.titulo === titulo) || [];
    const temVNA = ['NTN-B', 'NTN-C', 'LFT'].includes(titulo);

    return (
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b bg-muted/50">
              <th className="text-left p-2">Data Ref</th>
              <th className="text-left p-2">Vencimento</th>
              {temVNA && <th className="text-right p-2">VNA</th>}
              <th className="text-right p-2">Taxa Mercado</th>
              <th className="text-right p-2">Taxa Calc</th>
              <th className="text-right p-2">PU Mercado</th>
              <th className="text-right p-2">PU Calc</th>
              <th className="text-right p-2">Dif PU</th>
              <th className="text-right p-2">Dif Taxa</th>
              <th className="text-center p-2">Status</th>
            </tr>
          </thead>
          <tbody>
            {resultados.map((r, idx) => (
              <tr key={idx} className="border-b hover:bg-muted/30">
                <td className="p-2">{new Date(r.dataReferencia).toLocaleDateString('pt-BR')}</td>
                <td className="p-2">{new Date(r.dataVencimento).toLocaleDateString('pt-BR')}</td>
                {temVNA && <td className="text-right p-2">{r.vna?.toFixed(6)}</td>}
                <td className="text-right p-2">{r.taxaMercado.toFixed(4)}%</td>
                <td className="text-right p-2">{r.taxaCalculada.toFixed(4)}%</td>
                <td className="text-right p-2">{r.puMercado.toFixed(6)}</td>
                <td className="text-right p-2">{r.puCalculado.toFixed(6)}</td>
                <td className="text-right p-2">{r.diferenciaPU.toFixed(9)}</td>
                <td className="text-right p-2">{r.diferenciaTaxa.toFixed(6)}%</td>
                <td className="text-center p-2">
                  {r.ok ? (
                    <span className="inline-flex items-center px-2 py-1 rounded text-xs bg-green-100 text-green-800">
                      <CheckCircle2 className="h-3 w-3 mr-1" />
                      OK
                    </span>
                  ) : (
                    <span className="inline-flex items-center px-2 py-1 rounded text-xs bg-red-100 text-red-800">
                      <XCircle className="h-3 w-3 mr-1" />
                      FALHOU
                    </span>
                  )}
                </td>
              </tr>
            ))}
            {resultados.length === 0 && (
              <tr>
                <td colSpan={temVNA ? 10 : 9} className="text-center p-4 text-muted-foreground">
                  Nenhum resultado para {titulo}
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    );
  };

  const resumo = response?.resumo;

  return (
    <div>
      <h1 className="text-3xl font-bold mb-6">Validacao de Calculadoras</h1>

      <Card className="mb-6">
        <CardContent className="p-6">
          <div className="flex items-end gap-4">
            <div className="space-y-2">
              <Label htmlFor="diasUteis">Dias Uteis</Label>
              <Input
                id="diasUteis"
                type="number"
                min={1}
                max={30}
                value={diasUteis}
                onChange={(e) => setDiasUteis(parseInt(e.target.value) || 1)}
                className="w-32"
              />
            </div>
            <Button onClick={executarValidacao} disabled={validando}>
              {validando ? (
                <>
                  <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                  Validando...
                </>
              ) : (
                <>
                  <CheckCircle2 className="h-4 w-4 mr-2" />
                  Executar Validacao
                </>
              )}
            </Button>
          </div>
        </CardContent>
      </Card>

      {resumo && (
        <Card className="mb-6">
          <CardHeader>
            <CardTitle>Resumo</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-2 sm:grid-cols-5 gap-4">
              <div className="text-center p-3 bg-muted rounded-lg">
                <p className="text-sm text-muted-foreground">LTN</p>
                <p className="font-semibold">
                  <span className="text-green-600">{resumo.ltnOK} OK</span>
                  {resumo.ltnFalhou > 0 && (
                    <span className="text-red-600 ml-2">{resumo.ltnFalhou} Falhou</span>
                  )}
                </p>
              </div>
              <div className="text-center p-3 bg-muted rounded-lg">
                <p className="text-sm text-muted-foreground">NTN-B</p>
                <p className="font-semibold">
                  <span className="text-green-600">{resumo.ntnbOK} OK</span>
                  {resumo.ntnbFalhou > 0 && (
                    <span className="text-red-600 ml-2">{resumo.ntnbFalhou} Falhou</span>
                  )}
                </p>
              </div>
              <div className="text-center p-3 bg-muted rounded-lg">
                <p className="text-sm text-muted-foreground">NTN-F</p>
                <p className="font-semibold">
                  <span className="text-green-600">{resumo.ntnfOK} OK</span>
                  {resumo.ntnfFalhou > 0 && (
                    <span className="text-red-600 ml-2">{resumo.ntnfFalhou} Falhou</span>
                  )}
                </p>
              </div>
              <div className="text-center p-3 bg-muted rounded-lg">
                <p className="text-sm text-muted-foreground">LFT</p>
                <p className="font-semibold">
                  <span className="text-green-600">{resumo.lftOK} OK</span>
                  {resumo.lftFalhou > 0 && (
                    <span className="text-red-600 ml-2">{resumo.lftFalhou} Falhou</span>
                  )}
                </p>
              </div>
              <div className="text-center p-3 bg-muted rounded-lg">
                <p className="text-sm text-muted-foreground">NTN-C</p>
                <p className="font-semibold">
                  <span className="text-green-600">{resumo.ntncOK} OK</span>
                  {resumo.ntncFalhou > 0 && (
                    <span className="text-red-600 ml-2">{resumo.ntncFalhou} Falhou</span>
                  )}
                </p>
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      {response && (
        <Card>
          <CardContent className="p-6">
            <Tabs defaultValue="ltn">
              <TabsList className="mb-4">
                <TabsTrigger value="ltn">LTN</TabsTrigger>
                <TabsTrigger value="ntnb">NTN-B</TabsTrigger>
                <TabsTrigger value="ntnf">NTN-F</TabsTrigger>
                <TabsTrigger value="lft">LFT</TabsTrigger>
                <TabsTrigger value="ntnc">NTN-C</TabsTrigger>
                <TabsTrigger value="erros">Erros ({response.erros.length})</TabsTrigger>
              </TabsList>

              <TabsContent value="ltn">{renderTabela('LTN')}</TabsContent>
              <TabsContent value="ntnb">{renderTabela('NTN-B')}</TabsContent>
              <TabsContent value="ntnf">{renderTabela('NTN-F')}</TabsContent>
              <TabsContent value="lft">{renderTabela('LFT')}</TabsContent>
              <TabsContent value="ntnc">{renderTabela('NTN-C')}</TabsContent>
              <TabsContent value="erros">
                {response.erros.length > 0 ? (
                  <div className="space-y-2">
                    {response.erros.map((erro, idx) => (
                      <Alert key={idx} variant="destructive">
                        <AlertDescription>{erro}</AlertDescription>
                      </Alert>
                    ))}
                  </div>
                ) : (
                  <p className="text-muted-foreground">Nenhum erro encontrado.</p>
                )}
              </TabsContent>
            </Tabs>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
