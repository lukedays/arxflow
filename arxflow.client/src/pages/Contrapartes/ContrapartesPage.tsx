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
  useGetContrapartes,
  useCreateContraparte,
  useUpdateContraparte,
  useDeleteContraparte,
  getGetContrapartesQueryKey,
} from '../../api/generated/contrapartes/contrapartes';
import type { CreateContraparteRequest, UpdateContraparteRequest } from '../../api/generated/model';

interface ContraparteFormData {
  nome: string;
}

interface Contraparte {
  id: number;
  nome?: string;
}

export default function ContrapartesPage() {
  const queryClient = useQueryClient();
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [formData, setFormData] = useState<ContraparteFormData>({
    nome: '',
  });

  const { data: contrapartes = [], isLoading } = useGetContrapartes();
  const createMutation = useCreateContraparte();
  const updateMutation = useUpdateContraparte();
  const deleteMutation = useDeleteContraparte();

  const handleOpenDialog = (contraparte?: Contraparte) => {
    if (contraparte) {
      setEditingId(contraparte.id);
      setFormData({
        nome: contraparte.nome || '',
      });
    } else {
      setEditingId(null);
      setFormData({ nome: '' });
    }
    setDialogOpen(true);
  };

  const handleCloseDialog = () => {
    setDialogOpen(false);
    setEditingId(null);
    setFormData({ nome: '' });
  };

  const handleSave = async () => {
    try {
      if (editingId) {
        const request: UpdateContraparteRequest = {
          nome: formData.nome,
        };
        await updateMutation.mutateAsync({ id: editingId, data: request });
      } else {
        const request: CreateContraparteRequest = {
          nome: formData.nome,
        };
        await createMutation.mutateAsync({ data: request });
      }
      queryClient.invalidateQueries({ queryKey: getGetContrapartesQueryKey() });
      handleCloseDialog();
    } catch (error) {
      console.error('Erro ao salvar contraparte:', error);
    }
  };

  const handleDelete = async (id: number) => {
    if (window.confirm('Deseja realmente excluir esta contraparte?')) {
      try {
        await deleteMutation.mutateAsync({ id });
        queryClient.invalidateQueries({ queryKey: getGetContrapartesQueryKey() });
      } catch (error) {
        console.error('Erro ao excluir contraparte:', error);
      }
    }
  };

  const columns: ColumnDef<Contraparte>[] = [
    { accessorKey: 'id', header: 'ID' },
    { accessorKey: 'nome', header: 'Nome' },
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
        <h1 className="text-3xl font-bold">Contrapartes</h1>
        <Button onClick={() => handleOpenDialog()}>
          <Plus className="h-4 w-4 mr-2" />
          Nova Contraparte
        </Button>
      </div>

      <Card>
        <CardContent className="p-6">
          <DataTable columns={columns} data={contrapartes as Contraparte[]} loading={isLoading} />
        </CardContent>
      </Card>

      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>{editingId ? 'Editar Contraparte' : 'Nova Contraparte'}</DialogTitle>
            <DialogDescription>
              Preencha os campos abaixo para {editingId ? 'editar a' : 'criar uma nova'} contraparte.
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
