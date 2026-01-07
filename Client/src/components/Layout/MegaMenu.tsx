import { useNavigate, useLocation } from 'react-router-dom';
import {
  NavigationMenu,
  NavigationMenuContent,
  NavigationMenuItem,
  NavigationMenuLink,
  NavigationMenuList,
  NavigationMenuTrigger,
} from '@/components/ui/navigation-menu';
import {
  LayoutDashboard,
  FileText,
  Calculator,
  LineChart,
  CloudDownload,
  DollarSign,
  Building2,
  Landmark,
  Users,
  CheckSquare,
} from 'lucide-react';
import { cn } from '@/lib/utils';

interface MenuItemData {
  label: string;
  path: string;
  icon: React.ReactNode;
  description: string;
}

interface MenuGroup {
  label: string;
  items: MenuItemData[];
}

// Itens de menu agrupados por categoria
const menuGroups: MenuGroup[] = [
  {
    label: 'Operações',
    items: [
      {
        label: 'Dashboard',
        path: '/',
        icon: <LayoutDashboard className="h-5 w-5" />,
        description: 'Visão geral do sistema',
      },
      {
        label: 'Boletas',
        path: '/boletas',
        icon: <FileText className="h-5 w-5" />,
        description: 'Gerenciar boletas de operações',
      },
    ],
  },
  {
    label: 'Renda Fixa',
    items: [
      {
        label: 'Calculadora de Títulos',
        path: '/calculadora',
        icon: <Calculator className="h-5 w-5" />,
        description: 'Calcular PU e taxas de títulos',
      },
      {
        label: 'Validação',
        path: '/validacao',
        icon: <CheckSquare className="h-5 w-5" />,
        description: 'Validar calculadoras com dados ANBIMA',
      },
    ],
  },
  {
    label: 'Juros',
    items: [
      {
        label: 'Curva de Juros',
        path: '/yield-curve',
        icon: <LineChart className="h-5 w-5" />,
        description: 'Visualizar curvas de juros',
      },
    ],
  },
  {
    label: 'Auxiliares',
    items: [
      {
        label: 'Downloads',
        path: '/downloads',
        icon: <CloudDownload className="h-5 w-5" />,
        description: 'Download de dados B3, ANBIMA, BCB',
      },
    ],
  },
  {
    label: 'Cadastros',
    items: [
      {
        label: 'Ativos',
        path: '/ativos',
        icon: <DollarSign className="h-5 w-5" />,
        description: 'Cadastro de ativos',
      },
      {
        label: 'Emissores',
        path: '/emissores',
        icon: <Building2 className="h-5 w-5" />,
        description: 'Cadastro de emissores',
      },
      {
        label: 'Fundos',
        path: '/fundos',
        icon: <Landmark className="h-5 w-5" />,
        description: 'Cadastro de fundos',
      },
      {
        label: 'Contrapartes',
        path: '/contrapartes',
        icon: <Users className="h-5 w-5" />,
        description: 'Cadastro de contrapartes',
      },
    ],
  },
];

// Estilos para sobrescrever os padrões do NavigationMenu em fundo escuro
const triggerOverrideStyles = '!bg-transparent !text-primary-foreground hover:!bg-white/10 hover:!text-primary-foreground focus:!bg-white/10 focus:!text-primary-foreground data-[state=open]:!bg-white/10 data-[state=open]:!text-primary-foreground';

export default function MegaMenu() {
  const navigate = useNavigate();
  const location = useLocation();

  // Verifica se algum item do grupo esta ativo
  const isGroupActive = (group: MenuGroup): boolean => {
    return group.items.some((item) => location.pathname === item.path);
  };

  return (
    <NavigationMenu>
      <NavigationMenuList>
        {/* Grupos com dropdown */}
        {menuGroups.map((group) => {
          const isActive = isGroupActive(group);

          return (
            <NavigationMenuItem key={group.label}>
              <NavigationMenuTrigger
                className={cn(
                  triggerOverrideStyles,
                  isActive && '!bg-white/20 !font-bold'
                )}
              >
                {group.label}
              </NavigationMenuTrigger>
              <NavigationMenuContent>
                <ul className="grid w-[400px] gap-1 p-2 md:w-[500px] md:grid-cols-2">
                  {group.items.map((item) => (
                    <li key={item.path}>
                      <NavigationMenuLink
                        className={cn(
                          'flex items-start gap-3 select-none rounded-md p-3 leading-none no-underline outline-none cursor-pointer',
                          'hover:bg-accent hover:text-accent-foreground focus:bg-accent focus:text-accent-foreground',
                          location.pathname === item.path && 'bg-primary text-primary-foreground hover:bg-primary hover:text-primary-foreground'
                        )}
                        onClick={() => navigate(item.path)}
                      >
                        <div className="mt-0.5">{item.icon}</div>
                        <div className="flex flex-col gap-1">
                          <span className="text-sm font-medium leading-none">{item.label}</span>
                          <span className={cn(
                            "text-xs leading-snug",
                            location.pathname === item.path ? 'text-primary-foreground/80' : 'text-muted-foreground'
                          )}>
                            {item.description}
                          </span>
                        </div>
                      </NavigationMenuLink>
                    </li>
                  ))}
                </ul>
              </NavigationMenuContent>
            </NavigationMenuItem>
          );
        })}
      </NavigationMenuList>
    </NavigationMenu>
  );
}
