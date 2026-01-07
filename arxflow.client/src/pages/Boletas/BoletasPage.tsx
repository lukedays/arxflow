import { useState } from 'react';
import type { ColumnDef } from '@tanstack/react-table';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Card, CardContent } from '@/components/ui/card';
import { DataTable } from '@/components/ui/data-table';
import { Combobox } from '@/components/ui/combobox';
import { Plus, Pencil, Trash2 } from 'lucide-react';
import { useQueryClient } from '@tanstack/react-query';
import {
  useGetBoletas,
  useCreateBoleta,
  useUpdateBoleta,
  useDeleteBoleta,
} from '../../api/generated/boletas/boletas';
import { useGetAtivos } from '../../api/generated/ativos/ativos';
import { useGetContrapartes } from '../../api/generated/contrapartes/contrapartes';
import { useGetFundos } from '../../api/generated/fundos/fundos';
import type { CreateBoletaRequest, UpdateBoletaRequest } from '../../api/generated/model';

interface BoletaFormData {
  ativoId: number | null;
  contraparteId: number | null;
  fundoId: number | null;
  ticker: string;
  tipoOperacao: string;
  volume: string;
  quantidade: string;
  tipoPrecificacao: string;
  taxaNominal: string;
  alocacao: string;
  usuario: string;
  observacao: string;
}

interface Boleta {
  id: number;
  ticker?: string;
  ativoNome?: string;
  tipoOperacao?: string;
  volume?: number;
  quantidade?: number;
  fundoNome?: string;
  contraparteNome?: string;
  status?: string;
  ativoId?: number;
  contraparteId?: number;
  fundoId?: number;
  tipoPrecificacao?: string;
  taxaNominal?: number;
  alocacao?: string;
  usuario?: string;
  observacao?: string;
}

export default function BoletasPage() {
  const queryClient = useQueryClient();
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [formData, setFormData] = useState<BoletaFormData>({
    ativoId: null,
    contraparteId: null,
    fundoId: null,
    ticker: '',
    tipoOperacao: 'C',
    volume: '',
    quantidade: '',
    tipoPrecificacao: 'Nominal',
    taxaNominal: '',
    alocacao: '',
    usuario: '',
    observacao: '',
  });

  const { data: boletas = [], isLoading } = useGetBoletas();
  const { data: ativos = [] } = useGetAtivos();
  const { data: contrapartes = [] } = useGetContrapartes();
  const { data: fundos = [] } = useGetFundos();
  const createMutation = useCreateBoleta();
  const updateMutation = useUpdateBoleta();
  const deleteMutation = useDeleteBoleta();

  const handleOpenDialog = (boleta?: Boleta) => {
    if (boleta) {
      setEditingId(boleta.id);
      setFormData({
        ativoId: boleta.ativoId || null,
        contraparteId: boleta.contraparteId || null,
        fundoId: boleta.fundoId || null,
        ticker: boleta.ticker || '',
        tipoOperacao: boleta.tipoOperacao || 'C',
        volume: boleta.volume?.toString() || '',
        quantidade: boleta.quantidade?.toString() || '',
        tipoPrecificacao: boleta.tipoPrecificacao || 'Nominal',
        taxaNominal: boleta.taxaNominal?.toString() || '',
        alocacao: boleta.alocacao || '',
        usuario: boleta.usuario || '',
        observacao: boleta.observacao || '',
      });
    } else {
      setEditingId(null);
      setFormData({
        ativoId: null,
        contraparteId: null,
        fundoId: null,
        ticker: '',
        tipoOperacao: 'C',
        volume: '',
        quantidade: '',
        tipoPrecificacao: 'Nominal',
        taxaNominal: '',
        alocacao: '',
        usuario: '',
        observacao: '',
      });
    }
    setDialogOpen(true);
  };

  const handleCloseDialog = () => {
    setDialogOpen(false);
    setEditingId(null);
  };

  const handleSave = async () => {
    try {
      const baseData = {
        ativoId: formData.ativoId || undefined,
        contraparteId: formData.contraparteId || undefined,
        fundoId: formData.fundoId || undefined,
        ticker: formData.ticker || undefined,
        tipoOperacao: formData.tipoOperacao,
        volume: parseFloat(formData.volume) || 0,
        quantidade: parseFloat(formData.quantidade) || 0,
        tipoPrecificacao: formData.tipoPrecificacao,
        taxaNominal: formData.taxaNominal ? parseFloat(formData.taxaNominal) : undefined,
        alocacao: formData.alocacao,
        usuario: formData.usuario,
        observacao: formData.observacao,
        status: 'AguardandoFixing',
      };

      if (editingId) {
        const request: UpdateBoletaRequest = baseData;
        await updateMutation.mutateAsync({ id: editingId, data: request });
      } else {
        const request: CreateBoletaRequest = baseData;
        await createMutation.mutateAsync({ data: request });
      }

      queryClient.invalidateQueries({ queryKey: ['getBoletas'] });
      handleCloseDialog();
    } catch (error) {
      console.error('Erro ao salvar boleta:', error);
    }
  };

  const handleDelete = async (id: number) => {
    if (window.confirm('Deseja realmente excluir esta boleta?')) {
      try {
        await deleteMutation.mutateAsync({ id });
        queryClient.invalidateQueries({ queryKey: ['getBoletas'] });
      } catch (error) {
        console.error('Erro ao excluir boleta:', error);
      }
    }
  };

  const columns: ColumnDef<Boleta>[] = [
    { accessorKey: 'id', header: 'ID' },
    { accessorKey: 'ticker', header: 'Ticker' },
    { accessorKey: 'ativoNome', header: 'Ativo' },
    { accessorKey: 'tipoOperacao', header: 'Tipo' },
    {
      accessorKey: 'volume',
      header: 'Volume',
      cell: ({ row }) => row.original.volume?.toLocaleString('pt-BR'),
    },
    {
      accessorKey: 'quantidade',
      header: 'Qtd',
      cell: ({ row }) => row.original.quantidade?.toLocaleString('pt-BR'),
    },
    { accessorKey: 'fundoNome', header: 'Fundo' },
    { accessorKey: 'contraparteNome', header: 'Contraparte' },
    { accessorKey: 'status', header: 'Status' },
    {
      id: 'actions',
      header: 'Acoes',
      cell: ({ row }) => (
        <div className="flex gap-2">
          <Button variant="ghost" size="icon" onClick={() => handleOpenDialog(row.original)}>
            <Pencil className="h-4 w-4" />
          </Button>
          <Button variant="ghost" size="icon" onClick={() => handleDelete(row.original.id)}>
            <Trash2 className="h-4 w-4" />
          </Button>
        </div>
      ),
    },
  ];

  // Opcoes para os comboboxes
  const ativoOptions = (ativos as Array<{ id?: number; codAtivo?: string }>).map((a) => ({
    value: a.id!,
    label: a.codAtivo || '',
  }));
  const fundoOptions = (fundos as Array<{ id?: number; nome?: string }>).map((f) => ({
    value: f.id!,
    label: f.nome || '',
  }));
  const contraparteOptions = (contrapartes as Array<{ id?: number; nome?: string }>).map((c) => ({
    value: c.id!,
    label: c.nome || '',
  }));

  return (
    <div>
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold">Boletas</h1>
        <Button onClick={() => handleOpenDialog()}>
          <Plus className="h-4 w-4 mr-2" />
          Nova Boleta
        </Button>
      </div>

      <Card>
        <CardContent className="p-6">
          <DataTable columns={columns} data={boletas as Boleta[]} loading={isLoading} />
        </CardContent>
      </Card>

      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>{editingId ? 'Editar Boleta' : 'Nova Boleta'}</DialogTitle>
            <DialogDescription>
              Preencha os campos abaixo para {editingId ? 'editar a' : 'criar uma nova'} boleta.
            </DialogDescription>
          </DialogHeader>

          <div className="grid grid-cols-2 gap-4 py-4">
            <div className="space-y-2">
              <Label>Ativo</Label>
              <Combobox
                options={ativoOptions}
                value={formData.ativoId}
                onChange={(value) => setFormData({ ...formData, ativoId: value as number | null })}
                placeholder="Selecione um ativo"
                searchPlaceholder="Buscar ativo..."
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="ticker">Ticker</Label>
              <Input
                id="ticker"
                value={formData.ticker}
                onChange={(e) => setFormData({ ...formData, ticker: e.target.value })}
              />
            </div>

            <div className="space-y-2">
              <Label>Tipo de Operacao</Label>
              <Select
                value={formData.tipoOperacao}
                onValueChange={(value) => setFormData({ ...formData, tipoOperacao: value })}
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="C">Compra</SelectItem>
                  <SelectItem value="V">Venda</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label>Tipo de Precificacao</Label>
              <Select
                value={formData.tipoPrecificacao}
                onValueChange={(value) => setFormData({ ...formData, tipoPrecificacao: value })}
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="Nominal">Nominal</SelectItem>
                  <SelectItem value="Spread">Spread</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label htmlFor="volume">Volume</Label>
              <Input
                id="volume"
                type="number"
                step="0.01"
                value={formData.volume}
                onChange={(e) => setFormData({ ...formData, volume: e.target.value })}
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="quantidade">Quantidade</Label>
              <Input
                id="quantidade"
                type="number"
                step="0.01"
                value={formData.quantidade}
                onChange={(e) => setFormData({ ...formData, quantidade: e.target.value })}
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="taxaNominal">Taxa Nominal (%)</Label>
              <Input
                id="taxaNominal"
                type="number"
                step="0.01"
                value={formData.taxaNominal}
                onChange={(e) => setFormData({ ...formData, taxaNominal: e.target.value })}
              />
            </div>

            <div className="space-y-2">
              <Label>Fundo</Label>
              <Combobox
                options={fundoOptions}
                value={formData.fundoId}
                onChange={(value) => setFormData({ ...formData, fundoId: value as number | null })}
                placeholder="Selecione um fundo"
                searchPlaceholder="Buscar fundo..."
              />
            </div>

            <div className="col-span-2 space-y-2">
              <Label>Contraparte</Label>
              <Combobox
                options={contraparteOptions}
                value={formData.contraparteId}
                onChange={(value) =>
                  setFormData({ ...formData, contraparteId: value as number | null })
                }
                placeholder="Selecione uma contraparte"
                searchPlaceholder="Buscar contraparte..."
              />
            </div>

            <div className="col-span-2 space-y-2">
              <Label htmlFor="alocacao">Alocacao</Label>
              <Input
                id="alocacao"
                value={formData.alocacao}
                onChange={(e) => setFormData({ ...formData, alocacao: e.target.value })}
              />
            </div>

            <div className="col-span-2 space-y-2">
              <Label htmlFor="usuario">Usuario</Label>
              <Input
                id="usuario"
                value={formData.usuario}
                onChange={(e) => setFormData({ ...formData, usuario: e.target.value })}
              />
            </div>

            <div className="col-span-2 space-y-2">
              <Label htmlFor="observacao">Observacao</Label>
              <Textarea
                id="observacao"
                rows={3}
                value={formData.observacao}
                onChange={(e) => setFormData({ ...formData, observacao: e.target.value })}
              />
            </div>
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={handleCloseDialog}>
              Cancelar
            </Button>
            <Button
              onClick={handleSave}
              disabled={createMutation.isPending || updateMutation.isPending}
            >
              {editingId ? 'Salvar' : 'Criar'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
