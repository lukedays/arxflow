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
import { Card, CardContent } from '@/components/ui/card';
import { DataTable } from '@/components/ui/data-table';
import { Combobox } from '@/components/ui/combobox';
import { Plus, Pencil, Trash2 } from 'lucide-react';
import { useQueryClient } from '@tanstack/react-query';
import {
  useGetAtivos,
  useCreateAtivo,
  useUpdateAtivo,
  useDeleteAtivo,
} from '../../api/generated/ativos/ativos';
import { useGetEmissores } from '../../api/generated/emissores/emissores';
import type { CreateAtivoRequest, UpdateAtivoRequest } from '../../api/generated/model';

interface AtivoFormData {
  codAtivo: string;
  tipoAtivo: string;
  emissorId: number | null;
  alphaToolsId: string;
  dataVencimento: string;
}

interface Ativo {
  id: number;
  codAtivo?: string;
  tipoAtivo?: string;
  emissorId?: number;
  emissorNome?: string;
  alphaToolsId?: string;
  dataVencimento?: string;
}

export default function AtivosPage() {
  const queryClient = useQueryClient();
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [formData, setFormData] = useState<AtivoFormData>({
    codAtivo: '',
    tipoAtivo: '',
    emissorId: null,
    alphaToolsId: '',
    dataVencimento: '',
  });

  const { data: ativos = [], isLoading } = useGetAtivos();
  const { data: emissores = [] } = useGetEmissores();
  const createMutation = useCreateAtivo();
  const updateMutation = useUpdateAtivo();
  const deleteMutation = useDeleteAtivo();

  const handleOpenDialog = (ativo?: Ativo) => {
    if (ativo) {
      setEditingId(ativo.id);
      setFormData({
        codAtivo: ativo.codAtivo || '',
        tipoAtivo: ativo.tipoAtivo || '',
        emissorId: ativo.emissorId || null,
        alphaToolsId: ativo.alphaToolsId || '',
        dataVencimento: ativo.dataVencimento ? ativo.dataVencimento.split('T')[0] : '',
      });
    } else {
      setEditingId(null);
      setFormData({
        codAtivo: '',
        tipoAtivo: '',
        emissorId: null,
        alphaToolsId: '',
        dataVencimento: '',
      });
    }
    setDialogOpen(true);
  };

  const handleCloseDialog = () => {
    setDialogOpen(false);
    setEditingId(null);
    setFormData({
      codAtivo: '',
      tipoAtivo: '',
      emissorId: null,
      alphaToolsId: '',
      dataVencimento: '',
    });
  };

  const handleSave = async () => {
    try {
      if (editingId) {
        const request: UpdateAtivoRequest = {
          codAtivo: formData.codAtivo,
          tipoAtivo: formData.tipoAtivo || undefined,
          emissorId: formData.emissorId || undefined,
          alphaToolsId: formData.alphaToolsId || undefined,
          dataVencimento: formData.dataVencimento
            ? new Date(formData.dataVencimento).toISOString()
            : undefined,
        };
        await updateMutation.mutateAsync({ id: editingId, data: request });
      } else {
        const request: CreateAtivoRequest = {
          codAtivo: formData.codAtivo,
          tipoAtivo: formData.tipoAtivo || undefined,
          emissorId: formData.emissorId || undefined,
          alphaToolsId: formData.alphaToolsId || undefined,
          dataVencimento: formData.dataVencimento
            ? new Date(formData.dataVencimento).toISOString()
            : undefined,
        };
        await createMutation.mutateAsync({ data: request });
      }
      queryClient.invalidateQueries({ queryKey: ['getAtivos'] });
      handleCloseDialog();
    } catch (error) {
      console.error('Erro ao salvar ativo:', error);
    }
  };

  const handleDelete = async (id: number) => {
    if (window.confirm('Deseja realmente excluir este ativo?')) {
      try {
        await deleteMutation.mutateAsync({ id });
        queryClient.invalidateQueries({ queryKey: ['getAtivos'] });
      } catch (error) {
        console.error('Erro ao excluir ativo:', error);
      }
    }
  };

  const columns: ColumnDef<Ativo>[] = [
    { accessorKey: 'id', header: 'ID' },
    { accessorKey: 'codAtivo', header: 'Codigo' },
    { accessorKey: 'tipoAtivo', header: 'Tipo' },
    { accessorKey: 'emissorNome', header: 'Emissor' },
    { accessorKey: 'alphaToolsId', header: 'AlphaTools ID' },
    {
      accessorKey: 'dataVencimento',
      header: 'Vencimento',
      cell: ({ row }) => {
        const value = row.original.dataVencimento;
        if (!value) return '';
        return new Date(value).toLocaleDateString('pt-BR');
      },
    },
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

  // Opcoes para o combobox de emissores
  const emissorOptions = (emissores as Array<{ id?: number; nome?: string }>).map((e) => ({
    value: e.id!,
    label: e.nome || '',
  }));

  return (
    <div>
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold">Ativos</h1>
        <Button onClick={() => handleOpenDialog()}>
          <Plus className="h-4 w-4 mr-2" />
          Novo Ativo
        </Button>
      </div>

      <Card>
        <CardContent className="p-6">
          <DataTable columns={columns} data={ativos as Ativo[]} loading={isLoading} />
        </CardContent>
      </Card>

      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>{editingId ? 'Editar Ativo' : 'Novo Ativo'}</DialogTitle>
            <DialogDescription>
              Preencha os campos abaixo para {editingId ? 'editar o' : 'criar um novo'} ativo.
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-4 py-4">
            <div className="space-y-2">
              <Label htmlFor="codAtivo">Codigo do Ativo</Label>
              <Input
                id="codAtivo"
                value={formData.codAtivo}
                onChange={(e) => setFormData({ ...formData, codAtivo: e.target.value })}
                required
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="tipoAtivo">Tipo de Ativo</Label>
              <Input
                id="tipoAtivo"
                value={formData.tipoAtivo}
                onChange={(e) => setFormData({ ...formData, tipoAtivo: e.target.value })}
              />
            </div>

            <div className="space-y-2">
              <Label>Emissor</Label>
              <Combobox
                options={emissorOptions}
                value={formData.emissorId}
                onChange={(value) =>
                  setFormData({ ...formData, emissorId: value as number | null })
                }
                placeholder="Selecione um emissor"
                searchPlaceholder="Buscar emissor..."
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="alphaToolsId">AlphaTools ID</Label>
              <Input
                id="alphaToolsId"
                value={formData.alphaToolsId}
                onChange={(e) => setFormData({ ...formData, alphaToolsId: e.target.value })}
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
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={handleCloseDialog}>
              Cancelar
            </Button>
            <Button
              onClick={handleSave}
              disabled={
                !formData.codAtivo || createMutation.isPending || updateMutation.isPending
              }
            >
              {editingId ? 'Salvar' : 'Criar'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
