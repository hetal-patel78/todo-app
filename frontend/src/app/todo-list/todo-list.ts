import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { TodoApiService } from '../todo-api.service';
import { MAX_TITLE_LENGTH, Todo } from '../todo.model';

@Component({
  selector: 'app-todo-list',
  styleUrl: './todo-list.css',
  templateUrl: './todo-list.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TodoList implements OnInit {
  private readonly api = inject(TodoApiService);
  protected readonly maxLength = MAX_TITLE_LENGTH;
  protected readonly todos = signal<Todo[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.api.getAll().subscribe({
      next: (todos) => {
        this.todos.set(todos);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('could not load the todos');
        this.loading.set(false);
      },
    });
  }

  protected add(input: HTMLInputElement): void {
    const title = input.value.trim();
    if (!title) {
      return;
    }
    this.error.set(null);
    this.api.add(title).subscribe({
      next: (todo) => {
        this.todos.update((list) => [...list, todo]);
        input.value = '';
        input.focus();
      },
      error: () => this.error.set('could not add the to do. Please try again'),
    });
  }

  protected remove(todo: Todo): void {
    this.error.set(null);
    this.api.delete(todo.id).subscribe({
      next: () => this.todos.update((list) => list.filter((t) => t.id != todo.id)),
      error: () => this.error.set('could not delete todo. Please try again.'),
    });
  }
}
