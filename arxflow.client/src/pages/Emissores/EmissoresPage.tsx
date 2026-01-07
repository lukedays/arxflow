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
  IconButton,
} from '@mui/material';
import { DataGrid, GridActionsCellItem } from '@mui/x-data-grid';
import type { GridColDef } from '@mui/x-data-grid';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import { useQueryClient } from '@tanstack/react-query';
import {
  useGetEmissores,
  useCreateEmissor,
  useUpdateEmissor,
  useDeleteEmissor,
} from '../../api/generated/emissores/emissores';
import type { CreateEmissorRequest, UpdateEmissorRequest } from '../../api/generated/model';

interface EmissorFormData {
  nome: string;
  documento: string;
  alphaToolsId: string;
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

  const handleOpenDialog = (emissor?: any) => {
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
      queryClient.invalidateQueries({ queryKey: ['getEmissores'] });
      handleCloseDialog();
    } catch (error) {
      console.error('Erro ao salvar emissor:', error);
    }
  };

  const handleDelete = async (id: number) => {
    if (window.confirm('Deseja realmente excluir este emissor?')) {
      try {
        await deleteMutation.mutateAsync({ id });
        queryClient.invalidateQueries({ queryKey: ['getEmissores'] });
      } catch (error) {
        console.error('Erro ao excluir emissor:', error);
      }
    }
  };

  const columns: GridColDef[] = [
    { field: 'id', headerName: 'ID', width: 80 },
    { field: 'nome', headerName: 'Nome', flex: 1, minWidth: 200 },
    { field: 'documento', headerName: 'Documento', width: 150 },
    { field: 'alphaToolsId', headerName: 'AlphaTools ID', width: 150 },
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
        <Typography variant="h4">Emissores</Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => handleOpenDialog()}
        >
          Novo Emissor
        </Button>
      </Box>

      <Paper sx={{ height: 600, width: '100%' }}>
        <DataGrid
          rows={emissores}
          columns={columns}
          loading={isLoading}
          pageSizeOptions={[10, 25, 50, 100]}
          initialState={{
            pagination: { paginationModel: { pageSize: 25 } },
          }}
          disableRowSelectionOnClick
        />
      </Paper>

      {/* Dialog de Criação/Edição */}
      <Dialog open={dialogOpen} onClose={handleCloseDialog} maxWidth="sm" fullWidth>
        <DialogTitle>{editingId ? 'Editar Emissor' : 'Novo Emissor'}</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 2 }}>
            <TextField
              label="Nome"
              value={formData.nome}
              onChange={(e) => setFormData({ ...formData, nome: e.target.value })}
              required
              fullWidth
            />
            <TextField
              label="Documento"
              value={formData.documento}
              onChange={(e) => setFormData({ ...formData, documento: e.target.value })}
              fullWidth
            />
            <TextField
              label="AlphaTools ID"
              value={formData.alphaToolsId}
              onChange={(e) => setFormData({ ...formData, alphaToolsId: e.target.value })}
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
