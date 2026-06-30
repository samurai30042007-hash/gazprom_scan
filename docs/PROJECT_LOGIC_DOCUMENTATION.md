# Полная документация по логике проекта AIReviewSystem

Ниже приведена актуальная документация по основным файлам проекта. Она описывает не только назначение каждого файла, но и то, как он участвует в общей цепочке: от загрузки источника кода до формирования списка статических находок.

## 1. Общая архитектура

Проект построен по схеме, близкой к Clean Architecture:

- Web layer — пользовательский интерфейс и точки входа.
- Application layer — абстракции и контракты бизнес-сценариев.
- Domain layer — доменные сущности, перечисления и значения.
- Infrastructure layer — реализация работы с Git, Roslyn, файловой системой, EF Core и PostgreSQL.
- Contracts — DTO и контракты передачи данных.

Главная бизнес-идея приложения — принимать источник кода, анализировать его и показывать пользователю список проблем, найденных в изменённых C#-файлах.

## 2. Web-слой

### [src/AIReviewSystem.Web/Program.cs](../src/AIReviewSystem.Web/Program.cs)
- Краткое описание: Точка входа ASP.NET Core приложения.
- Логика реализации:
  - создаёт веб-хост;
  - подключает Blazor Server и Razor Components;
  - регистрирует сервисы уровня Application и Infrastructure;
  - конфигурирует middleware для HTTPS, ошибок, antiforgery и маршрутизации;
  - запускает приложение и включает интерактивный режим Blazor Server.

### [src/AIReviewSystem.Web/Components/Pages/Analysis.razor](../src/AIReviewSystem.Web/Components/Pages/Analysis.razor)
- Краткое описание: Основная страница анализа.
- Логика реализации:
  - показывает форму выбора источника данных: локальный путь, локальная папка, загрузка папки/файлов на сервер;
  - при выборе варианта с загрузкой файлов использует компонент InputFile для выбора нескольких файлов и целой папки;
  - при нажатии кнопки запускает анализ;
  - отображает результат в виде списка найденных диагностик и сводки по строкам;
  - в зависимости от источника либо запускает локальный Git-анализ, либо анализирует загруженную рабочую папку.

### [src/AIReviewSystem.Web/Components/Pages/Analysis.razor.cs](../src/AIReviewSystem.Web/Components/Pages/Analysis.razor.cs)
- Краткое описание: Code-behind логика страницы анализа.
- Логика реализации:
  - хранит состояние формы: путь, выбранный источник, загруженные файлы, статус анализа, найденные строки и diagnostics;
  - в методе AnalyzeAsync выбирает сценарий выполнения:
    - если выбран режим загрузки папки, файлы копируются во временную директорию и передаются в CodeAnalysisService;
    - если выбран локальный путь/папка, используется IRepositoryProvider и IGitAnalyzer;
  - для сохранения сессии используется репозиторий сессий и Unit of Work;
  - при отсутствии доступа к базе данных обработка не ломается: программа логирует предупреждение и продолжает анализ.

### [src/AIReviewSystem.Web/Components/Pages/History.razor](../src/AIReviewSystem.Web/Components/Pages/History.razor)
- Краткое описание: Страница истории анализа.
- Логика реализации: На текущем этапе выступает как экран-сценарий для будущего показа истории сессий, результатов анализа и статусов. Сейчас в ней нет полноценной бизнес-логики, а только базовый интерфейс.

### [src/AIReviewSystem.Web/Components/Pages/Home.razor](../src/AIReviewSystem.Web/Components/Pages/Home.razor)
- Краткое описание: Главная страница.
- Логика реализации: Предоставляет входную точку для навигации по приложению. В текущем варианте носит скорее информационный характер.

### [src/AIReviewSystem.Web/Components/Pages/Counter.razor](../src/AIReviewSystem.Web/Components/Pages/Counter.razor)
- Краткое описание: Демонстрационная страница шаблона Blazor.
- Логика реализации: Не используется в основной бизнес-цепочке и оставлена как шаблонная часть.

### [src/AIReviewSystem.Web/Components/Pages/Error.razor](../src/AIReviewSystem.Web/Components/Pages/Error.razor)
- Краткое описание: Страница ошибки.
- Логика реализации: Показывает информацию о неожиданной ошибке в приложении и используется инфраструктурой ASP.NET Core.

### [src/AIReviewSystem.Web/Components/Pages/Settings.razor](../src/AIReviewSystem.Web/Components/Pages/Settings.razor)
- Краткое описание: Страница настроек.
- Логика реализации: На текущем этапе не связана с анализом и служит шаблонной частью интерфейса.

### [src/AIReviewSystem.Web/Components/Pages/Weather.razor](../src/AIReviewSystem.Web/Components/Pages/Weather.razor)
- Краткое описание: Демонстрационный пример данных.
- Логика реализации: Не участвует в рабочем сценарии анализа и сохранен как стандартный шаблон проекта.

## 3. Application layer

### [src/AIReviewSystem.Application/DependencyInjection.cs](../src/AIReviewSystem.Application/DependencyInjection.cs)
- Краткое описание: Регистрация application-сервисов.
- Логика реализации: Является точкой расширения для подключения слоёв приложения в DI-контейнер. На текущем этапе здесь нет сложной логики, но файл поддерживает будущую оркестрацию анализа.

### [src/AIReviewSystem.Application/Abstractions/Analysis/IGitAnalyzer.cs](../src/AIReviewSystem.Application/Abstractions/Analysis/IGitAnalyzer.cs)
- Краткое описание: Контракт Git-анализатора.
- Логика реализации: Определяет входной путь к репозиторию и возвращает снимок репозитория вместе со списком изменённых файлов.

### [src/AIReviewSystem.Application/Abstractions/Analysis/ILanguageAnalyzer.cs](../src/AIReviewSystem.Application/Abstractions/Analysis/ILanguageAnalyzer.cs)
- Краткое описание: Контракт анализатора конкретного языка.
- Логика реализации: Описывает, способен ли анализатор работать с данным файлом, и как он должен возвращать список статических находок.

### [src/AIReviewSystem.Application/Abstractions/Analysis/IStaticAnalysisService.cs](../src/AIReviewSystem.Application/Abstractions/Analysis/IStaticAnalysisService.cs)
- Краткое описание: Контракт сервиса статического анализа.
- Логика реализации: Предоставляет единый сценарий анализа для всей сессии с передачей объекта AnalysisSession.

### [src/AIReviewSystem.Application/Abstractions/Analysis/IRepositoryProvider.cs](../src/AIReviewSystem.Application/Abstractions/Analysis/IRepositoryProvider.cs)
- Краткое описание: Контракт провайдера репозитория.
- Логика реализации:
  - определяет перечисление RepositorySourceKind с типами источников:
    - LocalPath,
    - LocalFolder,
    - ZipArchive,
    - GitUrl;
  - описывает RepositoryRequest и интерфейс ResolveAsync, который должен вернуть рабочую директорию или путь к репозиторию.

### [src/AIReviewSystem.Application/Abstractions/IUnitOfWork.cs](../src/AIReviewSystem.Application/Abstractions/IUnitOfWork.cs)
- Краткое описание: Контракт Unit of Work.
- Логика реализации: Объединяет сохранение состояния в базе данных в одном месте, чтобы слой Web не был привязан к конкретной реализации EF Core.

### [src/AIReviewSystem.Application/Abstractions/Repositories/IAnalysisSessionRepository.cs](../src/AIReviewSystem.Application/Abstractions/Repositories/IAnalysisSessionRepository.cs)
- Краткое описание: Репозиторий сессий анализа.
- Логика реализации: Позволяет получать, создавать и обновлять сессии анализа.

### [src/AIReviewSystem.Application/Abstractions/Repositories/IStaticFindingRepository.cs](../src/AIReviewSystem.Application/Abstractions/Repositories/IStaticFindingRepository.cs)
- Краткое описание: Репозиторий статических находок.
- Логика реализации: Хранит и извлекает результаты анализа в виде списка StaticFinding.

### [src/AIReviewSystem.Application/Abstractions/Repositories/IReportArtifactRepository.cs](../src/AIReviewSystem.Application/Abstractions/Repositories/IReportArtifactRepository.cs)
- Краткое описание: Репозиторий артефактов отчётов.
- Логика реализации: Предназначен для хранения и поиска отчётов по сессиям анализа.

### [src/AIReviewSystem.Application/Abstractions/Services/IAnalysisWorkflowService.cs](../src/AIReviewSystem.Application/Abstractions/Services/IAnalysisWorkflowService.cs)
- Краткое описание: Контракт workflow-сервиса.
- Логика реализации: Описывает запуск анализа по идентификатору сессии. В текущей реализации используется как заготовка под будущую оркестрацию.

### [src/AIReviewSystem.Application/Abstractions/Services/IReportExportService.cs](../src/AIReviewSystem.Application/Abstractions/Services/IReportExportService.cs)
- Краткое описание: Контракт экспорта отчётов.
- Логика реализации: Должен генерировать отчёт по результатам анализа в формате Markdown или другом удобном формате.

## 4. Domain layer

### [src/AIReviewSystem.Domain/Entities/AnalysisSession.cs](../src/AIReviewSystem.Domain/Entities/AnalysisSession.cs)
- Краткое описание: Главная сущность сессии.
- Логика реализации:
  - хранит идентификатор сессии;
  - хранит путь к репозиторию или загруженной рабочей папке;
  - хранит статус выполнения;
  - хранит время старта и завершения;
  - содержит связанные наборы изменённых файлов, найденных проблем и отчётов.

### [src/AIReviewSystem.Domain/Entities/ChangedFile.cs](../src/AIReviewSystem.Domain/Entities/ChangedFile.cs)
- Краткое описание: Модель изменённого файла.
- Логика реализации:
  - сохраняет путь к файлу;
  - определяет тип изменения;
  - хранит количество строк добавления и удаления;
  - помечает файл как C#-файл для последующего анализа.

### [src/AIReviewSystem.Domain/Entities/StaticFinding.cs](../src/AIReviewSystem.Domain/Entities/StaticFinding.cs)
- Краткое описание: Нормализованная находка статического анализа.
- Логика реализации:
  - хранит идентификатор правила;
  - хранит уровень важности;
  - хранит текст сообщения;
  - хранит файл, строку и колонку возникновения проблемы;
  - хранит имя анализатора.

### [src/AIReviewSystem.Domain/Entities/RepositorySnapshot.cs](../src/AIReviewSystem.Domain/Entities/RepositorySnapshot.cs)
- Краткое описание: Снимок состояния репозитория.
- Логика реализации: Служит для фиксации ветки, коммита и режима diff на момент анализа.

### [src/AIReviewSystem.Domain/Entities/ReportArtifact.cs](../src/AIReviewSystem.Domain/Entities/ReportArtifact.cs)
- Краткое описание: Артефакт результата анализа.
- Логика реализации: Хранит информацию о том, где физически расположен отчёт, в каком формате он сохранён и каким хешем он описан.

### [src/AIReviewSystem.Domain/ValueObjects/CommitRange.cs](../src/AIReviewSystem.Domain/ValueObjects/CommitRange.cs)
- Краткое описание: Объект диапазона коммитов.
- Логика реализации: Представляет пару базового и целевого коммита в виде самостоятельного значения.

### [src/AIReviewSystem.Domain/ValueObjects/RepositoryPath.cs](../src/AIReviewSystem.Domain/ValueObjects/RepositoryPath.cs)
- Краткое описание: Значимый объект пути к репозиторию.
- Логика реализации: Оборачивает строковый путь и делает работу с путём более явной и типобезопасной.

### [src/AIReviewSystem.Domain/ValueObjects/FileLocation.cs](../src/AIReviewSystem.Domain/ValueObjects/FileLocation.cs)
- Краткое описание: Значимый объект расположения файла.
- Логика реализации: Служит для передачи точек обнаружения диагностик в дальнейшем отчёте.

### [src/AIReviewSystem.Domain/Enums/AnalysisStatus.cs](../src/AIReviewSystem.Domain/Enums/AnalysisStatus.cs)
- Краткое описание: Состояния жизненного цикла сессии.
- Логика реализации: Используется для маркировки статуса сессии: черновик, выполняется, завершено, ошибка.

### [src/AIReviewSystem.Domain/Enums/ChangeType.cs](../src/AIReviewSystem.Domain/Enums/ChangeType.cs)
- Краткое описание: Возможные типы изменений файла.
- Логика реализации: Нормализует изменения, чтобы UI мог отображать их в понятном виде.

### [src/AIReviewSystem.Domain/Enums/SeverityLevel.cs](../src/AIReviewSystem.Domain/Enums/SeverityLevel.cs)
- Краткое описание: Уровни серьёзности проблем.
- Логика реализации: Определяет масштаб серьёзности найденной диагностики.

## 5. Infrastructure layer

### [src/AIReviewSystem.Infrastructure/DependencyInjection.cs](../src/AIReviewSystem.Infrastructure/DependencyInjection.cs)
- Краткое описание: Конфигурация инфраструктурных сервисов.
- Логика реализации:
  - читает строку подключения из конфигурации;
  - настраивает EF Core с PostgreSQL через Npgsql;
  - регистрирует UnitOfWork, репозитории, Git-анализатор, провайдер репозитория и Roslyn-сервисы.

### [src/AIReviewSystem.Infrastructure/Persistence/ReviewDbContext.cs](../src/AIReviewSystem.Infrastructure/Persistence/ReviewDbContext.cs)
- Краткое описание: EF Core контекст данных.
- Логика реализации:
  - описывает наборы сущностей AnalysisSession, RepositorySnapshot, ChangedFile, StaticFinding, ReportArtifact;
  - задаёт таблицы и связи между ними;
  - настраивает каскадное удаление связанных данных.

### [src/AIReviewSystem.Infrastructure/Persistence/EfUnitOfWork.cs](../src/AIReviewSystem.Infrastructure/Persistence/EfUnitOfWork.cs)
- Краткое описание: Реализация Unit of Work на EF Core.
- Логика реализации: Делегирует сохранение всех изменений в контекст базы данных через SaveChangesAsync.

### [src/AIReviewSystem.Infrastructure/Repositories/EfAnalysisSessionRepository.cs](../src/AIReviewSystem.Infrastructure/Repositories/EfAnalysisSessionRepository.cs)
- Краткое описание: Репозиторий сессий на EF Core.
- Логика реализации:
  - получает сессию по идентификатору вместе с вложенным snapshot;
  - возвращает последние сессии по времени старта;
  - добавляет новые записи;
  - обновляет существующие записи.

### [src/AIReviewSystem.Infrastructure/Repositories/EfStaticFindingRepository.cs](../src/AIReviewSystem.Infrastructure/Repositories/EfStaticFindingRepository.cs)
- Краткое описание: Репозиторий статических находок на EF Core.
- Логика реализации: Извлекает находки по идентификатору сессии и пакетно добавляет их в базу.

### [src/AIReviewSystem.Infrastructure/Repositories/EfReportArtifactRepository.cs](../src/AIReviewSystem.Infrastructure/Repositories/EfReportArtifactRepository.cs)
- Краткое описание: Репозиторий артефактов отчётов.
- Логика реализации: Ищет последний отчёт по сессии и добавляет новый артефакт при генерации результата.

### [src/AIReviewSystem.Infrastructure/Analysis/LibGit2GitAnalyzer.cs](../src/AIReviewSystem.Infrastructure/Analysis/LibGit2GitAnalyzer.cs)
- Краткое описание: Git-анализатор на базе LibGit2Sharp.
- Логика реализации:
  - определяется путь к Git-репозиторию через Repository.Discover;
  - анализируется рабочее дерево;
  - формируется список изменённых файлов;
  - рассчитываются добавления и удаления строк;
  - для каждого файла определяется, является ли он C#-файлом.

### [src/AIReviewSystem.Infrastructure/Analysis/GitRepositoryProvider.cs](../src/AIReviewSystem.Infrastructure/Analysis/GitRepositoryProvider.cs)
- Краткое описание: Провайдер источника репозитория.
- Логика реализации:
  - для локального пути или папки проверяет существование директории и валидность Git-репозитория;
  - для ZIP и Git URL пока отдаёт NotSupportedException, так как это не реализовано в MVP.

### [src/AIReviewSystem.Infrastructure/Analysis/RoslynLanguageAnalyzer.cs](../src/AIReviewSystem.Infrastructure/Analysis/RoslynLanguageAnalyzer.cs)
- Краткое описание: Roslyn-анализатор для C#.
- Логика реализации:
  - находит solution или project в рабочей директории;
  - открывает решение через MSBuildWorkspace;
  - находит проект, содержащий конкретный изменённый файл;
  - создаёт Compilation и берёт диагностические сообщения;
  - возвращает список StaticFinding для конкретного файла.

### [src/AIReviewSystem.Infrastructure/Analysis/RoslynStaticAnalysisService.cs](../src/AIReviewSystem.Infrastructure/Analysis/RoslynStaticAnalysisService.cs)
- Краткое описание: Оркестратор статического анализа.
- Логика реализации:
  - берёт все изменённые C#-файлы из сессии;
  - по каждому файлу вызывает ILanguageAnalyzer;
  - собирает все находки в один список;
  - сохраняет их в репозиторий находок.

### [src/AIReviewSystem.Infrastructure/Analysis/CodeAnalysisService.cs](../src/AIReviewSystem.Infrastructure/Analysis/CodeAnalysisService.cs)
- Краткое описание: Сервис анализа загруженной рабочей папки.
- Логика реализации:
  - принимает путь к временной папке с загруженными файлами;
  - ищет solution или project в этой папке;
  - открывает проект через MSBuildWorkspace;
  - строит Compilation и возвращает список диагностик для всех C#-файлов.

## 6. Contracts layer

### [src/AIReviewSystem.Contracts/Requests/StartAnalysisRequest.cs](../src/AIReviewSystem.Contracts/Requests/StartAnalysisRequest.cs)
- Краткое описание: Запрос запуска анализа.
- Логика реализации: Передаёт путь к источнику анализа как неизменяемый DTO.

### [src/AIReviewSystem.Contracts/Responses/AnalysisSessionResponse.cs](../src/AIReviewSystem.Contracts/Responses/AnalysisSessionResponse.cs)
- Краткое описание: DTO сессии анализа.
- Логика реализации: Формирует наружный ответ о сессии: идентификатор, путь, статус, время запуска и завершения.

### [src/AIReviewSystem.Contracts/Responses/ReportArtifactResponse.cs](../src/AIReviewSystem.Contracts/Responses/ReportArtifactResponse.cs)
- Краткое описание: DTO артефакта отчёта.
- Логика реализации: Переносит информацию об отчёте в наружный слой.

### [src/AIReviewSystem.Contracts/Common/AnalysisStatusDto.cs](../src/AIReviewSystem.Contracts/Common/AnalysisStatusDto.cs)
- Краткое описание: DTO статуса сессии.
- Логика реализации: Дублирует доменный enum для передачи данных через HTTP/контракты.

## 7. Поток выполнения анализа

1. Пользователь выбирает источник: локальный путь, локальная папка или папка с файлами для загрузки.
2. Если выбран локальный путь/папка, UI передаёт путь в RepositoryProvider.
3. RepositoryProvider проверяет, что это Git-репозиторий, и возвращает рабочий путь.
4. Git-анализатор находит изменённые файлы и собирает метаданные по ним.
5. Для каждого изменённого C#-файла Roslyn анализирует проект/решение и создаёт диагностические сообщения.
6. Результаты сохраняются в сессии анализа и отображаются в интерфейсе.
7. Если выбран режим загрузки папки, файлы копируются во временную директорию и анализируется уже эта папка как рабочее пространство.

## 8. Текущее состояние проекта

Проект находится на этапе MVP и уже умеет:

- принимать локальный путь или рабочую папку;
- принимать загруженную папку с файлами проекта;
- анализировать изменённые C#-файлы через Roslyn;
- сохранять сессию и найденные проблемы;
- показывать результаты в интерфейсе.

Оставшиеся ограничения:

- ZIP-архивы и Git URL ещё не реализованы;
- экспорты отчётов и полноценный workflow-сервис находятся в заготовке;
- сохранение в PostgreSQL зависит от доступности базы данных.

