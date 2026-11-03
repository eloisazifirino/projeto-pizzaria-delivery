import { Routes } from '@angular/router';
import { CatalogoComponent } from './components/catalogo/catalogo';
import { PedidosComponent } from './components/pedidos/pedidos.component';

export const routes: Routes = [
  {
    path: 'loja',
    children: [
      { path: '', pathMatch: 'full', component: CatalogoComponent },
      { path: 'pedido', component: PedidosComponent }
    ]
  },
  {
    path: 'admin',
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'pedidos' },
      { path: 'pedidos', component: PedidosComponent, data: { administrativo: true } }
    ]
  },
  { path: '', pathMatch: 'full', redirectTo: 'loja' }
];
