import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Todo } from '../todo.model';
import { TodoList } from './todo-list';

const milk: Todo = { id: '1', title: 'Milk', createdAt: '2026-01-01T00:00:00Z' };
const eggs: Todo = { id: '2', title: 'Eggs', createdAt: '2026-01-01T00:00:00Z' };

describe('TodoList', () => {
  let fixture: ComponentFixture<TodoList>;
  let http: HttpTestingController;
  let el: HTMLElement;

  async function render(inital: Todo[] = []): Promise<void> {
    fixture = TestBed.createComponent(TodoList);
    el = fixture.nativeElement;
    fixture.detectChanges();
    http.expectOne('/api/todos').flush(inital);
    await fixture.whenStable();
  }

  const titles = () => Array.from(el.querySelectorAll('.title')).map((e) => e.textContent?.trim());

  function addViaForm(text: string): void {
    const input = el.querySelector('input') as HTMLInputElement;
    input.value = text;
    el.querySelector('form')!.dispatchEvent(new Event('submit'));
  }

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('shows an empty state when there are no todos', async () => {
    await render();
    expect(el.textContent).toContain('Nothing to do');
  });

  it('lists existing todos', async () => {
    await render([milk, eggs]);
    expect(titles()).toEqual(['Milk', 'Eggs']);
  });

  it('shows an error when loading fails', async () => {
    fixture = TestBed.createComponent(TodoList);
    el = fixture.nativeElement;
    fixture.detectChanges();
    http.expectOne('/api/todos').flush('boom', { status: 500, statusText: 'Server Error' });
    await fixture.whenStable();

    expect(el.querySelector('[role="alert"]')?.textContent).toContain('could not load');
  });

  it('adds a trimmed todo and clears the input', async () => {
    await render();
    addViaForm('  Milk  ');

    const req = http.expectOne('/api/todos');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ title: 'Milk' });
    req.flush(milk);
    await fixture.whenStable();

    expect(titles()).toEqual(['Milk']);
    expect((el.querySelector('input') as HTMLInputElement).value).toBe('');
  });

  it('does not call the API for a blank title', async () => {
    await render();
    addViaForm('   ');

    http.expectNone('/api/todos');
  });

  it('keeps the input and shows an error when adding fails', async () => {
    await render();
    addViaForm('Milk');
    http.expectOne('/api/todos').flush('boom', { status: 500, statusText: 'Server Error' });
    await fixture.whenStable();

    expect(el.querySelector('[role="alert"]')?.textContent).toContain('could not add');
    expect((el.querySelector('input') as HTMLInputElement).value).toBe('Milk');
  });

  it('deletes a todo', async () => {
    await render([milk, eggs]);

    (el.querySelectorAll('.delete')[0] as HTMLButtonElement).click();
    const req = http.expectOne('/api/todos/1');
    expect(req.request.method).toBe('DELETE');
    req.flush(null, { status: 204, statusText: 'No Content' });
    await fixture.whenStable();

    expect(titles()).toEqual(['Eggs']);
  });

  it('keeps the todo and shows an error when deleting fails', async () => {
    await render([milk]);

    (el.querySelector('.delete') as HTMLButtonElement).click();
    http.expectOne('/api/todos/1').flush('boom', { status: 500, statusText: 'Server Error' });
    await fixture.whenStable();

    expect(titles()).toEqual(['Milk']);
    expect(el.querySelector('[role="alert"]')?.textContent).toContain('could not delete');
  });
});
