# Документация по рукописной логике проекта AIReviewSystem

Ниже перечислены основные рукописные файлы, в которых реализована бизнес-логика, инфраструктурная логика и пользовательский сценарий анализа репозитория. Шаблонные страницы с минимальной разметкой и стандартные заглушки не включены в этот список.

## 1. Web и точки входа

### [src/AIReviewSystem.Web/Program.cs](../src/AIReviewSystem.Web/Program.cs)
- Краткое описание: Точка входа ASP.NET Core приложения.
- Логика реализации: Создаёт приложение, подключает Razor Components, регистрирует сервисы уровня Application и Infrastructure, настраивает middleware для обработки ошибок, HTTPS, antiforgery и маршрутизацию Blazor Server.

### [src/AIReviewSystem.Web/Components/Pages/Analysis.razor](../src/AIReviewSystem.Web/Components/Pages/Analysis.razor)
- Краткое описание: Основная страница анализа Git-репозитория.
- Логика реализации: При нажатии кнопки принимает путь к репозиторию, пытается определить Git-корень через LibGit2Sharp, валидирует его, читает diff, строит список изменённых файлов, считает добавленные и удалённые строки, переводит статус изменений в человекочитаемый формат и отображает результат в таблице.

### [src/AIReviewSystem.Web/Components/Pages/History.razor](../src/AIReviewSystem.Web/Components/Pages/History.razor)
- Краткое описание: Страница истории анализов.
- Логика реализации: На текущем этапе отображает только описание сценария и архитектурный статус. В будущем здесь должен быть вывод сохранённых сессий, отчётов и статусов из базы данных.

## 2. Application Layer

### [src/AIReviewSystem.Application/DependencyInjection.cs](../src/AIReviewSystem.Application/DependencyInjection.cs)
- Краткое описание: Регистрация сервисов уровня Application.
- Логика реализации: Предоставляет точку расширения для подключения application-сервисов в контейнер зависимостей. На текущем этапе она пока не добавляет конкретных сервисов, но является входной точкой для будущей оркестрации анализа.

### [src/AIReviewSystem.Application/Abstractions/Services/IAnalysisWorkflowService.cs](../src/AIReviewSystem.Application/Abstractions/Services/IAnalysisWorkflowService.cs)
- Краткое описание: Контракт workflow-сервиса анализа.
- Логика реализации: Определяет единый сценарий запуска анализа по идентификатору сессии и поддержкой отмены операции.

### [src/AIReviewSystem.Application/Abstractions/Services/IReportExportService.cs](../src/AIReviewSystem.Application/Abstractions/Services/IReportExportService.cs)
- Краткое описание: Контракт экспорта отчёта.
- Логика реализации: Описывает действие генерации Markdown-отчёта по результатам конкретной сессии анализа.

### [src/AIReviewSystem.Application/Abstractions/Analysis/IGitAnalyzer.cs](../src/AIReviewSystem.Application/Abstractions/Analysis/IGitAnalyzer.cs)
- Краткое описание: Контракт анализатора Git.
- Логика реализации: Определяет входные параметры в виде пути к репозиторию и выходные данные в виде снимка репозитория и списка изменённых файлов.

### [src/AIReviewSystem.Application/Abstractions/Analysis/ILanguageAnalyzer.cs](../src/AIReviewSystem.Application/Abstractions/Analysis/ILanguageAnalyzer.cs)
- Краткое описание: Контракт языкового анализатора.
- Логика реализации: Описывает механизм выбора анализатора по типу файла, его возможность анализа и выдачу списка статических находок для конкретного изменённого файла.

### [src/AIReviewSystem.Application/Abstractions/Analysis/IStaticAnalysisService.cs](../src/AIReviewSystem.Application/Abstractions/Analysis/IStaticAnalysisService.cs)
- Краткое описание: Контракт сервиса статического анализа.
- Логика реализации: Определяет единый способ запуска анализа для всей сессии и получения списка найденных проблем.

### [src/AIReviewSystem.Application/Abstractions/IUnitOfWork.cs](../src/AIReviewSystem.Application/Abstractions/IUnitOfWork.cs)
- Краткое описание: Контракт транзакционной единицы работы.
- Логика реализации: Обеспечивает единый интерфейс для сохранения изменений в базе данных через единый контекст.

### [src/AIReviewSystem.Application/Abstractions/Repositories/IAnalysisSessionRepository.cs](../src/AIReviewSystem.Application/Abstractions/Repositories/IAnalysisSessionRepository.cs)
- Краткое описание: Репозиторий сессий анализа.
- Логика реализации: Предоставляет операции чтения и записи сессий анализа, включая получение по идентификатору, получение последних сессий и обновление состояния.

### [src/AIReviewSystem.Application/Abstractions/Repositories/IStaticFindingRepository.cs](../src/AIReviewSystem.Application/Abstractions/Repositories/IStaticFindingRepository.cs)
- Краткое описание: Репозиторий статических находок.
- Логика реализации: Управляет получением и массовой записью результатов статического анализа для конкретной сессии.

### [src/AIReviewSystem.Application/Abstractions/Repositories/IReportArtifactRepository.cs](../src/AIReviewSystem.Application/Abstractions/Repositories/IReportArtifactRepository.cs)
- Краткое описание: Репозиторий артефактов отчётов.
- Логика реализации: Служит для получения последнего отчёта по сессии и добавления новых артефактов экспорта.

## 3. Domain Layer

### [src/AIReviewSystem.Domain/Entities/AnalysisSession.cs](../src/AIReviewSystem.Domain/Entities/AnalysisSession.cs)
- Краткое описание: Главная сущность сессии анализа.
- Логика реализации: Хранит состояние анализа, путь к репозиторию, статус выполнения, время старта/завершения, сводку и связанные коллекции изменённых файлов, находок и отчётов.

### [src/AIReviewSystem.Domain/Entities/RepositorySnapshot.cs](../src/AIReviewSystem.Domain/Entities/RepositorySnapshot.cs)
- Краткое описание: Снимок состояния репозитория на момент анализа.
- Логика реализации: Служит для сохранения ветки, базового и целевого коммита, а также режима сравнения diff.

### [src/AIReviewSystem.Domain/Entities/ChangedFile.cs](../src/AIReviewSystem.Domain/Entities/ChangedFile.cs)
- Краткое описание: Модель изменённого файла.
- Логика реализации: Хранит путь файла, тип изменения, число добавленных и удалённых строк и признак того, что файл относится к C#.

### [src/AIReviewSystem.Domain/Entities/StaticFinding.cs](../src/AIReviewSystem.Domain/Entities/StaticFinding.cs)
- Краткое описание: Нормализованная находка статического анализа.
- Логика реализации: Описывает проблему анализа как правило с идентификатором, уровнем серьёзности, сообщением, путём к файлу, номером строки и именем анализатора.

### [src/AIReviewSystem.Domain/Entities/ReportArtifact.cs](../src/AIReviewSystem.Domain/Entities/ReportArtifact.cs)
- Краткое описание: Артефакт отчёта.
- Логика реализации: Сохраняет формат отчёта, физическое место хранения, хеш содержимого и время создания.

### [src/AIReviewSystem.Domain/ValueObjects/CommitRange.cs](../src/AIReviewSystem.Domain/ValueObjects/CommitRange.cs)
- Краткое описание: Значимый объект диапазона коммитов.
- Логика реализации: Представляет пару базового и целевого коммита как неизменяемую сущность для последующего анализа diff.

### [src/AIReviewSystem.Domain/ValueObjects/RepositoryPath.cs](../src/AIReviewSystem.Domain/ValueObjects/RepositoryPath.cs)
- Краткое описание: Значимый объект пути к репозиторию.
- Логика реализации: Оборачивает строку пути в отдельный тип, чтобы логика работы с путями была более явной и типобезопасной.

### [src/AIReviewSystem.Domain/ValueObjects/FileLocation.cs](../src/AIReviewSystem.Domain/ValueObjects/FileLocation.cs)
- Краткое описание: Значимый объект расположения файла.
- Логика реализации: Хранит путь к файлу и необязательную строку номера строки, что удобно для передачи точек обнаружения проблем в анализе.

### [src/AIReviewSystem.Domain/Enums/AnalysisStatus.cs](../src/AIReviewSystem.Domain/Enums/AnalysisStatus.cs)
- Краткое описание: Состояния жизненного цикла сессии анализа.
- Логика реализации: Определяет допустимые статусы — черновик, выполняется, завершено, ошибка.

### [src/AIReviewSystem.Domain/Enums/ChangeType.cs](../src/AIReviewSystem.Domain/Enums/ChangeType.cs)
- Краткое описание: Типы изменений файла.
- Логика реализации: Нормализует виды изменений для последующей классификации изменённых файлов.

### [src/AIReviewSystem.Domain/Enums/SeverityLevel.cs](../src/AIReviewSystem.Domain/Enums/SeverityLevel.cs)
- Краткое описание: Уровни серьёзности статических находок.
- Логика реализации: Переводит результаты анализа в понятную шкалу от информационной проблемы до критической.

## 4. Infrastructure Layer

### [src/AIReviewSystem.Infrastructure/DependencyInjection.cs](../src/AIReviewSystem.Infrastructure/DependencyInjection.cs)
- Краткое описание: Конфигурация инфраструктурных сервисов.
- Логика реализации: Читает строку подключения из конфигурации, настраивает EF Core с Npgsql и регистрирует репозитории, UnitOfWork и контекст базы данных.

### [src/AIReviewSystem.Infrastructure/Persistence/ReviewDbContext.cs](../src/AIReviewSystem.Infrastructure/Persistence/ReviewDbContext.cs)
- Краткое описание: Контекст базы данных приложения.
- Логика реализации: Описывает наборы сущностей, задаёт таблицы и связи между ними, включая каскадное удаление для связанных объектов анализа.

### [src/AIReviewSystem.Infrastructure/Persistence/EfUnitOfWork.cs](../src/AIReviewSystem.Infrastructure/Persistence/EfUnitOfWork.cs)
- Краткое описание: Реализация Unit of Work на базе EF Core.
- Логика реализации: Делегирует сохранение изменений в контекст базы данных через метод SaveChangesAsync.

### [src/AIReviewSystem.Infrastructure/Repositories/EfAnalysisSessionRepository.cs](../src/AIReviewSystem.Infrastructure/Repositories/EfAnalysisSessionRepository.cs)
- Краткое описание: EF-реализация репозитория сессий.
- Логика реализации: Получает сессии вместе со снимком репозитория, возвращает последние сессии по времени запуска, добавляет новые сессии и обновляет уже существующие.

### [src/AIReviewSystem.Infrastructure/Repositories/EfStaticFindingRepository.cs](../src/AIReviewSystem.Infrastructure/Repositories/EfStaticFindingRepository.cs)
- Краткое описание: EF-реализация репозитория статических находок.
- Логика реализации: Получает находки по идентификатору сессии и добавляет набор находок пакетно в базу данных.

### [src/AIReviewSystem.Infrastructure/Repositories/EfReportArtifactRepository.cs](../src/AIReviewSystem.Infrastructure/Repositories/EfReportArtifactRepository.cs)
- Краткое описание: EF-реализация репозитория артефактов отчётов.
- Логика реализации: Ищет последний отчёт по сессии по времени создания и добавляет новый артефакт в хранилище.

## 5. Contracts и DTO

### [src/AIReviewSystem.Contracts/Requests/StartAnalysisRequest.cs](../src/AIReviewSystem.Contracts/Requests/StartAnalysisRequest.cs)
- Краткое описание: DTO запроса запуска анализа.
- Логика реализации: Передаёт путь к репозиторию в виде простого неизменяемого объекта.

### [src/AIReviewSystem.Contracts/Responses/AnalysisSessionResponse.cs](../src/AIReviewSystem.Contracts/Responses/AnalysisSessionResponse.cs)
- Краткое описание: DTO ответа по сессии анализа.
- Логика реализации: Формирует данные о сессии для внешнего слоя: идентификатор, путь, статус, время запуска и завершения, сводку.

### [src/AIReviewSystem.Contracts/Responses/ReportArtifactResponse.cs](../src/AIReviewSystem.Contracts/Responses/ReportArtifactResponse.cs)
- Краткое описание: DTO ответа по артефакту отчёта.
- Логика реализации: Передаёт сведения о формате отчёта, его местоположении, хеше и времени создания.

### [src/AIReviewSystem.Contracts/Common/AnalysisStatusDto.cs](../src/AIReviewSystem.Contracts/Common/AnalysisStatusDto.cs)
- Краткое описание: DTO статуса анализа.
- Логика реализации: Дублирует доменный enum статусов для удобства передачи данных через внешний контракт.

## 6. Текущее состояние реализации

Проект находится на ранней стадии MVP. Наиболее развитой частью является UI-скрипт анализа Git-изменений, а остальные части архитектуры уже заложены в виде абстракций, доменных сущностей и инфраструктурных адаптеров. Основные будущие шаги — реализация workflow-сервиса, полноценный анализ C# через Roslyn, сохранение результатов в базе данных и генерация отчётов.
