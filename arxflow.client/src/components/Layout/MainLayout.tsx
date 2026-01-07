import { useState } from 'react';
import { Outlet } from 'react-router-dom';
import { Box, Toolbar, Container } from '@mui/material';
import TopBar from './TopBar';
import SideMenu from './SideMenu';

export default function MainLayout() {
  const [menuOpen, setMenuOpen] = useState(false);

  const toggleMenu = () => setMenuOpen(!menuOpen);
  const closeMenu = () => setMenuOpen(false);

  return (
    <Box sx={{ display: 'flex', minHeight: '100vh' }}>
      <TopBar onMenuToggle={toggleMenu} />
      <SideMenu open={menuOpen} onClose={closeMenu} />
      <Box
        component="main"
        sx={{
          flexGrow: 1,
          bgcolor: 'background.default',
          p: 3,
        }}
      >
        <Toolbar />
        <Container maxWidth={false}>
          <Outlet />
        </Container>
      </Box>
    </Box>
  );
}
