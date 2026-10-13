import React, { useEffect, useState } from 'react';
import { createRoot } from 'react-dom/client';
import { ArrowUpRight, AudioLines, CalendarDays, Camera, Check, ChevronRight, CircleHelp, Layers, LogOut, Monitor, Plus, Search, SlidersHorizontal, X } from 'lucide-react';
import { api, interval, type Booking, type Equipment, type Slot, type User } from './api';
import './style.css';

function App() {
  const [user, setUser] = useState<User | null>(null);
  const [items, setItems] = useState<Equipment[]>([]);
  const [bookings, setBookings] = useState<Booking[]>([]);
  const [page, setPage] = useState('catalog');
  const [query, setQuery] = useState('');
  const [category, setCategory] = useState('Все');
  const [error, setError] = useState('');
  const [notice, setNotice] = useState('');
  const [loading, setLoading] = useState(true);
  const [auth, setAuth] = useState(false);
  const [selected, setSelected] = useState<Equipment | null>(null);
  const [adding, setAdding] = useState(false);
  async function reload() {
    setError(''); setLoading(true);
    try { setItems(await api<Equipment[]>('/equipment')); }
    catch { setError('Не удалось загрузить каталог. Проверьте, что API запущен на порту 5080.'); }
    finally { setLoading(false); }
  }
  async function loadBookings() { try { setBookings(await api<Booking[]>('/bookings')); } catch (e) { setError((e as Error).message); } }
  useEffect(() => { void reload(); api<User>('/auth/me').then(setUser).catch(() => {}); }, []);
  useEffect(() => { if (user && page === 'bookings') void loadBookings(); }, [user, page]);
  const filtered = items.filter(i => (category === 'Все' || i.category === category) && `${i.name} ${i.description}`.toLowerCase().includes(query.toLowerCase()));
  const categories = ['Все', ...new Set(items.map(i => i.category))];
  const active = bookings.filter(b => !b.isCancelled && new Date(b.endsAt) > new Date());
  async function logout() { try { await api('/auth/logout', 'POST'); setUser(null); setBookings([]); setPage('catalog'); } catch (e) { setError((e as Error).message); } }
  return <div className="shell">
    <aside className="sidebar">
      <a className="brand" href="#" onClick={() => setPage('catalog')}><span className="brand-mark"><Layers size={23}/></span>equip<span className="brand-dot">.</span></a>
      <div className="workspace"><span className="workspace-icon">W</span><div><b>Workspace</b><small>Общее оборудование</small></div></div>
      <div className="nav-label">ВАШЕ ПРОСТРАНСТВО</div>
      <nav><button className={page === 'catalog' ? 'active' : ''} onClick={() => setPage('catalog')}><Layers size={19}/>Каталог оборудования<ChevronRight size={16}/></button>
      <button className={page === 'bookings' ? 'active' : ''} onClick={() => user ? setPage('bookings') : setAuth(true)}><CalendarDays size={19}/>{user?.role === 'Admin' ? 'Все бронирования' : 'Мои бронирования'}</button></nav>
      <div className="sidebar-bottom"><div className="help-card"><CircleHelp size={21}/><b>Как это работает?</b><p>Выберите оборудование, укажите время и забронируйте его для своей следующей идеи.</p></div>
      {user ? <div className="profile"><span className="avatar">{user.name[0]}</span><div><b>{user.name}</b><small>{user.role === 'Admin' ? 'Администратор' : 'Участник'}</small></div><button aria-label="Выйти" onClick={logout}><LogOut size={18}/></button></div> : <button className="primary full" onClick={() => setAuth(true)}>Войти в аккаунт<ArrowUpRight size={17}/></button>}</div>
    </aside>
    <main>
      <header className="topbar"><div>Workspace <ChevronRight size={14}/> <strong>{page === 'catalog' ? 'Каталог' : 'Бронирования'}</strong></div><span className="top-tag"><span/> Пространство для идей</span></header>
      <div className="content">
        {error && <div role="alert" className="alert">{error}<button onClick={reload}>Повторить</button></div>}
        {notice && <div role="status" className="notice"><Check size={18}/>{notice}<button aria-label="Закрыть уведомление" onClick={() => setNotice('')}><X size={16}/></button></div>}
        <div className="page-title"><div><span className="eyebrow">{page === 'catalog' ? 'МЕНЬШЕ ОГРАНИЧЕНИЙ. БОЛЬШЕ ВОЗМОЖНОСТЕЙ.' : 'ВСЁ ПО ПЛАНУ'}</span><h1>{page === 'catalog' ? 'Инструменты для ваших идей' : 'Ваши бронирования'}</h1><p>{page === 'catalog' ? 'Нужная техника в нужный момент. Выбирайте и бронируйте онлайн.' : 'Управляйте планами и освобождайте оборудование, если они изменились.'}</p></div>{user?.role === 'Admin' && page === 'catalog' && <button className="primary" onClick={() => setAdding(true)}><Plus size={18}/>Добавить</button>}</div>
        {page === 'catalog' ? <>
          <section className="hero"><div><span className="hero-tag">ОБЩИЕ РЕСУРСЫ · БОЛЬШИЕ ИДЕИ</span><h2>Создавайте.<br/>Остальное уже здесь.</h2><p>От первой презентации до нового подкаста —<br/>найдите оборудование для следующего шага.</p><a href="#catalog">Посмотреть оборудование <ArrowUpRight size={17}/></a></div><div className="hero-art" aria-hidden="true"><div className="orbit orbit-one"/><div className="orbit orbit-two"/><div className="art-tile tile-camera"><Camera size={72} strokeWidth={1.2}/></div><div className="art-tile tile-audio"><AudioLines size={54} strokeWidth={1.3}/></div><span className="art-label"><span/> Готово к вашим планам</span><span className="spark">✦</span></div></section>
          <div className="section-heading" id="catalog"><h2>Каталог оборудования <span>{items.length}</span></h2><small>Бронирование до 24 часов</small></div>
          <div className="filters"><div className="tabs">{categories.map(c => <button key={c} className={category === c ? 'selected' : ''} onClick={() => setCategory(c)}>{c}</button>)}</div><label className="search"><Search size={17}/><input aria-label="Поиск оборудования" placeholder="Найти оборудование…" value={query} onChange={e => setQuery(e.target.value)}/></label></div>
          {loading ? <div className="empty">Загружаем оборудование…</div> : !filtered.length ? <div className="empty"><Search/><h3>Ничего не найдено</h3><p>Попробуйте другую категорию или измените запрос.</p><button onClick={() => { setQuery(''); setCategory('Все'); }}>Сбросить фильтры</button></div> : <div className="grid">{filtered.map((item, n) => <article className="equipment-card" key={item.id}><div className={`product-art art-${n % 4}`}><span className={`availability ${!item.isActive ? 'unavailable' : ''}`}><span/>{item.isActive ? 'Доступно для брони' : 'На обслуживании'}</span><ProductIcon category={item.category}/><span className="product-number">EQ / {String(items.indexOf(item) + 1).padStart(3, '0')}</span></div><div className="card-body"><span className="category">{item.category}</span><h3>{item.name}</h3><p>{item.description}</p><div className="card-footer"><span>Всё необходимое в комплекте</span><button aria-label={`Забронировать ${item.name}`} disabled={!item.isActive} onClick={() => user ? setSelected(item) : setAuth(true)}><ArrowUpRight size={21}/></button></div>{user?.role === 'Admin' && <button className="text-button" onClick={async () => { try { await api(`/equipment/${item.id}`, 'PUT', { ...item, isActive: !item.isActive }); await reload(); } catch (e) { setError((e as Error).message); } }}>{item.isActive ? 'Отправить на обслуживание' : 'Вернуть в каталог'}</button>}</div></article>)}</div>}
        </> : <><div className="booking-summary"><CalendarDays/><div><b>{active.length} активных бронирований</b><p>Время отображается в вашем часовом поясе: {Intl.DateTimeFormat().resolvedOptions().timeZone}</p></div></div>{!bookings.length ? <div className="empty"><CalendarDays size={36}/><h3>Пока никаких планов</h3><p>Начните с каталога — оборудование уже ждёт.</p><button className="primary" onClick={() => setPage('catalog')}>Открыть каталог</button></div> : <div className="booking-list">{bookings.map(b => <div className="booking-row" key={b.id}><div className="booking-icon"><CalendarDays size={24}/></div><div className="booking-details"><h3>{b.equipmentName}</h3><p>{interval(b.startsAt, b.endsAt)}</p>{user?.role === 'Admin' && <small>{b.userName}</small>}</div><span className={`status ${b.isCancelled ? 'cancelled' : ''}`}>{b.isCancelled ? 'Отменено' : new Date(b.endsAt) <= new Date() ? 'Завершено' : 'Запланировано'}</span>{!b.isCancelled && new Date(b.endsAt) > new Date() && <button className="secondary" onClick={async () => { if (!confirm('Отменить бронирование?')) return; try { await api(`/bookings/${b.id}/cancel`, 'POST'); await loadBookings(); setNotice('Бронирование отменено.'); } catch (e) { setError((e as Error).message); } }}>Отменить</button>}</div>)}</div>}</>}
        <footer><span>equip. · Больше пространства для ваших идей</span><span>Бронируйте осознанно. Делитесь возможностями.</span></footer>
      </div>
    </main>
    {auth && <AuthModal close={() => setAuth(false)} done={u => { setUser(u); setAuth(false); setNotice('Вы вошли в аккаунт. Можно бронировать!'); }}/>}
    {selected && <BookingModal item={selected} close={() => setSelected(null)} done={() => { setSelected(null); setPage('bookings'); void loadBookings(); setNotice('Оборудование забронировано. До встречи!'); }}/>}
    {adding && <EquipmentModal close={() => setAdding(false)} done={() => { setAdding(false); void reload(); }}/>}
  </div>;
}
function ProductIcon({ category }: { category: string }) {
  return <div className="product-icon">{category === 'Камеры' ? <Camera/> : category === 'Аудио' ? <AudioLines/> : category === 'Презентации' ? <Monitor/> : <SlidersHorizontal/>}</div>;
}
function Modal({ title, close, children }: { title: string; close: () => void; children: React.ReactNode }) {
  useEffect(() => { const handler = (e: KeyboardEvent) => { if (e.key === 'Escape') close(); }; document.addEventListener('keydown', handler); return () => document.removeEventListener('keydown', handler); }, [close]);
  return <div className="modal-backdrop" onClick={close}><section className="modal" role="dialog" aria-modal="true" aria-label={title} onClick={e => e.stopPropagation()}><div className="modal-heading"><h2>{title}</h2><button aria-label="Закрыть" onClick={close}><X/></button></div>{children}</section></div>;
}
function AuthModal({ close, done }: { close: () => void; done: (u: User) => void }) {
  const [register, setRegister] = useState(false); const [error, setError] = useState(''); const [busy, setBusy] = useState(false);
  return <Modal title={register ? 'Присоединиться к equip' : 'С возвращением'} close={close}><p className="modal-intro">Всё, что нужно для ваших идей, в одном месте.</p><form onSubmit={async e => { e.preventDefault(); const data = Object.fromEntries(new FormData(e.currentTarget)); setBusy(true); setError(''); try { done(await api<User>(`/auth/${register ? 'register' : 'login'}`, 'POST', data)); } catch (e) { setError((e as Error).message); } finally { setBusy(false); } }}>
  {register && <label>Ваше имя<input name="name" required minLength={2} maxLength={80} autoComplete="name" autoFocus/></label>}<label>Email<input name="email" type="email" required maxLength={200} autoComplete="email" autoFocus={!register}/></label><label>Пароль<input name="password" type="password" required minLength={register ? 10 : 1} maxLength={128} autoComplete={register ? 'new-password' : 'current-password'}/></label>{register && <small>Не менее 10 символов.</small>}{error && <p role="alert" className="form-error">{error}</p>}<button className="primary full" disabled={busy}>{busy ? 'Подождите…' : register ? 'Создать аккаунт' : 'Войти'}<ArrowUpRight size={18}/></button></form><button className="text-button full" onClick={() => { setRegister(!register); setError(''); }}>{register ? 'Уже есть аккаунт? Войти' : 'Нет аккаунта? Зарегистрироваться'}</button></Modal>;
}
function BookingModal({ item, close, done }: { item: Equipment; close: () => void; done: () => void }) {
  const [slots, setSlots] = useState<Slot[]>([]); const [error, setError] = useState(''); const [busy, setBusy] = useState(false);
  useEffect(() => { api<Slot[]>(`/equipment/${item.id}/schedule`).then(setSlots).catch(() => setError('Не удалось загрузить расписание.')); }, [item.id]);
  return <Modal title="Забронировать оборудование" close={close}><div className="selected-equipment"><ProductIcon category={item.category}/><div><small>{item.category}</small><h3>{item.name}</h3></div></div><p className="modal-intro">До 24 часов, на 90 дней вперёд. Укажите местное время.</p><form onSubmit={async e => { e.preventDefault(); const data = new FormData(e.currentTarget); setBusy(true); setError(''); try { await api('/bookings', 'POST', { equipmentId: item.id, startsAt: new Date(String(data.get('start'))).toISOString(), endsAt: new Date(String(data.get('end'))).toISOString() }); done(); } catch (e) { setError((e as Error).message); api<Slot[]>(`/equipment/${item.id}/schedule`).then(setSlots).catch(() => {}); } finally { setBusy(false); } }}><label>Начало<input type="datetime-local" name="start" required autoFocus/></label><label>Окончание<input type="datetime-local" name="end" required/></label>{error && <p role="alert" className="form-error">{error}</p>}<button className="primary full" disabled={busy}>{busy ? 'Бронируем…' : 'Подтвердить бронирование'}<Check size={18}/></button></form><div className="schedule"><h4>Занятые интервалы</h4>{slots.length ? slots.map((s, i) => <p key={i}>{interval(s.startsAt, s.endsAt)}</p>) : <p>Ближайших бронирований нет.</p>}</div></Modal>;
}
function EquipmentModal({ close, done }: { close: () => void; done: () => void }) {
  const [error, setError] = useState(''); const [busy, setBusy] = useState(false);
  return <Modal title="Новое оборудование" close={close}><form onSubmit={async e => { e.preventDefault(); const data = Object.fromEntries(new FormData(e.currentTarget)); setBusy(true); try { await api('/equipment', 'POST', data); done(); } catch (e) { setError((e as Error).message); } finally { setBusy(false); } }}><label>Название<input name="name" required maxLength={120} autoFocus/></label><label>Категория<input name="category" required maxLength={60} list="categories"/><datalist id="categories">{['Камеры', 'Аудио', 'Презентации', 'Дизайн'].map(c => <option key={c}>{c}</option>)}</datalist></label><label>Описание<textarea name="description" maxLength={1000} rows={3}/></label>{error && <p role="alert" className="form-error">{error}</p>}<button className="primary full" disabled={busy}>{busy ? 'Сохраняем…' : 'Добавить оборудование'}</button></form></Modal>;
}
createRoot(document.getElementById('root')!).render(<React.StrictMode><App/></React.StrictMode>);
