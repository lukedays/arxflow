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
  Autocomplete,
} from '@mui/material';
import { DataGrid, GridActionsCellItem } from '@mui/x-data-grid';
import type { GridColDef } from '@mui/x-data-grid';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import { useQueryClient } from '@tanstack/react-query';
import {
  useGetAtivos,
  useCreateAtivo,
  useUpdateAtivo,
  useDeleteAtivo,
  useSearchAtivos,
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

  const handleOpenDialog = (ativo?: any) => {
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
          dataVencimento: formData.dataVencimento ? new Date(formData.dataVencimento).toISOString() : undefined,
        };
        await updateMutation.mutateAsync({ id: editingId, data: request });
      } else {
        const request: CreateAtivoRequest = {
          codAtivo: formData.codAtivo,
          tipoAtivo: formData.tipoAtivo || undefined,
          emissorId: formData.emissorId || undefined,
          alphaToolsId: formData.alphaToolsId || undefined,
          dataVencimento: formData.dataVencimento ? new Date(formData.dataVencimento).toISOString() : undefined,
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

  const columns: GridColDef[] = [
    { field: 'id', headerName: 'ID', width: 80 },
    { field: 'codAtivo', headerName: 'Código', flex: 1, minWidth: 150 },
    { field: 'tipoAtivo', headerName: 'Tipo', width: 120 },
    { field: 'emissorNome', headerName: 'Emissor', flex: 1, minWidth: 200 },
    { field: 'alphaToolsId', headerName: 'AlphaTools ID', width: 150 },
    {
      field: 'dataVencimento',
      headerName: 'Vencimento',
      width: 130,
      valueFormatter: (value) => {
        if (!value) return '';
        return new Date(value).toLocaleDateString('pt-BR');
      },
    },
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

  const emissorSelecionado = emissores.find(e => e.id === formData.emissorId);

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4">Ativos</Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => handleOpenDialog()}
        >
          Novo Ativo
        </Button>
      </Box>

      <Paper sx={{ height: 600, width: '100%' }}>
        <DataGrid
          rows={ativos}
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
        <DialogTitle>{editingId ? 'Editar Ativo' : 'Novo Ativo'}</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 2 }}>
            <TextField
              label="Código do Ativo"
              value={formData.codAtivo}
              onChange={(e) => setFormData({ ...formData, codAtivo: e.target.value })}
              required
              fullWidth
            />
            <TextField
              label="Tipo de Ativo"
              value={formData.tipoAtivo}
              onChange={(e) => setFormData({ ...formData, tipoAtivo: e.target.value })}
              fullWidth
            />
            <Autocomplete
              options={emissores}
              getOptionLabel={(option) => option.nome || ''}
              value={emissorSelecionado || null}
              onChange={(_, newValue) => {
                setFormData({ ...formData, emissorId: newValue?.id || null });
              }}
              renderInput={(params) => (
                <TextField {...params} label="Emissor" />
              )}
              fullWidth
            />
            <TextField
              label="AlphaTools ID"
              value={formData.alphaToolsId}
              onChange={(e) => setFormData({ ...formData, alphaToolsId: e.target.value })}
              fullWidth
            />
            <TextField
              label="Data de Vencimento"
              type="date"
              value={formData.dataVencimento}
              onChange={(e) => setFormData({ ...formData, dataVencimento: e.target.value })}
              InputLabelProps={{ shrink: true }}
              fullWidth
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>Cancelar</Button>
          <Button
            onClick={handleSave}
            variant="contained"
            disabled={!formData.codAtivo || createMutation.isPending || updateMutation.isPending}
          >
            {editingId ? 'Salvar' : 'Criar'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
