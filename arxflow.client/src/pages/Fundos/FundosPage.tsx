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
import { Plus, Pencil, Trash2 } from 'lucide-react';
import { useQueryClient } from '@tanstack/react-query';
import {
  useGetFundos,
  useCreateFundo,
  useUpdateFundo,
  useDeleteFundo,
  getGetFundosQueryKey,
} from '../../api/generated/fundos/fundos';
import type { CreateFundoRequest, UpdateFundoRequest } from '../../api/generated/model';

interface FundoFormData {
  nome: string;
  cnpj: string;
  alphaToolsId: string;
}

interface Fundo {
  id: number;
  nome?: string;
  cnpj?: string;
  alphaToolsId?: string;
}

export default function FundosPage() {
  const queryClient = useQueryClient();
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [formData, setFormData] = useState<FundoFormData>({
    nome: '',
    cnpj: '',
    alphaToolsId: '',
  });

  const { data: fundos = [], isLoading } = useGetFundos();
  const createMutation = useCreateFundo();
  const updateMutation = useUpdateFundo();
  const deleteMutation = useDeleteFundo();

  const handleOpenDialog = (fundo?: Fundo) => {
    if (fundo) {
      setEditingId(fundo.id);
      setFormData({
        nome: fundo.nome || '',
        cnpj: fundo.cnpj || '',
        alphaToolsId: fundo.alphaToolsId || '',
      });
    } else {
      setEditingId(null);
      setFormData({ nome: '', cnpj: '', alphaToolsId: '' });
    }
    setDialogOpen(true);
  };

  const handleCloseDialog = () => {
    setDialogOpen(false);
    setEditingId(null);
    setFormData({ nome: '', cnpj: '', alphaToolsId: '' });
  };

  const handleSave = async () => {
    try {
      if (editingId) {
        const request: UpdateFundoRequest = {
          nome: formData.nome,
          cnpj: formData.cnpj || undefined,
          alphaToolsId: formData.alphaToolsId || undefined,
        };
        await updateMutation.mutateAsync({ id: editingId, data: request });
      } else {
        const request: CreateFundoRequest = {
          nome: formData.nome,
          cnpj: formData.cnpj || undefined,
          alphaToolsId: formData.alphaToolsId || undefined,
        };
        await createMutation.mutateAsync({ data: request });
      }
      queryClient.invalidateQueries({ queryKey: getGetFundosQueryKey() });
      handleCloseDialog();
    } catch (error) {
      console.error('Erro ao salvar fundo:', error);
    }
  };

  const handleDelete = async (id: number) => {
    if (window.confirm('Deseja realmente excluir este fundo?')) {
      try {
        await deleteMutation.mutateAsync({ id });
        queryClient.invalidateQueries({ queryKey: getGetFundosQueryKey() });
      } catch (error) {
        console.error('Erro ao excluir fundo:', error);
      }
    }
  };

  const columns: ColumnDef<Fundo>[] = [
    { accessorKey: 'id', header: 'ID' },
    { accessorKey: 'nome', header: 'Nome' },
    { accessorKey: 'cnpj', header: 'CNPJ' },
    { accessorKey: 'alphaToolsId', header: 'AlphaTools ID' },
    {
      id: 'actions',
      header: 'Ações',
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

  return (
    <div>
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold">Fundos</h1>
        <Button onClick={() => handleOpenDialog()}>
          <Plus className="h-4 w-4 mr-2" />
          Novo Fundo
        </Button>
      </div>

      <Card>
        <CardContent className="p-6">
          <DataTable columns={columns} data={fundos as Fundo[]} loading={isLoading} />
        </CardContent>
      </Card>

      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>{editingId ? 'Editar Fundo' : 'Novo Fundo'}</DialogTitle>
            <DialogDescription>
              Preencha os campos abaixo para {editingId ? 'editar o' : 'criar um novo'} fundo.
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-4 py-4">
            <div className="space-y-2">
              <Label htmlFor="nome">Nome</Label>
              <Input
                id="nome"
                value={formData.nome}
                onChange={(e) => setFormData({ ...formData, nome: e.target.value })}
                required
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="cnpj">CNPJ</Label>
              <Input
                id="cnpj"
                value={formData.cnpj}
                onChange={(e) => setFormData({ ...formData, cnpj: e.target.value })}
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
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={handleCloseDialog}>
              Cancelar
            </Button>
            <Button
              onClick={handleSave}
              disabled={!formData.nome || createMutation.isPending || updateMutation.isPending}
            >
              {editingId ? 'Salvar' : 'Criar'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
