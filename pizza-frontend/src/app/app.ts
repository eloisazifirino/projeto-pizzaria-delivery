import { Component } from '@angular/core';
import { CatalogoComponent } from './components/catalogo/catalogo';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CatalogoComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  title = 'pizza-frontend';
}