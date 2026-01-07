import {
  Drawer,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Divider,
  Toolbar,
  Box,
} from '@mui/material';
import { useNavigate, useLocation } from 'react-router-dom';
import HomeIcon from '@mui/icons-material/Home';
import DescriptionIcon from '@mui/icons-material/Description';
import CalculateIcon from '@mui/icons-material/Calculate';
import ShowChartIcon from '@mui/icons-material/ShowChart';
import CloudDownloadIcon from '@mui/icons-material/CloudDownload';
import AttachMoneyIcon from '@mui/icons-material/AttachMoney';
import BusinessIcon from '@mui/icons-material/Business';
import AccountBalanceIcon from '@mui/icons-material/AccountBalance';
import PeopleIcon from '@mui/icons-material/People';

const DRAWER_WIDTH = 240;

interface SideMenuProps {
  open: boolean;
  onClose: () => void;
}

interface MenuItem {
  label: string;
  path: string;
  icon: React.ReactNode;
  group?: string;
}

const menuItems: MenuItem[] = [
  { label: 'Início', path: '/', icon: <HomeIcon /> },
  { label: 'Boletas', path: '/boletas', icon: <DescriptionIcon />, group: 'Operações' },
  { label: 'Calculadora de Títulos', path: '/calculadora', icon: <CalculateIcon />, group: 'Auxiliares' },
  { label: 'Curva de Juros', path: '/yield-curve', icon: <ShowChartIcon />, group: 'Auxiliares' },
  { label: 'Downloads', path: '/downloads', icon: <CloudDownloadIcon />, group: 'Auxiliares' },
  { label: 'Ativos', path: '/ativos', icon: <AttachMoneyIcon />, group: 'Cadastros' },
  { label: 'Emissores', path: '/emissores', icon: <BusinessIcon />, group: 'Cadastros' },
  { label: 'Fundos', path: '/fundos', icon: <AccountBalanceIcon />, group: 'Cadastros' },
  { label: 'Contrapartes', path: '/contrapartes', icon: <PeopleIcon />, group: 'Cadastros' },
];

export default function SideMenu({ open, onClose }: SideMenuProps) {
  const navigate = useNavigate();
  const location = useLocation();

  const handleNavigate = (path: string) => {
    navigate(path);
    onClose();
  };

  const renderMenuItems = () => {
    const groups: { [key: string]: MenuItem[] } = { '': [] };
    menuItems.forEach((item) => {
      const group = item.group || '';
      if (!groups[group]) groups[group] = [];
      groups[group].push(item);
    });

    return Object.entries(groups).map(([groupName, items]) => (
      <Box key={groupName || 'main'}>
        {groupName && (
          <>
            <Divider sx={{ my: 1 }} />
            <ListItem>
              <ListItemText
                primary={groupName}
                primaryTypographyProps={{
                  variant: 'caption',
                  color: 'text.secondary',
                  fontWeight: 600,
                }}
              />
            </ListItem>
          </>
        )}
        {items.map((item) => (
          <ListItem key={item.path} disablePadding>
            <ListItemButton
              selected={location.pathname === item.path}
              onClick={() => handleNavigate(item.path)}
            >
              <ListItemIcon>{item.icon}</ListItemIcon>
              <ListItemText primary={item.label} />
            </ListItemButton>
          </ListItem>
        ))}
      </Box>
    ));
  };

  return (
    <Drawer
      variant="temporary"
      open={open}
      onClose={onClose}
      sx={{
        width: DRAWER_WIDTH,
        flexShrink: 0,
        '& .MuiDrawer-paper': {
          width: DRAWER_WIDTH,
          boxSizing: 'border-box',
        },
      }}
    >
      <Toolbar />
      <Box sx={{ overflow: 'auto' }}>
        <List>{renderMenuItems()}</List>
      </Box>
    </Drawer>
  );
}
