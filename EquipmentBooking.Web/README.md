# Equip — React frontend

React + TypeScript + Vite. Backend находится в соседней папке `EquipmentBooking`.

```powershell
cd C:\Users\maxim\RiderProjects\EquipmentBooking.Web
npm install
npm run dev
```

Интерфейс: http://localhost:5173. API должен работать на http://localhost:5080; Vite проксирует запросы `/api` в него. Не открывайте `index.html` напрямую.

```powershell
npm test
npm run build
npm run preview
```

Каталог, поиск, категории, регистрация/вход, расписание, бронирование, отмена. Администратор может добавлять оборудование и переключать обслуживание. Сессия в серверной HttpOnly cookie; токены не хранятся в localStorage. Клиент получает CSRF-токен перед каждым изменением.

`src/api.ts` — типы и HTTP-клиент; `src/main.tsx` — интерфейс; `src/style.css` — адаптивные стили. Иконки — lucide-react. Шрифт Manrope загружается из Google Fonts, при недоступной сети используется sans-serif.

Полные инструкции, схема БД и описание защиты от двойного бронирования — в README backend-проекта. Для production следует отдавать `dist` и проксировать `/api` через один HTTPS origin; `npm run dev` предназначен для разработки.
