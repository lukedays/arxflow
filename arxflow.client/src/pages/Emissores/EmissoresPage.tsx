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
  useGetEmissores,
  useCreateEmissor,
  useUpdateEmissor,
  useDeleteEmissor,
  getGetEmissoresQueryKey,
} from '../../api/generated/emissores/emissores';
import type { CreateEmissorRequest, UpdateEmissorRequest } from '../../api/generated/model';

interface EmissorFormData {
  nome: string;
  documento: string;
  alphaToolsId: string;
}

interface Emissor {
  id: number;
  nome?: string;
  documento?: string;
  alphaToolsId?: string;
}

export default function EmissoresPage() {
  const queryClient = useQueryClient();
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [formData, setFormData] = useState<EmissorFormData>({
    nome: '',
    documento: '',
    alphaToolsId: '',
  });

  // Hooks gerados pelo Orval
  const { data: emissores = [], isLoading } = useGetEmissores();
  const createMutation = useCreateEmissor();
  const updateMutation = useUpdateEmissor();
  const deleteMutation = useDeleteEmissor();

  const handleOpenDialog = (emissor?: Emissor) => {
    if (emissor) {
      setEditingId(emissor.id);
      setFormData({
        nome: emissor.nome || '',
        documento: emissor.documento || '',
        alphaToolsId: emissor.alphaToolsId || '',
      });
    } else {
      setEditingId(null);
      setFormData({ nome: '', documento: '', alphaToolsId: '' });
    }
    setDialogOpen(true);
  };

  const handleCloseDialog = () => {
    setDialogOpen(false);
    setEditingId(null);
    setFormData({ nome: '', documento: '', alphaToolsId: '' });
  };

  const handleSave = async () => {
    try {
      if (editingId) {
        const request: UpdateEmissorRequest = {
          nome: formData.nome,
          documento: formData.documento || undefined,
          alphaToolsId: formData.alphaToolsId || undefined,
        };
        await updateMutation.mutateAsync({ id: editingId, data: request });
      } else {
        const request: CreateEmissorRequest = {
          nome: formData.nome,
          documento: formData.documento || undefined,
          alphaToolsId: formData.alphaToolsId || undefined,
        };
        await createMutation.mutateAsync({ data: request });
      }
      queryClient.invalidateQueries({ queryKey: getGetEmissoresQueryKey() });
      handleCloseDialog();
    } catch (error) {
      console.error('Erro ao salvar emissor:', error);
    }
  };

  const handleDelete = async (id: number) => {
    if (window.confirm('Deseja realmente excluir este emissor?')) {
      try {
        await deleteMutation.mutateAsync({ id });
        queryClient.invalidateQueries({ queryKey: getGetEmissoresQueryKey() });
      } catch (error) {
        console.error('Erro ao excluir emissor:', error);
      }
    }
  };

  const columns: ColumnDef<Emissor>[] = [
    { accessorKey: 'id', header: 'ID' },
    { accessorKey: 'nome', header: 'Nome' },
    { accessorKey: 'documento', header: 'Documento' },
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
        <h1 className="text-3xl font-bold">Emissores</h1>
        <Button onClick={() => handleOpenDialog()}>
          <Plus className="h-4 w-4 mr-2" />
          Novo Emissor
        </Button>
      </div>

      <Card>
        <CardContent className="p-6">
          <DataTable columns={columns} data={emissores as Emissor[]} loading={isLoading} />
        </CardContent>
      </Card>

      {/* Dialog de Criação/Edição */}
      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>{editingId ? 'Editar Emissor' : 'Novo Emissor'}</DialogTitle>
            <DialogDescription>
              Preencha os campos abaixo para {editingId ? 'editar o' : 'criar um novo'} emissor.
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
              <Label htmlFor="documento">Documento</Label>
              <Input
                id="documento"
                value={formData.documento}
                onChange={(e) => setFormData({ ...formData, documento: e.target.value })}
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
