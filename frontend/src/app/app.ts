import { ChangeDetectionStrategy, Component } from '@angular/core';
import { TodoList } from './todo-list/todo-list';

@Component({
  selector: 'app-root',
  imports: [TodoList],
  styleUrl: './app.css',
  templateUrl: './app.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {}
