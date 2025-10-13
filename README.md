<div align="center" class="header">  
  <img src="https://github.com/user-attachments/assets/1c88e9b9-01ff-4e22-b835-b09c900fb399" alt="Banner" width="100%" />
</div>

# Adventure Time: Space Station 14

**Adventure Time** — это русскоязычный сервер Space Station 14, с полной локализацией, частыми обновлениями и собственными изменениями (модификациями).  
Проект работает на движке **Robust Toolbox** (C#).  

---

## 🔗 Полезные ссылки

| Назначение | Ссылка |
|------------|--------|
| Наш Discord | https://discord.gg/NY3KDNuH9r |
| Вики проекта | https://wiki.adventurestation.space/Заглавная_страница |
| Steam (официальный клиент) | https://store.steampowered.com/app/1255460/Space_Station_14/ |
| Клиент без Steam (nightlies) | https://spacestation14.io/about/nightlies/ |
| Основной репозиторий SS14 | https://github.com/space-wizards/space-station-14 |
| Репозиторий сервера ADT | https://github.com/AdventureTimeSS14/space_station_ADT |

---

## 📚 О проекте

- **Цель**: реализация полностью русскоязычного сервера на движке SS14, поддержка изменений из upstream, и добавление собственных фич.  
- **Контрибьютинг**: вы можете присоединиться — у нас открыт список задач. Убедитесь, что PR соответствует руководству по вкладу.  
- **Лицензии ассетов**: большинство ресурсов — под лицензией **CC-BY-SA 3.0**, но некоторые — под лицензией с ограничением на коммерческое использование (CC-BY-NC-SA или аналогичной). При коммерческом использовании такие ассеты надо исключать.

---

## 🛠 Как собрать

```bash
git clone <репозиторий>
cd <клон>
# Инициализация подмодулей (если не сработало простое клонирование)
git submodule update --init --recursive
# Или запустить RUN_THIS.py

# Сборка
dotnet build

# Запуск серверной части и клиентской части
dotnet run --project Content.Server
dotnet run --project Content.Client
# Или запустить
runserver.bat
runclient.bat
````

Более подробная инструкция по настройке — в официальной документации SS14: [https://docs.spacestation14.com/en/general-development/setup.html](https://docs.spacestation14.com/en/general-development/setup.html)
