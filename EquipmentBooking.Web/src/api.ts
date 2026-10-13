export type User = { id: string; name: string; email: string; role: string };
export type Equipment = { id: string; name: string; category: string; description: string; isActive: boolean };
export type Slot = { startsAt: string; endsAt: string };
export type Booking = Slot & { id: string; equipmentId: string; equipmentName: string; userName: string; isCancelled: boolean };
export async function api<T>(path: string, method = 'GET', body?: unknown): Promise<T> {
  const headers: Record<string, string> = {};
  if (method !== 'GET') {
    const csrf = await fetch('/api/auth/csrf', { credentials: 'same-origin' });
    if (!csrf.ok) throw new Error('Не удалось установить соединение.');
    headers['X-CSRF-TOKEN'] = (await csrf.json()).token;
    headers['Content-Type'] = 'application/json';
  }
  const response = await fetch(`/api${path}`, { method, headers, credentials: 'same-origin', body: body === undefined ? undefined : JSON.stringify(body) });
  if (!response.ok) {
    const data = await response.json().catch(() => ({}));
    throw new Error(data.error || (response.status === 401 ? 'Войдите в аккаунт.' : response.status === 403 ? 'Недостаточно прав.' : 'Не удалось выполнить запрос. Попробуйте ещё раз.'));
  }
  return response.status === 204 ? undefined as T : response.json();
}
export function interval(start: string, end: string) {
  const format = new Intl.DateTimeFormat('ru-RU', { day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit' });
  return `${format.format(new Date(start))} — ${format.format(new Date(end))}`;
}
