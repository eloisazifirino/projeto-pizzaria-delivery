import { Routes } from '@angular/router';
import { LojaComponent } from './components/loja/loja';
import { AdminComponent } from './components/admin/admin';

export const routes: Routes = [
  { path: 'loja', component: LojaComponent },
  { path: 'loja/pedido', redirectTo: 'loja', pathMatch: 'full' },
  { path: 'admin', component: AdminComponent },
  { path: 'admin/pedidos', redirectTo: 'admin', pathMatch: 'full' },
  { path: '', redirectTo: 'loja', pathMatch: 'full' },
  { path: '**', redirectTo: 'loja' }
];
