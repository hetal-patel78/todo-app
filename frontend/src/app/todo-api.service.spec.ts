import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { TodoApiService } from './todo-api.service';
import { Todo } from './todo.model';

describe('TodoApiService', () => {
  let service: TodoApiService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(TodoApiService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  const todo: Todo = { id: '1', title: 'Milk', createdAt: '2026-01-01T00:00:00Z' };

  it('getAll calls GET /api/todos', () => {
    let result: Todo[] | undefined;
    service.getAll().subscribe((r) => (result = r));
    const req = http.expectOne('/api/todos');
    expect(req.request.method).toBe('GET');
    req.flush([todo]);
    expect(result).toEqual([todo]);
  });

  it('add calls the POST /api/tools with the title', () => {
    let result: Todo | undefined;
    service.add('Milk').subscribe((r) => (result = r));
    const req = http.expectOne('/api/todos');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ title: 'Milk' });
    req.flush(todo);
    expect(result).toEqual(todo);
  });

  it('delete call DELETE /api/todo/:id', () => {
    service.delete('1').subscribe();
    const req = http.expectOne('/api/todos/1');
    expect(req.request.method).toBe('DELETE');
    req.flush(null, { status: 204, statusText: 'No Conent' });
  });
});
