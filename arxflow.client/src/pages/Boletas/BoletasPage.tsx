import { useState, useMemo } from 'react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Checkbox } from '@/components/ui/checkbox';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Combobox } from '@/components/ui/combobox';
import { DatePicker } from '@/components/ui/date-picker';
import { Badge } from '@/components/ui/badge';
import { Copy, Pencil, X, Check, Trash2, ArrowUp, ArrowDown, ArrowUpDown } from 'lucide-react';
import { useQueryClient } from '@tanstack/react-query';
import {
  useGetBoletas,
  useCreateBoleta,
  useUpdateBoleta,
  useDeleteBoleta,
  getGetBoletasQueryKey,
} from '../../api/generated/boletas/boletas';
import { useGetAtivos } from '../../api/generated/ativos/ativos';
import { useGetContrapartes } from '../../api/generated/contrapartes/contrapartes';

// Tipos
interface Ativo {
  id?: number;
  codAtivo?: string;
}

interface Contraparte {
  id?: number;
  nome?: string;
}

interface Boleta {
  id: number;
  criadoEm?: string;
  ticker?: string;
  ativoId?: number;
  tipoOperacao?: string;
  volume?: number;
  quantidade?: number;
  tipoPrecificacao?: string;
  ntnbReferencia?: string;
  spreadValor?: number;
  taxaNominal?: number;
  dataFixing?: string;
  pu?: number;
  contraparteId?: number;
  contraparteNome?: string;
  alocacao?: string;
  usuario?: string;
  dataLiquidacao?: string;
  status?: string;
  observacao?: string;
}

// Opções de NTNB
const ntnbOpcoes = ['B26', 'B27', 'B28', 'B29', 'B30', 'B32', 'B33', 'B35', 'B40', 'B45', 'B50', 'B55', 'B60'];

// Opções de Status com cores
const statusOpcoes = [
  { value: 'AguardandoFixing', label: 'Aguardando Fixing', color: 'bg-orange-500 text-white' },
  { value: 'AguardandoBoletagem', label: 'Aguardando Boletagem', color: 'bg-gray-400 text-white' },
  { value: 'Boletada', label: 'Boletada', color: 'bg-yellow-500 text-white' },
  { value: 'Liquidada', label: 'Liquidada', color: 'bg-green-600 text-white' },
];

// Estado inicial para nova boleta
const getInitialNewBoleta = () => ({
  ativoId: null as number | null,
  ticker: '',
  tipoOperacao: 'C',
  volume: '',
  quantidade: '',
  tipoPrecificacao: 'Nominal',
  ntnbReferencia: '',
  spreadValor: '',
  taxaNominal: '',
  dataFixing: '',
  pu: '',
  contraparteId: null as number | null,
  alocacao: '',
  usuario: '',
  dataLiquidacao: '',
  status: 'AguardandoFixing',
  observacao: '',
});

export default function BoletasPage() {
  const queryClient = useQueryClient();

  // Dados
  const { data: boletas = [], isLoading } = useGetBoletas();
  const { data: ativos = [] } = useGetAtivos();
  const { data: contrapartes = [] } = useGetContrapartes();
  const createMutation = useCreateBoleta();
  const updateMutation = useUpdateBoleta();
  const deleteMutation = useDeleteBoleta();

  // Estado de seleção
  const [selecionadas, setSelecionadas] = useState<Set<number>>(new Set());

  // Estado de nova boleta
  const [novaBoleta, setNovaBoleta] = useState(getInitialNewBoleta());

  // Estado de edição
  const [editandoId, setEditandoId] = useState<number | null>(null);
  const [editBoleta, setEditBoleta] = useState(getInitialNewBoleta());

  // Estado do dialog de exclusão
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [boletaParaExcluir, setBoletaParaExcluir] = useState<Boleta | null>(null);

  // Estado de ordenação
  const [sortColumn, setSortColumn] = useState<keyof Boleta | null>(null);
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('asc');

  // Opções para comboboxes
  const ativoOptions = useMemo(
    () => (ativos as Ativo[]).map((a) => ({ value: a.id!, label: a.codAtivo || '' })),
    [ativos]
  );
  const contraparteOptions = useMemo(
    () => (contrapartes as Contraparte[]).map((c) => ({ value: c.id!, label: c.nome || '' })),
    [contrapartes]
  );

  // Boletas ordenadas
  const boletasOrdenadas = useMemo(() => {
    const data = [...(boletas as Boleta[])];
    if (!sortColumn) return data;

    return data.sort((a, b) => {
      const aVal = a[sortColumn];
      const bVal = b[sortColumn];

      if (aVal === undefined || aVal === null) return sortDirection === 'asc' ? 1 : -1;
      if (bVal === undefined || bVal === null) return sortDirection === 'asc' ? -1 : 1;

      if (typeof aVal === 'string' && typeof bVal === 'string') {
        return sortDirection === 'asc' ? aVal.localeCompare(bVal) : bVal.localeCompare(aVal);
      }

      if (typeof aVal === 'number' && typeof bVal === 'number') {
        return sortDirection === 'asc' ? aVal - bVal : bVal - aVal;
      }

      return 0;
    });
  }, [boletas, sortColumn, sortDirection]);

  // Handler de ordenação
  const handleSort = (column: keyof Boleta) => {
    if (sortColumn === column) {
      setSortDirection(sortDirection === 'asc' ? 'desc' : 'asc');
    } else {
      setSortColumn(column);
      setSortDirection('asc');
    }
  };

  // Componente de cabeçalho ordenável
  const SortableHeader = ({ column, children }: { column: keyof Boleta; children: React.ReactNode }) => (
    <div
      className="flex items-center gap-1 cursor-pointer select-none hover:text-primary"
      onClick={() => handleSort(column)}
    >
      {children}
      {sortColumn === column ? (
        sortDirection === 'asc' ? <ArrowUp className="h-3 w-3" /> : <ArrowDown className="h-3 w-3" />
      ) : (
        <ArrowUpDown className="h-3 w-3 opacity-30" />
      )}
    </div>
  );

  // Funções de seleção
  const toggleSelecao = (id: number) => {
    const novaSelecao = new Set(selecionadas);
    if (novaSelecao.has(id)) {
      novaSelecao.delete(id);
    } else {
      novaSelecao.add(id);
    }
    setSelecionadas(novaSelecao);
  };

  const toggleSelecionarTodas = () => {
    if (selecionadas.size === (boletas as Boleta[]).length) {
      setSelecionadas(new Set());
    } else {
      setSelecionadas(new Set((boletas as Boleta[]).map((b) => b.id)));
    }
  };

  // Copiar selecionadas para clipboard
  const copiarSelecionadas = async () => {
    const boletasSelecionadas = (boletas as Boleta[]).filter((b) => selecionadas.has(b.id));
    if (boletasSelecionadas.length === 0) return;

    const header = 'Data/Hora\tTicker\tOp\tVol (R$mm)\tQtd\tTipo\tNTNB\tSpread/Nominal\tData Fixing\tPU\tContraparte\tAlocação\tUsuário\tObservação';
    const rows = boletasSelecionadas.map((b) => {
      const preco = b.tipoPrecificacao === 'Spread' ? `${b.spreadValor ?? ''}bps` : b.taxaNominal?.toFixed(2) ?? '';
      const fixing = b.dataFixing ? new Date(b.dataFixing).toLocaleDateString('pt-BR') : '-';
      return `${b.criadoEm ? new Date(b.criadoEm).toLocaleString('pt-BR') : ''}\t${b.ticker ?? ''}\t${b.tipoOperacao ?? ''}\t${b.volume?.toFixed(2) ?? ''}\t${b.quantidade ?? ''}\t${b.tipoPrecificacao ?? ''}\t${b.ntnbReferencia ?? '-'}\t${preco}\t${fixing}\t${b.pu?.toFixed(2) ?? ''}\t${b.contraparteNome ?? '-'}\t${b.alocacao ?? ''}\t${b.usuario ?? ''}\t${b.observacao ?? ''}`;
    });

    await navigator.clipboard.writeText([header, ...rows].join('\n'));
    alert(`${boletasSelecionadas.length} boleta(s) copiada(s)!`);
  };

  // Salvar nova boleta
  const salvarNovaBoleta = async () => {
    const ativo = (ativos as Ativo[]).find((a) => a.id === novaBoleta.ativoId);

    await createMutation.mutateAsync({
      data: {
        ativoId: novaBoleta.ativoId ?? undefined,
        ticker: ativo?.codAtivo || novaBoleta.ticker,
        tipoOperacao: novaBoleta.tipoOperacao,
        volume: parseFloat(novaBoleta.volume.replace(',', '.')) || 0,
        quantidade: parseFloat(novaBoleta.quantidade.replace(',', '.')) || 0,
        tipoPrecificacao: novaBoleta.tipoPrecificacao,
        ntnbReferencia: novaBoleta.tipoPrecificacao === 'Spread' ? novaBoleta.ntnbReferencia || undefined : undefined,
        spreadValor: novaBoleta.tipoPrecificacao === 'Spread' ? parseFloat(novaBoleta.spreadValor.replace(',', '.')) || undefined : undefined,
        taxaNominal: novaBoleta.tipoPrecificacao === 'Nominal' ? parseFloat(novaBoleta.taxaNominal.replace(',', '.')) || undefined : undefined,
        dataFixing: novaBoleta.tipoPrecificacao === 'Spread' && novaBoleta.dataFixing ? new Date(novaBoleta.dataFixing).toISOString() : undefined,
        pu: parseFloat(novaBoleta.pu.replace(',', '.')) || undefined,
        contraparteId: novaBoleta.contraparteId ?? undefined,
        alocacao: novaBoleta.alocacao || undefined,
        usuario: novaBoleta.usuario || undefined,
        dataLiquidacao: novaBoleta.dataLiquidacao ? new Date(novaBoleta.dataLiquidacao).toISOString() : undefined,
        status: novaBoleta.status,
        observacao: novaBoleta.observacao || undefined,
      },
    });

    queryClient.invalidateQueries({ queryKey: getGetBoletasQueryKey() });
    setNovaBoleta(getInitialNewBoleta());
  };

  // Iniciar edição
  const iniciarEdicao = (boleta: Boleta) => {
    setEditandoId(boleta.id);
    setEditBoleta({
      ativoId: boleta.ativoId ?? null,
      ticker: boleta.ticker || '',
      tipoOperacao: boleta.tipoOperacao || 'C',
      volume: boleta.volume?.toString().replace('.', ',') || '',
      quantidade: boleta.quantidade?.toString() || '',
      tipoPrecificacao: boleta.tipoPrecificacao || 'Nominal',
      ntnbReferencia: boleta.ntnbReferencia || '',
      spreadValor: boleta.spreadValor?.toString() || '',
      taxaNominal: boleta.taxaNominal?.toString().replace('.', ',') || '',
      dataFixing: boleta.dataFixing ? boleta.dataFixing.split('T')[0] : '',
      pu: boleta.pu?.toString().replace('.', ',') || '',
      contraparteId: boleta.contraparteId ?? null,
      alocacao: boleta.alocacao || '',
      usuario: boleta.usuario || '',
      dataLiquidacao: boleta.dataLiquidacao ? boleta.dataLiquidacao.split('T')[0] : '',
      status: boleta.status || 'AguardandoFixing',
      observacao: boleta.observacao || '',
    });
  };

  // Salvar edição
  const salvarEdicao = async () => {
    if (!editandoId) return;

    const ativo = (ativos as Ativo[]).find((a) => a.id === editBoleta.ativoId);

    await updateMutation.mutateAsync({
      id: editandoId,
      data: {
        ativoId: editBoleta.ativoId ?? undefined,
        ticker: ativo?.codAtivo || editBoleta.ticker,
        tipoOperacao: editBoleta.tipoOperacao,
        volume: parseFloat(editBoleta.volume.replace(',', '.')) || 0,
        quantidade: parseFloat(editBoleta.quantidade.replace(',', '.')) || 0,
        tipoPrecificacao: editBoleta.tipoPrecificacao,
        ntnbReferencia: editBoleta.tipoPrecificacao === 'Spread' ? editBoleta.ntnbReferencia || undefined : undefined,
        spreadValor: editBoleta.tipoPrecificacao === 'Spread' ? parseFloat(editBoleta.spreadValor.replace(',', '.')) || undefined : undefined,
        taxaNominal: editBoleta.tipoPrecificacao === 'Nominal' ? parseFloat(editBoleta.taxaNominal.replace(',', '.')) || undefined : undefined,
        dataFixing: editBoleta.tipoPrecificacao === 'Spread' && editBoleta.dataFixing ? new Date(editBoleta.dataFixing).toISOString() : undefined,
        pu: parseFloat(editBoleta.pu.replace(',', '.')) || undefined,
        contraparteId: editBoleta.contraparteId ?? undefined,
        alocacao: editBoleta.alocacao || undefined,
        usuario: editBoleta.usuario || undefined,
        dataLiquidacao: editBoleta.dataLiquidacao ? new Date(editBoleta.dataLiquidacao).toISOString() : undefined,
        status: editBoleta.status,
        observacao: editBoleta.observacao || undefined,
      },
    });

    queryClient.invalidateQueries({ queryKey: getGetBoletasQueryKey() });
    setEditandoId(null);
  };

  // Cancelar edição
  const cancelarEdicao = () => {
    setEditandoId(null);
  };

  // Abrir dialog de exclusão
  const abrirDialogExclusao = (boleta: Boleta) => {
    setBoletaParaExcluir(boleta);
    setDeleteDialogOpen(true);
  };

  // Confirmar exclusão
  const confirmarExclusao = async () => {
    if (!boletaParaExcluir) return;

    await deleteMutation.mutateAsync({ id: boletaParaExcluir.id });
    queryClient.invalidateQueries({ queryKey: getGetBoletasQueryKey() });
    selecionadas.delete(boletaParaExcluir.id);
    setSelecionadas(new Set(selecionadas));
    setDeleteDialogOpen(false);
    setBoletaParaExcluir(null);
  };

  if (isLoading) {
    return <div className="p-6">Carregando...</div>;
  }

  return (
    <div>
      {/* Cabeçalho */}
      <div className="flex justify-between items-center mb-4">
        <h1 className="text-2xl font-bold">Boletas Registradas</h1>
        <div className="flex items-center gap-2">
          {selecionadas.size > 0 && (
            <Button onClick={copiarSelecionadas}>
              <Copy className="h-4 w-4 mr-2" />
              Copiar {selecionadas.size} selecionada(s)
            </Button>
          )}
          <span className="bg-primary text-primary-foreground px-3 py-1 rounded-full text-sm">
            {(boletas as Boleta[]).length} registros
          </span>
        </div>
      </div>

      {/* Tabela */}
      <div className="border rounded-lg">
        <Table className="table-fixed w-full text-xs">
          <TableHeader>
            <TableRow>
              <TableHead className="w-8 px-1">
                <Checkbox
                  checked={selecionadas.size === (boletas as Boleta[]).length && (boletas as Boleta[]).length > 0}
                  onCheckedChange={toggleSelecionarTodas}
                />
              </TableHead>
              <TableHead className="w-24 px-1"><SortableHeader column="criadoEm">Data</SortableHeader></TableHead>
              <TableHead className="w-[70px] px-1"><SortableHeader column="ticker">Ticker</SortableHeader></TableHead>
              <TableHead className="w-10 px-1"><SortableHeader column="tipoOperacao">Op</SortableHeader></TableHead>
              <TableHead className="w-16 px-1"><SortableHeader column="volume">Vol</SortableHeader></TableHead>
              <TableHead className="w-14 px-1"><SortableHeader column="quantidade">Qtd</SortableHeader></TableHead>
              <TableHead className="w-16 px-1"><SortableHeader column="tipoPrecificacao">Tipo</SortableHeader></TableHead>
              <TableHead className="w-12 px-1"><SortableHeader column="ntnbReferencia">NTNB</SortableHeader></TableHead>
              <TableHead className="w-16 px-1">Taxa</TableHead>
              <TableHead className="w-24 px-1"><SortableHeader column="dataFixing">Fixing</SortableHeader></TableHead>
              <TableHead className="w-20 px-1"><SortableHeader column="pu">PU</SortableHeader></TableHead>
              <TableHead className="w-20 px-1"><SortableHeader column="contraparteNome">Contrap.</SortableHeader></TableHead>
              <TableHead className="w-16 px-1"><SortableHeader column="alocacao">Aloc.</SortableHeader></TableHead>
              <TableHead className="w-14 px-1"><SortableHeader column="usuario">User</SortableHeader></TableHead>
              <TableHead className="w-24 px-1"><SortableHeader column="dataLiquidacao">Liq.</SortableHeader></TableHead>
              <TableHead className="w-24 px-1"><SortableHeader column="status">Status</SortableHeader></TableHead>
              <TableHead className="w-20 px-1">Obs</TableHead>
              <TableHead className="w-16 px-1">Ações</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {/* Linha de nova boleta */}
            <TableRow className="bg-primary/5">
              <TableCell className="px-1"></TableCell>
              <TableCell className="px-1 text-muted-foreground">Auto</TableCell>
              <TableCell className="px-1">
                <Combobox
                  options={ativoOptions}
                  value={novaBoleta.ativoId}
                  onChange={(v) => setNovaBoleta({ ...novaBoleta, ativoId: v as number | null })}
                  placeholder="Ativo"
                  className="h-7 text-xs"
                />
              </TableCell>
              <TableCell className="px-1">
                <Select value={novaBoleta.tipoOperacao} onValueChange={(v) => setNovaBoleta({ ...novaBoleta, tipoOperacao: v })}>
                  <SelectTrigger className="h-7 w-12 text-xs"><SelectValue /></SelectTrigger>
                  <SelectContent>
                    <SelectItem value="C">C</SelectItem>
                    <SelectItem value="V">V</SelectItem>
                  </SelectContent>
                </Select>
              </TableCell>
              <TableCell className="px-1">
                <Input className="h-7 text-xs" placeholder="1,5" value={novaBoleta.volume} onChange={(e) => setNovaBoleta({ ...novaBoleta, volume: e.target.value })} />
              </TableCell>
              <TableCell className="px-1">
                <Input className="h-7 text-xs" placeholder="1000" value={novaBoleta.quantidade} onChange={(e) => setNovaBoleta({ ...novaBoleta, quantidade: e.target.value })} />
              </TableCell>
              <TableCell className="px-1">
                <Select value={novaBoleta.tipoPrecificacao} onValueChange={(v) => setNovaBoleta({ ...novaBoleta, tipoPrecificacao: v })}>
                  <SelectTrigger className="h-7 text-xs"><SelectValue /></SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Nominal">Nominal</SelectItem>
                    <SelectItem value="Spread">Spread</SelectItem>
                  </SelectContent>
                </Select>
              </TableCell>
              <TableCell className="px-1">
                {novaBoleta.tipoPrecificacao === 'Spread' ? (
                  <Select value={novaBoleta.ntnbReferencia} onValueChange={(v) => setNovaBoleta({ ...novaBoleta, ntnbReferencia: v })}>
                    <SelectTrigger className="h-7 text-xs"><SelectValue placeholder="-" /></SelectTrigger>
                    <SelectContent>
                      {ntnbOpcoes.map((n) => <SelectItem key={n} value={n}>{n}</SelectItem>)}
                    </SelectContent>
                  </Select>
                ) : <span className="text-muted-foreground">-</span>}
              </TableCell>
              <TableCell className="px-1">
                {novaBoleta.tipoPrecificacao === 'Spread' ? (
                  <Input className="h-7 text-xs" placeholder="50" value={novaBoleta.spreadValor} onChange={(e) => setNovaBoleta({ ...novaBoleta, spreadValor: e.target.value })} />
                ) : (
                  <Input className="h-7 text-xs" placeholder="98,50" value={novaBoleta.taxaNominal} onChange={(e) => setNovaBoleta({ ...novaBoleta, taxaNominal: e.target.value })} />
                )}
              </TableCell>
              <TableCell className="px-1">
                {novaBoleta.tipoPrecificacao === 'Spread' ? (
                  <DatePicker value={novaBoleta.dataFixing} onChange={(v) => setNovaBoleta({ ...novaBoleta, dataFixing: v })} placeholder="Fixing" className="h-7 text-xs" />
                ) : <span className="text-muted-foreground">-</span>}
              </TableCell>
              <TableCell className="px-1">
                <Input className="h-7 text-xs" placeholder="1000,00" value={novaBoleta.pu} onChange={(e) => setNovaBoleta({ ...novaBoleta, pu: e.target.value })} />
              </TableCell>
              <TableCell className="px-1">
                <Combobox
                  options={contraparteOptions}
                  value={novaBoleta.contraparteId}
                  onChange={(v) => setNovaBoleta({ ...novaBoleta, contraparteId: v as number | null })}
                  placeholder="-"
                  className="h-7 text-xs"
                />
              </TableCell>
              <TableCell className="px-1">
                <Input className="h-7 text-xs" placeholder="Fundo" value={novaBoleta.alocacao} onChange={(e) => setNovaBoleta({ ...novaBoleta, alocacao: e.target.value })} />
              </TableCell>
              <TableCell className="px-1">
                <Input className="h-7 text-xs" placeholder="Nome" value={novaBoleta.usuario} onChange={(e) => setNovaBoleta({ ...novaBoleta, usuario: e.target.value })} />
              </TableCell>
              <TableCell className="px-1">
                <DatePicker value={novaBoleta.dataLiquidacao} onChange={(v) => setNovaBoleta({ ...novaBoleta, dataLiquidacao: v })} placeholder="Liquidacao" className="h-7 text-xs" />
              </TableCell>
              <TableCell className="px-1">
                <Select value={novaBoleta.status} onValueChange={(v) => setNovaBoleta({ ...novaBoleta, status: v })}>
                  <SelectTrigger className="h-7 text-xs"><SelectValue /></SelectTrigger>
                  <SelectContent>
                    {statusOpcoes.map((s) => <SelectItem key={s.value} value={s.value}>{s.label}</SelectItem>)}
                  </SelectContent>
                </Select>
              </TableCell>
              <TableCell className="px-1">
                <Input className="h-7 text-xs" placeholder="Obs" value={novaBoleta.observacao} onChange={(e) => setNovaBoleta({ ...novaBoleta, observacao: e.target.value })} />
              </TableCell>
              <TableCell className="px-1">
                <Button size="sm" onClick={salvarNovaBoleta} disabled={createMutation.isPending}>
                  Salvar
                </Button>
              </TableCell>
            </TableRow>

            {/* Linhas das boletas existentes */}
            {boletasOrdenadas.map((boleta) => {
              const isEditando = editandoId === boleta.id;
              const isSelecionada = selecionadas.has(boleta.id);
              const data = isEditando ? editBoleta : null;

              return (
                <TableRow key={boleta.id} className={`${isEditando ? 'bg-primary/5' : ''} ${isSelecionada ? 'bg-primary/10' : ''}`}>
                  <TableCell className="px-1">
                    <Checkbox checked={isSelecionada} onCheckedChange={() => toggleSelecao(boleta.id)} />
                  </TableCell>
                  <TableCell className="px-1">
                    {boleta.criadoEm ? new Date(boleta.criadoEm).toLocaleString('pt-BR', { day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit' }) : ''}
                  </TableCell>
                  <TableCell className="px-1">
                    {isEditando ? (
                      <Combobox options={ativoOptions} value={data!.ativoId} onChange={(v) => setEditBoleta({ ...data!, ativoId: v as number | null })} placeholder="Ativo" className="h-7 text-xs" />
                    ) : boleta.ticker}
                  </TableCell>
                  <TableCell className="px-1">
                    {isEditando ? (
                      <Select value={data!.tipoOperacao} onValueChange={(v) => setEditBoleta({ ...data!, tipoOperacao: v })}>
                        <SelectTrigger className="h-7 w-12 text-xs"><SelectValue /></SelectTrigger>
                        <SelectContent>
                          <SelectItem value="C">C</SelectItem>
                          <SelectItem value="V">V</SelectItem>
                        </SelectContent>
                      </Select>
                    ) : (
                      <span className={`px-1.5 py-0.5 rounded text-xs font-medium ${boleta.tipoOperacao === 'C' ? 'bg-green-600 text-white' : 'bg-red-600 text-white'}`}>
                        {boleta.tipoOperacao}
                      </span>
                    )}
                  </TableCell>
                  <TableCell className="px-1">
                    {isEditando ? (
                      <Input className="h-7 text-xs" value={data!.volume} onChange={(e) => setEditBoleta({ ...data!, volume: e.target.value })} />
                    ) : boleta.volume?.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}
                  </TableCell>
                  <TableCell className="px-1">
                    {isEditando ? (
                      <Input className="h-7 text-xs" value={data!.quantidade} onChange={(e) => setEditBoleta({ ...data!, quantidade: e.target.value })} />
                    ) : boleta.quantidade?.toLocaleString('pt-BR')}
                  </TableCell>
                  <TableCell className="px-1">
                    {isEditando ? (
                      <Select value={data!.tipoPrecificacao} onValueChange={(v) => setEditBoleta({ ...data!, tipoPrecificacao: v })}>
                        <SelectTrigger className="h-7 text-xs"><SelectValue /></SelectTrigger>
                        <SelectContent>
                          <SelectItem value="Nominal">Nominal</SelectItem>
                          <SelectItem value="Spread">Spread</SelectItem>
                        </SelectContent>
                      </Select>
                    ) : boleta.tipoPrecificacao === 'Spread' ? 'Spread' : 'Nominal'}
                  </TableCell>
                  <TableCell className="px-1">
                    {isEditando && data!.tipoPrecificacao === 'Spread' ? (
                      <Select value={data!.ntnbReferencia} onValueChange={(v) => setEditBoleta({ ...data!, ntnbReferencia: v })}>
                        <SelectTrigger className="h-7 text-xs"><SelectValue placeholder="-" /></SelectTrigger>
                        <SelectContent>
                          {ntnbOpcoes.map((n) => <SelectItem key={n} value={n}>{n}</SelectItem>)}
                        </SelectContent>
                      </Select>
                    ) : boleta.ntnbReferencia || '-'}
                  </TableCell>
                  <TableCell className="px-1">
                    {isEditando ? (
                      data!.tipoPrecificacao === 'Spread' ? (
                        <Input className="h-7 text-xs" value={data!.spreadValor} onChange={(e) => setEditBoleta({ ...data!, spreadValor: e.target.value })} />
                      ) : (
                        <Input className="h-7 text-xs" value={data!.taxaNominal} onChange={(e) => setEditBoleta({ ...data!, taxaNominal: e.target.value })} />
                      )
                    ) : boleta.tipoPrecificacao === 'Spread' ? `${boleta.spreadValor ?? ''}bps` : boleta.taxaNominal?.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}
                  </TableCell>
                  <TableCell className="px-1">
                    {isEditando && data!.tipoPrecificacao === 'Spread' ? (
                      <DatePicker value={data!.dataFixing} onChange={(v) => setEditBoleta({ ...data!, dataFixing: v })} placeholder="Fixing" className="h-7 text-xs" />
                    ) : boleta.dataFixing ? new Date(boleta.dataFixing).toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit' }) : '-'}
                  </TableCell>
                  <TableCell className="px-1">
                    {isEditando ? (
                      <Input className="h-7 text-xs" value={data!.pu} onChange={(e) => setEditBoleta({ ...data!, pu: e.target.value })} />
                    ) : boleta.pu?.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}
                  </TableCell>
                  <TableCell className="px-1">
                    {isEditando ? (
                      <Combobox options={contraparteOptions} value={data!.contraparteId} onChange={(v) => setEditBoleta({ ...data!, contraparteId: v as number | null })} placeholder="-" className="h-7 text-xs" />
                    ) : boleta.contraparteNome || '-'}
                  </TableCell>
                  <TableCell className="px-1">
                    {isEditando ? (
                      <Input className="h-7 text-xs" value={data!.alocacao} onChange={(e) => setEditBoleta({ ...data!, alocacao: e.target.value })} />
                    ) : boleta.alocacao}
                  </TableCell>
                  <TableCell className="px-1">
                    {isEditando ? (
                      <Input className="h-7 text-xs" value={data!.usuario} onChange={(e) => setEditBoleta({ ...data!, usuario: e.target.value })} />
                    ) : boleta.usuario}
                  </TableCell>
                  <TableCell className="px-1">
                    {isEditando ? (
                      <DatePicker value={data!.dataLiquidacao} onChange={(v) => setEditBoleta({ ...data!, dataLiquidacao: v })} placeholder="Liquidacao" className="h-7 text-xs" />
                    ) : boleta.dataLiquidacao ? new Date(boleta.dataLiquidacao).toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit' }) : '-'}
                  </TableCell>
                  <TableCell className="px-1">
                    {isEditando ? (
                      <Select value={data!.status} onValueChange={(v) => setEditBoleta({ ...data!, status: v })}>
                        <SelectTrigger className="h-7 text-xs"><SelectValue /></SelectTrigger>
                        <SelectContent>
                          {statusOpcoes.map((s) => <SelectItem key={s.value} value={s.value}>{s.label}</SelectItem>)}
                        </SelectContent>
                      </Select>
                    ) : (() => {
                      const statusInfo = statusOpcoes.find((s) => s.value === boleta.status);
                      return statusInfo ? (
                        <Badge className={`whitespace-nowrap ${statusInfo.color}`}>{statusInfo.label}</Badge>
                      ) : boleta.status;
                    })()}
                  </TableCell>
                  <TableCell className="px-1">
                    {isEditando ? (
                      <Input className="h-7 text-xs" value={data!.observacao} onChange={(e) => setEditBoleta({ ...data!, observacao: e.target.value })} />
                    ) : boleta.observacao}
                  </TableCell>
                  <TableCell className="px-1">
                    {isEditando ? (
                      <div className="flex gap-1">
                        <Button size="sm" variant="default" onClick={salvarEdicao} disabled={updateMutation.isPending}>
                          <Check className="h-4 w-4" />
                        </Button>
                        <Button size="sm" variant="outline" onClick={cancelarEdicao}>
                          <X className="h-4 w-4" />
                        </Button>
                      </div>
                    ) : (
                      <div className="flex gap-1">
                        <Button size="sm" variant="ghost" onClick={() => iniciarEdicao(boleta)}>
                          <Pencil className="h-4 w-4" />
                        </Button>
                        <Button size="sm" variant="ghost" onClick={() => abrirDialogExclusao(boleta)}>
                          <Trash2 className="h-4 w-4" />
                        </Button>
                      </div>
                    )}
                  </TableCell>
                </TableRow>
              );
            })}
          </TableBody>
        </Table>
      </div>

      {/* Dialog de confirmação de exclusão */}
      <Dialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Confirmar Exclusão</DialogTitle>
            <DialogDescription>
              Deseja realmente excluir a boleta{' '}
              <strong>{boletaParaExcluir?.ticker}</strong>
              {boletaParaExcluir?.criadoEm && (
                <> de {new Date(boletaParaExcluir.criadoEm).toLocaleString('pt-BR', { day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit' })}</>
              )}
              ? Esta ação não pode ser desfeita.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDeleteDialogOpen(false)}>
              Cancelar
            </Button>
            <Button variant="destructive" onClick={confirmarExclusao} disabled={deleteMutation.isPending}>
              Excluir
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
