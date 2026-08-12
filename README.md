# MusicBetter

Кроссплатформенный музыкальный плеер (Android + Windows) на .NET MAUI, без рекламы,
с локальной библиотекой и офлайн-избранным. Интерфейс — в духе Mi Music (тёмная тема,
быстрые карточки "Избранные / Плейлисты / Недавние", список треков, мини-плеер снизу).

## Что умеет

- **Своя музыка полностью офлайн.** Импортируешь файлы (mp3/flac/wav/m4a/ogg/aac) через
  системный проводник — они добавляются в локальную SQLite-базу вместе с метаданными
  (исполнитель/альбом/обложка, читаются из тегов через TagLib#). Работает без интернета.
- **Избранное офлайн.** Помечаешь трек звёздочкой/⋮ — он остаётся в базе и доступен без сети,
  если это локальный файл.
- **Поиск по YouTube** через официальный YouTube Data API v3 + воспроизведение через
  официальный YouTube IFrame Player внутри WebView (`EmbedPlayerPage`).
- **SoundCloud** — резолв публичных ссылок на треки через официальный SoundCloud API,
  воспроизведение через официальный SoundCloud Widget (`w.soundcloud.com/player`).
- Никакой рекламы, никаких трекеров, никакой телеметрии.

## Важная оговорка по стриминговым источникам

Приложение **не скачивает** аудио из YouTube/SoundCloud/Spotify и не работает с ними в обход
их условий использования — это было бы нарушением ToS и, скорее всего, незаконным. Поэтому:

- Треки из YouTube/SoundCloud проигрываются **только при наличии интернета**, через
  официальные встроенные плееры площадок — это их ограничение, не наше.
- Полноценной интеграции со Spotify нет и не может быть: Spotify Web API отдаёт только
  метаданные и управление плеера через официальный SDK, сам аудиопоток остаётся
  внутри экосистемы Spotify (доступен только Premium-пользователям через их приложение).
  Если хочешь — можно добавить только раздел "открыть в Spotify" (deep link), без плеера внутри.
- "Офлайн работает везде" честно относится только к **твоим собственным файлам**,
  добавленным в библиотеку.

## Структура проекта

```
MusicBetter/
  MusicBetter.csproj        — таргеты net8.0-android и net8.0-windows10.0.19041.0
  MauiProgram.cs
  App.xaml(.cs), AppShell.xaml(.cs)
  Models/                   — Track, Playlist, SearchResult
  Data/AppDatabase.cs       — SQLite (sqlite-net-pcl), файл musicbetter.db3 в AppDataDirectory
  Services/
    LibraryService.cs       — импорт локальных файлов + чтение тегов
    YouTubeSearchService.cs — поиск через YouTube Data API v3
    SoundCloudSearchService.cs — resolve через SoundCloud API
    PlayerService.cs        — очередь воспроизведения, избранное
    AppConfig.cs            — сюда вписываешь свои API-ключи
  ViewModels/                — MVVM (CommunityToolkit.Mvvm)
  Views/
    MainPage.xaml(.cs)       — главный экран (как на скрине)
    EmbedPlayerPage.xaml(.cs)— WebView с официальным плеером YouTube/SoundCloud
  Resources/                 — иконка, splash, стили, изображения
  Platforms/Android, Platforms/Windows — платформенный код
```

## Как собрать

1. Установи **Visual Studio 2022** (17.9+) с workload **.NET Multi-platform App UI development**.
2. Открой `MusicBetter.sln`.
3. Впиши свои ключи в `Services/AppConfig.cs`:
   - `YouTubeApiKey` — получить на https://console.cloud.google.com/apis/library/youtube.googleapis.com
   - `SoundCloudClientId` — заявка на https://developers.soundcloud.com/docs/api/guide
4. Выбери target framework (`net8.0-android` для телефона/эмулятора,
   `net8.0-windows10.0.19041.0` для Windows) и нажми Run.

Пакеты (`sqlite-net-pcl`, `TagLibSharp`, `CommunityToolkit.Mvvm`, `CommunityToolkit.Maui`)
подтянутся из NuGet автоматически при первой сборке.

## Автоматические сборки и релизы (GitHub Actions)

В `.github/` три workflow-файла и dependabot:

- **workflows/build.yml** — при каждом push/PR в `main` проверяет, что Android- и
  Windows-сборки компилируются без ошибок (быстрая проверка, без релиза).
- **workflows/auto-tag.yml** — при каждом пуше в `main` автоматически создаёт новый тег
  `vX.Y.Z` (увеличивает patch-версию) и пушит его. Чтобы пропустить это для конкретного
  коммита, добавь `[skip release]` в текст коммита.
- **workflows/release.yml** — запускается по тегу `v*.*.*` (в том числе созданному
  auto-tag.yml) либо вручную (Actions → Release → Run workflow). Собирает APK (Android)
  и zip (Windows) и публикует их в разделе **Releases**.
- **dependabot.yml** — раз в неделю сам присылает PR на обновление NuGet-пакетов и версий
  GitHub Actions.

Также в корне репозитория лежит **global.json** — он жёстко фиксирует версию .NET SDK
(8.0.x) для всех сборок. Без него раннеры GitHub иногда подхватывают другую
предустановленную версию SDK, из-за чего меняется поведение флагов CLI и сборка падает
с непонятными ошибками — так и было на предыдущих попытках.

### Как это работает на практике

Теперь релиз собирается **сам** при каждом пуше в `main` — ничего вручную запускать не
нужно. Просто:
```
git add .
git commit -m "какие-то изменения"
git push origin main
```
Через 5-10 минут в **Releases** появится новая версия с `MusicBetter-android.apk` и
`MusicBetter-windows.zip`.

Если нужно собрать релиз без пуша в main (например, пересобрать текущую версию) —
**Actions → Release → Run workflow**, указать версию вручную.

Если в репозитории уже есть старый упавший `dotnet.yml` — удали его, чтобы не было
конфликтующих workflow.

APK подписывается стандартным debug-ключом .NET SDK — этого достаточно для установки
и личного использования. Для публикации в Google Play в будущем потребуется отдельный
release-keystore и настройка подписи в `MusicBetter.csproj`.

## Публикация на GitHub

Перед тем как выкладывать репозиторий публично:
- **Не коммить** заполненные API-ключи в `AppConfig.cs` — либо оставь их пустыми и опиши
  в README, что их нужно вписать локально, либо вынеси в переменные окружения/`local.settings.json`
  и добавь этот файл в `.gitignore` (пример уже включён).
- Проверь `bin/`, `obj/` — тоже в `.gitignore`, чтобы не тащить мусор сборки в репозиторий.

## Возможные доработки

- Загрузка обложек альбомов для локальных треков из открытых источников (MusicBrainz/Cover Art Archive).
- Плейлисты с drag&drop сортировкой (модель `Playlist`/`PlaylistTrack` уже готова в БД).
- Синхронизация библиотеки между Android и Windows через свой backend (например, Supabase) —
  синхронизировались бы только метаданные и статус "избранное", не сами аудиофайлы.
- Полноэкранный плеер с анимацией обложки, таймером сна, эквалайзером для локальных треков.
