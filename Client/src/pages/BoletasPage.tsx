import React, { useState, useMemo } from 'react';
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
import {
  DropdownMenu,
  DropdownMenuCheckboxItem,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { Combobox } from '@/components/ui/combobox';
import { DatePicker } from '@/components/ui/date-picker';
import { Badge } from '@/components/ui/badge';
import { Tooltip, TooltipContent, TooltipTrigger } from '@/components/ui/tooltip';
import { Copy, Pencil, X, Check, Trash2, ArrowUp, ArrowDown, ArrowUpDown, Link2, Unlink2, ChevronDown, ChevronRight, Settings2 } from 'lucide-react';
import { useQueryClient } from '@tanstack/react-query';
import {
  useGetBoletas,
  useCreateBoleta,
  useUpdateBoleta,
  useDeleteBoleta,
  getGetBoletasQueryKey,
} from '../api/generated/boletas/boletas';
import { useGetAtivos } from '../api/generated/ativos/ativos';
import { useGetContrapartes } from '../api/generated/contrapartes/contrapartes';

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
  boletaPrincipalId?: number | null;
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
  { value: 'AguardandoFixing', label: 'Aguardando Fixing', color: 'bg-blue-500 text-white' },
  { value: 'AguardandoBoletagem', label: 'Aguardando Boletagem', color: 'bg-gray-400 text-white' },
  { value: 'Boletada', label: 'Boletada', color: 'bg-yellow-500 text-white' },
  { value: 'Liquidada', label: 'Liquidada', color: 'bg-green-600 text-white' },
];

// Definicao das colunas da tabela
const colunasDisponiveis = [
  { id: 'data', label: 'Data', padrao: true },
  { id: 'observacao', label: 'Observação', padrao: true },
  { id: 'ticker', label: 'Ticker', padrao: true },
  { id: 'operacao', label: 'C/V', padrao: true },
  { id: 'status', label: 'Status', padrao: true },
  { id: 'volume', label: 'Volume', padrao: true },
  { id: 'quantidade', label: 'Quantidade', padrao: true },
  { id: 'tipo', label: 'Tipo', padrao: true },
  { id: 'ntnb', label: 'NTNB', padrao: true },
  { id: 'taxa', label: 'Taxa', padrao: true },
  { id: 'dataFixing', label: 'Data Fixing', padrao: true },
  { id: 'pu', label: 'PU', padrao: true },
  { id: 'contraparte', label: 'Contraparte', padrao: true },
  { id: 'alocacao', label: 'Alocação', padrao: false },
  { id: 'usuario', label: 'Usuário', padrao: false },
  { id: 'liquidacao', label: 'Liquidação', padrao: true },
] as const;

type ColunaId = typeof colunasDisponiveis[number]['id'];

// Estado inicial para nova boleta
const getInitialNewBoleta = () => ({
  boletaPrincipalId: null as number | null,
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

  // Estado de boletas expandidas (mostrando as ligadas)
  const [expandidas, setExpandidas] = useState<Set<number>>(new Set());

  // Funcao para alternar expansao de uma boleta principal
  const toggleExpandir = (id: number) => {
    const novasExpandidas = new Set(expandidas);
    if (novasExpandidas.has(id)) {
      novasExpandidas.delete(id);
    } else {
      novasExpandidas.add(id);
    }
    setExpandidas(novasExpandidas);
  };

  // Estado de ordenação
  const [sortColumn, setSortColumn] = useState<keyof Boleta | null>(null);
  const [sortDirection, setSortDirection] = useState<'asc' | 'desc'>('asc');

  // Estado de colunas visiveis
  const [colunasVisiveis, setColunasVisiveis] = useState<Set<ColunaId>>(
    new Set(colunasDisponiveis.filter(c => c.padrao).map(c => c.id))
  );

  const toggleColuna = (colunaId: ColunaId) => {
    const novasVisiveis = new Set(colunasVisiveis);
    if (novasVisiveis.has(colunaId)) {
      novasVisiveis.delete(colunaId);
    } else {
      novasVisiveis.add(colunaId);
    }
    setColunasVisiveis(novasVisiveis);
  };

  const colunaVisivel = (colunaId: ColunaId) => colunasVisiveis.has(colunaId);

  // Opções para comboboxes
  const ativoOptions = useMemo(
    () => (ativos as Ativo[]).map((a) => ({ value: a.id!, label: a.codAtivo || '' })),
    [ativos]
  );
  const contraparteOptions = useMemo(
    () => (contrapartes as Contraparte[]).map((c) => ({ value: c.id!, label: c.nome || '' })),
    [contrapartes]
  );

  // Boletas agrupadas (principais com suas ligadas)
  const boletasAgrupadas = useMemo(() => {
    const todasBoletas = boletas as Boleta[];

    // Separar boletas principais (sem boletaPrincipalId) e ligadas
    const principais = todasBoletas.filter(b => !b.boletaPrincipalId);
    const ligadasMap = new Map<number, Boleta[]>();

    // Agrupar ligadas por principal
    todasBoletas.filter(b => b.boletaPrincipalId).forEach(b => {
      const principalId = b.boletaPrincipalId!;
      if (!ligadasMap.has(principalId)) {
        ligadasMap.set(principalId, []);
      }
      ligadasMap.get(principalId)!.push(b);
    });

    // Ordenar principais
    const sortFn = (a: Boleta, b: Boleta) => {
      if (!sortColumn) return 0;
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
    };

    principais.sort(sortFn);

    // Retornar estrutura com informacoes de agrupamento
    return principais.map(principal => ({
      boleta: principal,
      ligadas: (ligadasMap.get(principal.id) || []).sort(sortFn),
      temLigadas: ligadasMap.has(principal.id) && ligadasMap.get(principal.id)!.length > 0
    }));
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

  // Criar boleta ligada (copia todos os dados e vincula a principal)
  const criarBoletaLigada = async (boletaPrincipal: Boleta) => {
    await createMutation.mutateAsync({
      data: {
        boletaPrincipalId: boletaPrincipal.id,
        ativoId: boletaPrincipal.ativoId ?? undefined,
        ticker: boletaPrincipal.ticker,
        tipoOperacao: boletaPrincipal.tipoOperacao || 'C',
        volume: boletaPrincipal.volume || 0,
        quantidade: boletaPrincipal.quantidade || 0,
        tipoPrecificacao: boletaPrincipal.tipoPrecificacao || 'Nominal',
        ntnbReferencia: boletaPrincipal.ntnbReferencia || undefined,
        spreadValor: boletaPrincipal.spreadValor ?? undefined,
        taxaNominal: boletaPrincipal.taxaNominal ?? undefined,
        dataFixing: boletaPrincipal.dataFixing || undefined,
        pu: boletaPrincipal.pu ?? undefined,
        contraparteId: boletaPrincipal.contraparteId ?? undefined,
        alocacao: boletaPrincipal.alocacao || undefined,
        usuario: boletaPrincipal.usuario || undefined,
        dataLiquidacao: boletaPrincipal.dataLiquidacao || undefined,
        status: boletaPrincipal.status || 'AguardandoFixing',
        observacao: boletaPrincipal.observacao || undefined,
      },
    });
    queryClient.invalidateQueries({ queryKey: getGetBoletasQueryKey() });
    // Expandir automaticamente a boleta principal para mostrar as ligadas
    if (!expandidas.has(boletaPrincipal.id)) {
      setExpandidas(new Set([...expandidas, boletaPrincipal.id]));
    }
  };

  // Desligar boleta (torna independente)
  const desligarBoleta = async (boleta: Boleta) => {
    await updateMutation.mutateAsync({
      id: boleta.id,
      data: {
        boletaPrincipalId: null,
        ativoId: boleta.ativoId ?? undefined,
        ticker: boleta.ticker,
        tipoOperacao: boleta.tipoOperacao || 'C',
        volume: boleta.volume || 0,
        quantidade: boleta.quantidade || 0,
        tipoPrecificacao: boleta.tipoPrecificacao || 'Nominal',
        ntnbReferencia: boleta.ntnbReferencia || undefined,
        spreadValor: boleta.spreadValor ?? undefined,
        taxaNominal: boleta.taxaNominal ?? undefined,
        dataFixing: boleta.dataFixing || undefined,
        pu: boleta.pu ?? undefined,
        contraparteId: boleta.contraparteId ?? undefined,
        alocacao: boleta.alocacao || undefined,
        usuario: boleta.usuario || undefined,
        dataLiquidacao: boleta.dataLiquidacao || undefined,
        status: boleta.status || 'AguardandoFixing',
        observacao: boleta.observacao || undefined,
      },
    });
    queryClient.invalidateQueries({ queryKey: getGetBoletasQueryKey() });
  };

  // Iniciar edição
  const iniciarEdicao = (boleta: Boleta) => {
    setEditandoId(boleta.id);
    setEditBoleta({
      boletaPrincipalId: boleta.boletaPrincipalId ?? null,
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
        boletaPrincipalId: editBoleta.boletaPrincipalId ?? undefined,
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
            <Tooltip>
              <TooltipTrigger asChild>
                <Button onClick={copiarSelecionadas}>
                  <Copy className="h-4 w-4 mr-2" />
                  Copiar {selecionadas.size} selecionada(s)
                </Button>
              </TooltipTrigger>
              <TooltipContent>Copiar boletas selecionadas para a area de transferencia</TooltipContent>
            </Tooltip>
          )}
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="outline" size="sm">
                <Settings2 className="h-4 w-4 mr-2" />
                Colunas
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end" className="w-48">
              <DropdownMenuLabel>Colunas visiveis</DropdownMenuLabel>
              <DropdownMenuSeparator />
              {colunasDisponiveis.map((coluna) => (
                <DropdownMenuCheckboxItem
                  key={coluna.id}
                  checked={colunaVisivel(coluna.id)}
                  onCheckedChange={() => toggleColuna(coluna.id)}
                >
                  {coluna.label}
                </DropdownMenuCheckboxItem>
              ))}
              <DropdownMenuSeparator />
              <DropdownMenuItem
                onClick={() => setColunasVisiveis(new Set(colunasDisponiveis.map(c => c.id)))}
              >
                Mostrar todas
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
          <span className="bg-primary text-primary-foreground px-3 py-1 rounded-full text-sm">
            {(boletas as Boleta[]).length} registros
          </span>
        </div>
      </div>

      {/* Tabela */}
      <div className="border rounded-lg overflow-auto max-h-[calc(100vh-180px)]">
        <Table className="min-w-[1800px] text-xs">
          <TableHeader className="sticky top-0 bg-background z-20">
            <TableRow>
              <TableHead className="w-8 px-1 sticky left-0 bg-background z-30">
                <Checkbox
                  checked={selecionadas.size === (boletas as Boleta[]).length && (boletas as Boleta[]).length > 0}
                  onCheckedChange={toggleSelecionarTodas}
                />
              </TableHead>
              <TableHead className="w-20 px-1 sticky left-8 bg-background z-30">Ações</TableHead>
              {colunaVisivel('data') && <TableHead className="w-28 px-1"><SortableHeader column="criadoEm">Data</SortableHeader></TableHead>}
              {colunaVisivel('observacao') && <TableHead className="w-32 px-1">Observação</TableHead>}
              {colunaVisivel('ticker') && <TableHead className="w-20 px-1"><SortableHeader column="ticker">Ticker</SortableHeader></TableHead>}
              {colunaVisivel('operacao') && <TableHead className="w-16 px-1"><SortableHeader column="tipoOperacao">C/V</SortableHeader></TableHead>}
              {colunaVisivel('status') && <TableHead className="w-32 px-1"><SortableHeader column="status">Status</SortableHeader></TableHead>}
              {colunaVisivel('volume') && <TableHead className="w-20 px-1"><SortableHeader column="volume">Volume</SortableHeader></TableHead>}
              {colunaVisivel('quantidade') && <TableHead className="w-20 px-1"><SortableHeader column="quantidade">Quantidade</SortableHeader></TableHead>}
              {colunaVisivel('tipo') && <TableHead className="w-20 px-1"><SortableHeader column="tipoPrecificacao">Tipo</SortableHeader></TableHead>}
              {colunaVisivel('ntnb') && <TableHead className="w-16 px-1"><SortableHeader column="ntnbReferencia">NTNB</SortableHeader></TableHead>}
              {colunaVisivel('taxa') && <TableHead className="w-20 px-1">Taxa</TableHead>}
              {colunaVisivel('dataFixing') && <TableHead className="w-28 px-1"><SortableHeader column="dataFixing">Data Fixing</SortableHeader></TableHead>}
              {colunaVisivel('pu') && <TableHead className="w-24 px-1"><SortableHeader column="pu">PU</SortableHeader></TableHead>}
              {colunaVisivel('contraparte') && <TableHead className="w-28 px-1"><SortableHeader column="contraparteNome">Contraparte</SortableHeader></TableHead>}
              {colunaVisivel('alocacao') && <TableHead className="w-24 px-1"><SortableHeader column="alocacao">Alocação</SortableHeader></TableHead>}
              {colunaVisivel('usuario') && <TableHead className="w-20 px-1"><SortableHeader column="usuario">Usuário</SortableHeader></TableHead>}
              {colunaVisivel('liquidacao') && <TableHead className="w-28 px-1"><SortableHeader column="dataLiquidacao">Liquidação</SortableHeader></TableHead>}
            </TableRow>
          </TableHeader>
          <TableBody>
            {/* Linha de nova boleta */}
            <TableRow className="bg-primary/5">
              <TableCell className="px-1 sticky left-0 bg-primary/5 z-10"></TableCell>
              <TableCell className="px-1 sticky left-8 bg-primary/5 z-10">
                <Tooltip>
                  <TooltipTrigger asChild>
                    <Button size="sm" onClick={salvarNovaBoleta} disabled={createMutation.isPending}>
                      <Check className="h-4 w-4" />
                    </Button>
                  </TooltipTrigger>
                  <TooltipContent>Salvar nova boleta</TooltipContent>
                </Tooltip>
              </TableCell>
              {colunaVisivel('data') && <TableCell className="px-1 text-muted-foreground">Auto</TableCell>}
              {colunaVisivel('observacao') && <TableCell className="px-1">
                <Input className="h-7 text-xs" placeholder="Observação" value={novaBoleta.observacao} onChange={(e) => setNovaBoleta({ ...novaBoleta, observacao: e.target.value })} />
              </TableCell>}
              {colunaVisivel('ticker') && <TableCell className="px-1">
                <Combobox
                  options={ativoOptions}
                  value={novaBoleta.ativoId}
                  onChange={(v) => setNovaBoleta({ ...novaBoleta, ativoId: v as number | null })}
                  placeholder="Ativo"
                  className="h-7 text-xs"
                />
              </TableCell>}
              {colunaVisivel('operacao') && <TableCell className="px-1">
                <Select value={novaBoleta.tipoOperacao} onValueChange={(v) => setNovaBoleta({ ...novaBoleta, tipoOperacao: v })}>
                  <SelectTrigger className="h-7 w-12 text-xs"><SelectValue /></SelectTrigger>
                  <SelectContent>
                    <SelectItem value="C">C</SelectItem>
                    <SelectItem value="V">V</SelectItem>
                  </SelectContent>
                </Select>
              </TableCell>}
              {colunaVisivel('status') && <TableCell className="px-1">
                <Select value={novaBoleta.status} onValueChange={(v) => setNovaBoleta({ ...novaBoleta, status: v })}>
                  <SelectTrigger className="h-7 text-xs"><SelectValue /></SelectTrigger>
                  <SelectContent>
                    {statusOpcoes.map((s) => <SelectItem key={s.value} value={s.value}>{s.label}</SelectItem>)}
                  </SelectContent>
                </Select>
              </TableCell>}
              {colunaVisivel('volume') && <TableCell className="px-1">
                <Input className="h-7 text-xs" placeholder="1,5" value={novaBoleta.volume} onChange={(e) => setNovaBoleta({ ...novaBoleta, volume: e.target.value })} />
              </TableCell>}
              {colunaVisivel('quantidade') && <TableCell className="px-1">
                <Input className="h-7 text-xs" placeholder="1000" value={novaBoleta.quantidade} onChange={(e) => setNovaBoleta({ ...novaBoleta, quantidade: e.target.value })} />
              </TableCell>}
              {colunaVisivel('tipo') && <TableCell className="px-1">
                <Select value={novaBoleta.tipoPrecificacao} onValueChange={(v) => setNovaBoleta({ ...novaBoleta, tipoPrecificacao: v })}>
                  <SelectTrigger className="h-7 text-xs"><SelectValue /></SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Nominal">Nominal</SelectItem>
                    <SelectItem value="Spread">Spread</SelectItem>
                  </SelectContent>
                </Select>
              </TableCell>}
              {colunaVisivel('ntnb') && <TableCell className="px-1">
                {novaBoleta.tipoPrecificacao === 'Spread' ? (
                  <Select value={novaBoleta.ntnbReferencia} onValueChange={(v) => setNovaBoleta({ ...novaBoleta, ntnbReferencia: v })}>
                    <SelectTrigger className="h-7 text-xs"><SelectValue placeholder="-" /></SelectTrigger>
                    <SelectContent>
                      {ntnbOpcoes.map((n) => <SelectItem key={n} value={n}>{n}</SelectItem>)}
                    </SelectContent>
                  </Select>
                ) : <span className="text-muted-foreground">-</span>}
              </TableCell>}
              {colunaVisivel('taxa') && <TableCell className="px-1">
                {novaBoleta.tipoPrecificacao === 'Spread' ? (
                  <Input className="h-7 text-xs" placeholder="50" value={novaBoleta.spreadValor} onChange={(e) => setNovaBoleta({ ...novaBoleta, spreadValor: e.target.value })} />
                ) : (
                  <Input className="h-7 text-xs" placeholder="98,50" value={novaBoleta.taxaNominal} onChange={(e) => setNovaBoleta({ ...novaBoleta, taxaNominal: e.target.value })} />
                )}
              </TableCell>}
              {colunaVisivel('dataFixing') && <TableCell className="px-1">
                {novaBoleta.tipoPrecificacao === 'Spread' ? (
                  <DatePicker value={novaBoleta.dataFixing} onChange={(v) => setNovaBoleta({ ...novaBoleta, dataFixing: v })} placeholder="Fixing" className="h-7 text-xs" />
                ) : <span className="text-muted-foreground">-</span>}
              </TableCell>}
              {colunaVisivel('pu') && <TableCell className="px-1">
                <Input className="h-7 text-xs" placeholder="1000,00" value={novaBoleta.pu} onChange={(e) => setNovaBoleta({ ...novaBoleta, pu: e.target.value })} />
              </TableCell>}
              {colunaVisivel('contraparte') && <TableCell className="px-1">
                <Combobox
                  options={contraparteOptions}
                  value={novaBoleta.contraparteId}
                  onChange={(v) => setNovaBoleta({ ...novaBoleta, contraparteId: v as number | null })}
                  placeholder="-"
                  className="h-7 text-xs"
                />
              </TableCell>}
              {colunaVisivel('alocacao') && <TableCell className="px-1">
                <Input className="h-7 text-xs" placeholder="Fundo" value={novaBoleta.alocacao} onChange={(e) => setNovaBoleta({ ...novaBoleta, alocacao: e.target.value })} />
              </TableCell>}
              {colunaVisivel('usuario') && <TableCell className="px-1">
                <Input className="h-7 text-xs" placeholder="Nome" value={novaBoleta.usuario} onChange={(e) => setNovaBoleta({ ...novaBoleta, usuario: e.target.value })} />
              </TableCell>}
              {colunaVisivel('liquidacao') && <TableCell className="px-1">
                <DatePicker value={novaBoleta.dataLiquidacao} onChange={(v) => setNovaBoleta({ ...novaBoleta, dataLiquidacao: v })} placeholder="Liquidacao" className="h-7 text-xs" />
              </TableCell>}
            </TableRow>

            {/* Linhas das boletas existentes (agrupadas) */}
            {boletasAgrupadas.map(({ boleta, ligadas, temLigadas }) => {
              const isExpandida = expandidas.has(boleta.id);

              // Funcao para renderizar uma linha de boleta
              const renderLinhaBoleta = (b: Boleta, isLigada: boolean = false) => {
                const isEditando = editandoId === b.id;
                const isSelecionada = selecionadas.has(b.id);
                const data = isEditando ? editBoleta : null;
                const bgClass = isEditando ? 'bg-primary/5' : isSelecionada ? 'bg-primary/10' : isLigada ? 'bg-blue-50' : 'bg-background';

                return (
                  <TableRow key={b.id} className={`${isEditando ? 'bg-primary/5' : ''} ${isSelecionada ? 'bg-primary/10' : ''} ${isLigada ? 'bg-blue-50' : ''}`}>
                    <TableCell className={`px-1 sticky left-0 z-10 ${bgClass}`}>
                      <div className="flex items-center gap-1">
                        {!isLigada && temLigadas && (
                          <Tooltip>
                            <TooltipTrigger asChild>
                              <Button size="sm" variant="ghost" className="h-5 w-5 p-0" onClick={() => toggleExpandir(boleta.id)}>
                                {isExpandida ? <ChevronDown className="h-3 w-3" /> : <ChevronRight className="h-3 w-3" />}
                              </Button>
                            </TooltipTrigger>
                            <TooltipContent>{isExpandida ? 'Ocultar boletas ligadas' : `Mostrar ${ligadas.length} boleta(s) ligada(s)`}</TooltipContent>
                          </Tooltip>
                        )}
                        {isLigada && <span className="w-5" />}
                        <Checkbox checked={isSelecionada} onCheckedChange={() => toggleSelecao(b.id)} />
                      </div>
                    </TableCell>
                    <TableCell className={`px-1 sticky left-8 z-10 ${bgClass}`}>
                      {isEditando ? (
                        <div className="flex gap-1">
                          <Tooltip>
                            <TooltipTrigger asChild>
                              <Button size="sm" variant="default" onClick={salvarEdicao} disabled={updateMutation.isPending}>
                                <Check className="h-4 w-4" />
                              </Button>
                            </TooltipTrigger>
                            <TooltipContent>Salvar alteracoes</TooltipContent>
                          </Tooltip>
                          <Tooltip>
                            <TooltipTrigger asChild>
                              <Button size="sm" variant="outline" onClick={cancelarEdicao}>
                                <X className="h-4 w-4" />
                              </Button>
                            </TooltipTrigger>
                            <TooltipContent>Cancelar edicao</TooltipContent>
                          </Tooltip>
                        </div>
                      ) : (
                        <div className="flex gap-1">
                          {isLigada ? (
                            <Tooltip>
                              <TooltipTrigger asChild>
                                <Button size="sm" variant="ghost" onClick={() => desligarBoleta(b)}>
                                  <Unlink2 className="h-4 w-4 text-blue-600" />
                                </Button>
                              </TooltipTrigger>
                              <TooltipContent>Tornar boleta independente</TooltipContent>
                            </Tooltip>
                          ) : (
                            <Tooltip>
                              <TooltipTrigger asChild>
                                <Button size="sm" variant="ghost" onClick={() => criarBoletaLigada(b)}>
                                  <Link2 className="h-4 w-4" />
                                </Button>
                              </TooltipTrigger>
                              <TooltipContent>Criar boleta ligada</TooltipContent>
                            </Tooltip>
                          )}
                          <Tooltip>
                            <TooltipTrigger asChild>
                              <Button size="sm" variant="ghost" onClick={() => iniciarEdicao(b)}>
                                <Pencil className="h-4 w-4" />
                              </Button>
                            </TooltipTrigger>
                            <TooltipContent>Editar boleta</TooltipContent>
                          </Tooltip>
                          <Tooltip>
                            <TooltipTrigger asChild>
                              <Button size="sm" variant="ghost" onClick={() => abrirDialogExclusao(b)}>
                                <Trash2 className="h-4 w-4" />
                              </Button>
                            </TooltipTrigger>
                            <TooltipContent>Excluir boleta</TooltipContent>
                          </Tooltip>
                        </div>
                      )}
                    </TableCell>
                    {colunaVisivel('data') && <TableCell className="px-1">
                      {b.criadoEm ? new Date(b.criadoEm).toLocaleString('pt-BR', { day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit' }) : ''}
                    </TableCell>}
                    {colunaVisivel('observacao') && <TableCell className={`px-1 ${!isEditando && !isLigada ? 'cursor-pointer hover:bg-muted/50' : ''}`} onClick={() => !isEditando && !isLigada && iniciarEdicao(b)}>
                      {isEditando && !isLigada ? (
                        <Input className="h-7 text-xs" value={data!.observacao} onChange={(e) => setEditBoleta({ ...data!, observacao: e.target.value })} />
                      ) : b.observacao}
                    </TableCell>}
                    {colunaVisivel('ticker') && <TableCell className={`px-1 ${!isEditando ? 'cursor-pointer hover:bg-muted/50' : ''}`} onClick={() => !isEditando && iniciarEdicao(b)}>
                      {isEditando ? (
                        <Combobox options={ativoOptions} value={data!.ativoId} onChange={(v) => setEditBoleta({ ...data!, ativoId: v as number | null })} placeholder="Ativo" className="h-7 text-xs" />
                      ) : b.ticker}
                    </TableCell>}
                    {colunaVisivel('operacao') && <TableCell className={`px-1 ${!isEditando && !isLigada ? 'cursor-pointer hover:bg-muted/50' : ''}`} onClick={() => !isEditando && !isLigada && iniciarEdicao(b)}>
                      {isEditando && !isLigada ? (
                        <Select value={data!.tipoOperacao} onValueChange={(v) => setEditBoleta({ ...data!, tipoOperacao: v })}>
                          <SelectTrigger className="h-7 w-12 text-xs"><SelectValue /></SelectTrigger>
                          <SelectContent>
                            <SelectItem value="C">C</SelectItem>
                            <SelectItem value="V">V</SelectItem>
                          </SelectContent>
                        </Select>
                      ) : (
                        <span className={`px-1.5 py-0.5 rounded text-xs font-medium ${b.tipoOperacao === 'C' ? 'bg-green-600 text-white' : 'bg-red-600 text-white'}`}>
                          {b.tipoOperacao}
                        </span>
                      )}
                    </TableCell>}
                    {colunaVisivel('status') && <TableCell className={`px-1 ${!isEditando && !isLigada ? 'cursor-pointer hover:bg-muted/50' : ''}`} onClick={() => !isEditando && !isLigada && iniciarEdicao(b)}>
                      {isEditando && !isLigada ? (
                        <Select value={data!.status} onValueChange={(v) => setEditBoleta({ ...data!, status: v })}>
                          <SelectTrigger className="h-7 text-xs"><SelectValue /></SelectTrigger>
                          <SelectContent>
                            {statusOpcoes.map((s) => <SelectItem key={s.value} value={s.value}>{s.label}</SelectItem>)}
                          </SelectContent>
                        </Select>
                      ) : (() => {
                        const statusInfo = statusOpcoes.find((s) => s.value === b.status);
                        return statusInfo ? (
                          <Badge className={`whitespace-nowrap ${statusInfo.color}`}>{statusInfo.label}</Badge>
                        ) : b.status;
                      })()}
                    </TableCell>}
                    {colunaVisivel('volume') && <TableCell className={`px-1 ${!isEditando && !isLigada ? 'cursor-pointer hover:bg-muted/50' : ''}`} onClick={() => !isEditando && !isLigada && iniciarEdicao(b)}>
                      {isEditando && !isLigada ? (
                        <Input className="h-7 text-xs" value={data!.volume} onChange={(e) => setEditBoleta({ ...data!, volume: e.target.value })} />
                      ) : b.volume?.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}
                    </TableCell>}
                    {colunaVisivel('quantidade') && <TableCell className={`px-1 ${!isEditando ? 'cursor-pointer hover:bg-muted/50' : ''}`} onClick={() => !isEditando && iniciarEdicao(b)}>
                      {isEditando ? (
                        <Input className="h-7 text-xs" value={data!.quantidade} onChange={(e) => setEditBoleta({ ...data!, quantidade: e.target.value })} />
                      ) : b.quantidade?.toLocaleString('pt-BR')}
                    </TableCell>}
                    {colunaVisivel('tipo') && <TableCell className={`px-1 ${!isEditando && !isLigada ? 'cursor-pointer hover:bg-muted/50' : ''}`} onClick={() => !isEditando && !isLigada && iniciarEdicao(b)}>
                      {isEditando && !isLigada ? (
                        <Select value={data!.tipoPrecificacao} onValueChange={(v) => setEditBoleta({ ...data!, tipoPrecificacao: v })}>
                          <SelectTrigger className="h-7 text-xs"><SelectValue /></SelectTrigger>
                          <SelectContent>
                            <SelectItem value="Nominal">Nominal</SelectItem>
                            <SelectItem value="Spread">Spread</SelectItem>
                          </SelectContent>
                        </Select>
                      ) : b.tipoPrecificacao === 'Spread' ? 'Spread' : 'Nominal'}
                    </TableCell>}
                    {colunaVisivel('ntnb') && <TableCell className={`px-1 ${!isEditando && !isLigada ? 'cursor-pointer hover:bg-muted/50' : ''}`} onClick={() => !isEditando && !isLigada && iniciarEdicao(b)}>
                      {isEditando && !isLigada && data!.tipoPrecificacao === 'Spread' ? (
                        <Select value={data!.ntnbReferencia} onValueChange={(v) => setEditBoleta({ ...data!, ntnbReferencia: v })}>
                          <SelectTrigger className="h-7 text-xs"><SelectValue placeholder="-" /></SelectTrigger>
                          <SelectContent>
                            {ntnbOpcoes.map((n) => <SelectItem key={n} value={n}>{n}</SelectItem>)}
                          </SelectContent>
                        </Select>
                      ) : b.ntnbReferencia || '-'}
                    </TableCell>}
                    {colunaVisivel('taxa') && <TableCell className={`px-1 ${!isEditando && !isLigada ? 'cursor-pointer hover:bg-muted/50' : ''}`} onClick={() => !isEditando && !isLigada && iniciarEdicao(b)}>
                      {isEditando && !isLigada ? (
                        data!.tipoPrecificacao === 'Spread' ? (
                          <Input className="h-7 text-xs" value={data!.spreadValor} onChange={(e) => setEditBoleta({ ...data!, spreadValor: e.target.value })} />
                        ) : (
                          <Input className="h-7 text-xs" value={data!.taxaNominal} onChange={(e) => setEditBoleta({ ...data!, taxaNominal: e.target.value })} />
                        )
                      ) : b.tipoPrecificacao === 'Spread' ? `${b.spreadValor ?? ''}bps` : b.taxaNominal?.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}
                    </TableCell>}
                    {colunaVisivel('dataFixing') && <TableCell className={`px-1 ${!isEditando && !isLigada ? 'cursor-pointer hover:bg-muted/50' : ''}`} onClick={() => !isEditando && !isLigada && iniciarEdicao(b)}>
                      {isEditando && !isLigada && data!.tipoPrecificacao === 'Spread' ? (
                        <DatePicker value={data!.dataFixing} onChange={(v) => setEditBoleta({ ...data!, dataFixing: v })} placeholder="Fixing" className="h-7 text-xs" />
                      ) : b.dataFixing ? new Date(b.dataFixing).toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit' }) : '-'}
                    </TableCell>}
                    {colunaVisivel('pu') && <TableCell className={`px-1 ${!isEditando && !isLigada ? 'cursor-pointer hover:bg-muted/50' : ''}`} onClick={() => !isEditando && !isLigada && iniciarEdicao(b)}>
                      {isEditando && !isLigada ? (
                        <Input className="h-7 text-xs" value={data!.pu} onChange={(e) => setEditBoleta({ ...data!, pu: e.target.value })} />
                      ) : b.pu?.toLocaleString('pt-BR', { minimumFractionDigits: 2 })}
                    </TableCell>}
                    {colunaVisivel('contraparte') && <TableCell className={`px-1 ${!isEditando && !isLigada ? 'cursor-pointer hover:bg-muted/50' : ''}`} onClick={() => !isEditando && !isLigada && iniciarEdicao(b)}>
                      {isEditando && !isLigada ? (
                        <Combobox options={contraparteOptions} value={data!.contraparteId} onChange={(v) => setEditBoleta({ ...data!, contraparteId: v as number | null })} placeholder="-" className="h-7 text-xs" />
                      ) : b.contraparteNome || '-'}
                    </TableCell>}
                    {colunaVisivel('alocacao') && <TableCell className={`px-1 ${!isEditando && !isLigada ? 'cursor-pointer hover:bg-muted/50' : ''}`} onClick={() => !isEditando && !isLigada && iniciarEdicao(b)}>
                      {isEditando && !isLigada ? (
                        <Input className="h-7 text-xs" value={data!.alocacao} onChange={(e) => setEditBoleta({ ...data!, alocacao: e.target.value })} />
                      ) : b.alocacao}
                    </TableCell>}
                    {colunaVisivel('usuario') && <TableCell className={`px-1 ${!isEditando && !isLigada ? 'cursor-pointer hover:bg-muted/50' : ''}`} onClick={() => !isEditando && !isLigada && iniciarEdicao(b)}>
                      {isEditando && !isLigada ? (
                        <Input className="h-7 text-xs" value={data!.usuario} onChange={(e) => setEditBoleta({ ...data!, usuario: e.target.value })} />
                      ) : b.usuario}
                    </TableCell>}
                    {colunaVisivel('liquidacao') && <TableCell className={`px-1 ${!isEditando && !isLigada ? 'cursor-pointer hover:bg-muted/50' : ''}`} onClick={() => !isEditando && !isLigada && iniciarEdicao(b)}>
                      {isEditando && !isLigada ? (
                        <DatePicker value={data!.dataLiquidacao} onChange={(v) => setEditBoleta({ ...data!, dataLiquidacao: v })} placeholder="Liquidacao" className="h-7 text-xs" />
                      ) : b.dataLiquidacao ? new Date(b.dataLiquidacao).toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit' }) : '-'}
                    </TableCell>}
                  </TableRow>
                );
              };

              return (
                <React.Fragment key={boleta.id}>
                  {renderLinhaBoleta(boleta, false)}
                  {isExpandida && ligadas.map(ligada => renderLinhaBoleta(ligada, true))}
                </React.Fragment>
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
