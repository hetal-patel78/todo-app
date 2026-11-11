import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Todo } from './todo.model';

@Injectable({ providedIn: 'root' })
export class TodoApiService {
  private readonly http = inject(HttpClient);
  private readonly url = '/api/todos';

  getAll(): Observable<Todo[]> {
    return this.http.get<Todo[]>(this.url);
  }

  add(title: string): Observable<Todo> {
    return this.http.post<Todo>(this.url, { title });
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
}
