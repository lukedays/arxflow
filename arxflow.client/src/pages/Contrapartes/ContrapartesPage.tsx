import { useState } from 'react';
import {
  Box,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Paper,
  Typography,
} from '@mui/material';
import { DataGrid, GridActionsCellItem } from '@mui/x-data-grid';
import type { GridColDef } from '@mui/x-data-grid';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import { useQueryClient } from '@tanstack/react-query';
import {
  useGetContrapartes,
  useCreateContraparte,
  useUpdateContraparte,
  useDeleteContraparte,
} from '../../api/generated/contrapartes/contrapartes';
import type { CreateContraparteRequest, UpdateContraparteRequest } from '../../api/generated/model';

interface ContraparteFormData {
  nome: string;
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

  const handleOpenDialog = (contraparte?: any) => {
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
      queryClient.invalidateQueries({ queryKey: ['getContrapartes'] });
      handleCloseDialog();
    } catch (error) {
      console.error('Erro ao salvar contraparte:', error);
    }
  };

  const handleDelete = async (id: number) => {
    if (window.confirm('Deseja realmente excluir esta contraparte?')) {
      try {
        await deleteMutation.mutateAsync({ id });
        queryClient.invalidateQueries({ queryKey: ['getContrapartes'] });
      } catch (error) {
        console.error('Erro ao excluir contraparte:', error);
      }
    }
  };

  const columns: GridColDef[] = [
    { field: 'id', headerName: 'ID', width: 80 },
    { field: 'nome', headerName: 'Nome', flex: 1, minWidth: 300 },
    {
      field: 'actions',
      type: 'actions',
      headerName: 'Ações',
      width: 100,
      getActions: (params) => [
        <GridActionsCellItem
          icon={<EditIcon />}
          label="Editar"
          onClick={() => handleOpenDialog(params.row)}
        />,
        <GridActionsCellItem
          icon={<DeleteIcon />}
          label="Excluir"
          onClick={() => handleDelete(params.row.id)}
        />,
      ],
    },
  ];

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4">Contrapartes</Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => handleOpenDialog()}
        >
          Nova Contraparte
        </Button>
      </Box>

      <Paper sx={{ height: 600, width: '100%' }}>
        <DataGrid
          rows={contrapartes}
          columns={columns}
          loading={isLoading}
          pageSizeOptions={[10, 25, 50, 100]}
          initialState={{
            pagination: { paginationModel: { pageSize: 25 } },
          }}
          disableRowSelectionOnClick
        />
      </Paper>

      <Dialog open={dialogOpen} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
        <DialogTitle>{editingId ? 'Editar Contraparte' : 'Nova Contraparte'}</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 2 }}>
            <TextField
              label="Nome"
              value={formData.nome}
              onChange={(e) => setFormData({ ...formData, nome: e.target.value })}
              required
              fullWidth
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancelar</Button>
          <Button
            onClick={handleSave}
            variant="contained"
            disabled={!formData.nome || createMutation.isPending || updateMutation.isPending}
          >
            {editingId ? 'Salvar' : 'Criar'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
