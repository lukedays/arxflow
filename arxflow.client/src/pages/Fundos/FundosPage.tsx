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
  useGetFundos,
  useCreateFundo,
  useUpdateFundo,
  useDeleteFundo,
} from '../../api/generated/fundos/fundos';
import type { CreateFundoRequest, UpdateFundoRequest } from '../../api/generated/model';

interface FundoFormData {
  nome: string;
  cnpj: string;
  alphaToolsId: string;
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

  const handleOpenDialog = (fundo?: any) => {
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
      queryClient.invalidateQueries({ queryKey: ['getFundos'] });
      handleCloseDialog();
    } catch (error) {
      console.error('Erro ao salvar fundo:', error);
    }
  };

  const handleDelete = async (id: number) => {
    if (window.confirm('Deseja realmente excluir este fundo?')) {
      try {
        await deleteMutation.mutateAsync({ id });
        queryClient.invalidateQueries({ queryKey: ['getFundos'] });
      } catch (error) {
        console.error('Erro ao excluir fundo:', error);
      }
    }
  };

  const columns: GridColDef[] = [
    { field: 'id', headerName: 'ID', width: 80 },
    { field: 'nome', headerName: 'Nome', flex: 1, minWidth: 200 },
    { field: 'cnpj', headerName: 'CNPJ', width: 180 },
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
        <Typography variant="h4">Fundos</Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => handleOpenDialog()}
        >
          Novo Fundo
        </Button>
      </Box>

      <Paper sx={{ height: 600, width: '100%' }}>
        <DataGrid
          rows={fundos}
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
        <DialogTitle>{editingId ? 'Editar Fundo' : 'Novo Fundo'}</DialogTitle>
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
              label="CNPJ"
              value={formData.cnpj}
              onChange={(e) => setFormData({ ...formData, cnpj: e.target.value })}
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
