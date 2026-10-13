import { afterEach, describe, expect, it, vi } from 'vitest';
import { api, interval } from './api';
afterEach(() => vi.unstubAllGlobals());
describe('API client', () => {
  it('fetches a fresh CSRF token before mutations and sends session credentials', async () => {
    const fetchMock = vi.fn().mockResolvedValueOnce(new Response(JSON.stringify({ token: 'fresh-token' })))
      .mockResolvedValueOnce(new Response(null, { status: 204 }));
    vi.stubGlobal('fetch', fetchMock);
    await api('/auth/logout', 'POST');
    expect(fetchMock.mock.calls[0][0]).toBe('/api/auth/csrf');
    expect(fetchMock.mock.calls[1][1]).toMatchObject({ credentials: 'same-origin', headers: { 'X-CSRF-TOKEN': 'fresh-token' } });
  });
  it('shows a server conflict message to the user', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response(JSON.stringify({ error: 'Время занято' }), { status: 409 })));
    await expect(api('/bookings')).rejects.toThrow('Время занято');
  });
  it('does not send a mutation if token retrieval fails', async () => {
    const fetchMock = vi.fn().mockResolvedValue(new Response(null, { status: 503 }));
    vi.stubGlobal('fetch', fetchMock);
    await expect(api('/bookings', 'POST', {})).rejects.toThrow('соединение');
    expect(fetchMock).toHaveBeenCalledTimes(1);
  });
});
describe('booking interval display', () => {
  it('formats UTC timestamps in the visitor timezone', () => {
    const result = interval('2026-10-15T08:00:00Z', '2026-10-15T09:00:00Z');
    expect(result).toContain('15');
    expect(result).toContain('—');
    expect(result).not.toContain('Invalid');
  });
});
