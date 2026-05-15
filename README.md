# Gems Issue Notifier

Сервис уведомлений техподдержки об изменениях в данных заявок Okdesk

## Разделы конфигурации

- [Okdesk](#Okdesk)
- [Telegram](#Telegram)
- [Masking](#Masking)
- [Логирование](#Logging)



## Okdesk

1. [Список возвращаемых атрибутов заявки](#fields)
2. [Ограничение количества запросов](#requests-limit)
3. [Интервал запроса заявок](#request-interval)
4. [Интервал агрегации событий](#aggregation-interval)
5. [Фильтры запросов](#filters)
6. [Шаблоны сообщений](#message-templates)
7. [Активация бизнес процессов](#features)
###  <a id="fields">Список возвращаемых атрибутов заявки</a>

Поле *Fields* отвечает за то, какие параметры заявки будет получать система при каждом запросе пакета заявок. На данный момент список атрибутов содержит: **id, title, created_at, deadline_at, updated_at, completed_at, status, type, priority, company, contact, assignee.**

### <a id="requests-limit">Ограничение количества запросов</a>

Поле *RequestsPerSecondLimit* предназначено для установление ограничения на число запросов, отправляемых к системе Okdesk в секунду.

### <a id="request-interval">Интервал запроса заявок</a>

Поле *IssuesRequestIntervalInMinutes* отвечает за интервал запроса заявок в минутах.

### <a id="aggregation-interval">Интервал агрегации событий</a>

Секция *OutboxMessages* содержит следующие поля для конфигурациия агрегации:
- *IntervalInHoursForGreetings* - Через сколько часов снова можно добавлять приветственное сообщение по одной и той же заявке,
- *ProcessIntervalInSecondsForDomainEvent* - Как часто, в секундах, система проверяет и обрабатывает обычные события по заявкам,
- *ProcessIntervalInSecondsForCommentAggregator* - Как часто, в секундах, система собирает комментарии/события по заявке в одно сообщение,
- *ProcessMessagesBatchSize* -  Сколько сообщений система берёт в обработку за один запуск,
- *RetryCount* - Сколько раз система повторит обработку или отправку, если с первого раза произошла ошибка

### <a id="filters">Фильтры запросов</a>

Доступна конфигурация фильтров при формировании пакета заявок. 

- *FilterByCompanyIds*
- *FilterByAssigneeIds*
- *FilterByContactIds*
- *FilterByStatuses*

Для добавления статусов в фильтр по статусам нужно добавить коды нужных статусов:

<table>
    <tr>
        <th>Статус заявки</th>
        <th>Код</th>
    </tr>
    <tr>
        <td>Новая</td>
        <td>opened</td>
    </tr>
    <tr>
        <td>В работе</td>
        <td>inwork</td>
    </tr>
    <tr>
        <td>На внедрении</td>
        <td>vnedrenie</td>
    </tr>
    <tr>
        <td>На партнерах</td>
        <td>partner</td>
    </tr>
    <tr>
        <td>Ожидание ответа</td>
        <td>waiting</td>
    </tr>
    <tr>
        <td>На тестировщиках</td>
        <td>testirovanie</td>
    </tr>
    <tr>
        <td>На аналитиках</td>
        <td>analytics</td>
    </tr>
    <tr>
        <td>На реквестах</td>
        <td>request</td>
    </tr>
    <tr>
        <td>На рассмотрении у РО</td>
        <td>jira_po</td>
    </tr>
    <tr>
        <td>В бэклоге</td>
        <td>backlog</td>
    </tr>
    <tr>
        <td>Разработка</td>
        <td>development</td>
    </tr>
    <tr>
        <td>Ожидание обновления</td>
        <td>waitingforupdate</td>
    </tr>
    <tr>
        <td>Пожелание зафиксировано</td>
        <td>wish</td>
    </tr>
    <tr>
        <td>Отложена</td>
        <td>delayed</td>
    </tr>
    <tr>
        <td>Решена</td>
        <td>completed</td>
    </tr>
    <tr>
        <td>Закрыта</td>
        <td>closed</td>
    </tr>
</table>

### <a id="message-templates">Шаблоны сообщений</a>

Поле *MessageTemplates* содержит шаблоны сообщений, которые отправляются клиенту Okdesk через добавление комментария к измененной заявке.

- *StatusUpdated* - сообщение при изменении статуса заявки.
- *DeadLineUpdated* - сообщение при изменении срока решения заявки.
- *PriorityUpdatedToHighest* - сообщение при изменении приоритета на "Наивысший".
- *PriorityUpdatedToHigh* - сообщение при изменении приоритета на "Высокий".
- *PriorityUpdatedToNormal* - сообщение при изменении приоритета на "Обычный".
- *PriorityUpdatedToLow* - сообщение при изменении приоритета на "Низкий".
- *IssueCompleted* - сообщение при изменении статуса заявки на "Решена", содержит HTML-разметку и изображение для оценки работы тех поддержки.

Следующие шаблоны сообщений используются отправки клиентам уведомлений с ориентировочными сроками решения (после принятия заявки в работу)

- *Consultation* - для заявки типа "Консультация".
- *Service* - для заявки типа "Сервисный запрос".
- *IncidentHighest* - для заявки типа "Инцидент" с приоритетом "Наивысший".
- *IncidentHigh* - для заявки типа "Инцидент" с приоритетом "Высокий".
- *IncidentNormal* - для заявки типа "Инцидент" с приоритетом "Обычный".
- *IncidentLow* - для заявки типа "Инцидент" с приоритетом "Низкий".

#### <a id="features">Активация бизнес процессов</a>

Система предоставляет возможность отключения каждого блока бизнес-функционала. За это отвечает поле *FeatureManagement*. Для этого необходимо установить флаг *true* или *false*.

<table>
    <tr>
        <th>Поле</th>
        <th>Бизнес процесс</th>
    </tr>
    <tr>
        <td>IssueCompletedEventEnabled</td>
        <td>Запрос на получение обратной связи</td>
    </tr>
    <tr>
        <td>IssueCommentCreatedEventEnabled</td>
        <td>Оповещение инженера технической поддержки об изменениях в заявке: появление нового комментария от клиента</td>
    </tr>
    <tr>
        <td>IssueDeadlineUpdatedEventEnabled</td>
        <td>Оповещение клиента об изменениях в параметрах заявки: изменение срока решения заявки</td>
    </tr>
    <tr>
        <td>IssuePriorityUpdatedEventEnabled</td>
        <td>Оповещение инженера технической поддержки об изменениях в заявке (если приоритет был изменён клиентом). Оповещение клиента об изменениях в параметрах заявки (если приоритет был изменён сотрудником технической поддержки).</td>
    </tr>
    <tr>
        <td>IssueStatusUpdatedEventEnabled</td>
        <td>Оповещение клиента об изменениях в параметрах заявки: изменение статуса заявки</td>
    </tr>
    <tr>
        <td>IssueDeadlineNotificationEventEnabled</td>
        <td>Уведомление клиента о сроках решения заявки</td>
    </tr>
    <tr>
        <td>SkitIssuesProcessingEnabled</td>
        <td>Обработка заявок СКИТ</td>
    </tr>
    <tr>
        <td>IssueProblemPostCommentEventEnabled</td>
        <td>Обработка заявок СКИТ</td>
    </tr>
     <tr>
        <td>IssueAutoCompletedEventEnabled</td>
        <td>Авто-закрытие заявок по проблеме</td>
    </tr>
     <tr>
        <td>IssueProblemPostCommentEventEnabled</td>
        <td>Создание комментария при авто-закрытии заявки</td>
    </tr>
</table>

## Telegram

1. [Создание Telegram бота](#telegram-bot)
2. [Получение chat ID и thread ID для группы, разделенной на темы](#chatid-with-themes)
3. [Получение chat ID и thread ID для группы, не разделенной на темы](#chatid-with-themes)
4. [Шаблоны оповещений](#telegram-notifications)
5. [Другие параметры](#parameters)

### <a id="telegram-bot">Создание Telegram бота</a>
[Официальная документация по созданию и настройке бота в Telegram](https://core.telegram.org/bots/features#botfather)

- Отправьте [@BotFather](https://t.me/botfather) команду `/newbot` для создания нового бота
- Введите имя бота, которое будет отображается в профиле бота и при отправке ботом сообщений.
- Введите имя пользователя бота - короткое имя, используемое в поиске, упоминаниях и t.me ссылках. Имя пользователя может содержать от 5 до 32 символов, содержит только  латинские буквы, цифры и символы подчёркивания. Имя пользователя бота должно оканчиваться на "bot" (например, TetrisBot или tetris_bot). Имя пользователя должно быть уникальным для бота.
- Токен бота - это строка вида `110201543:AAHdqTcvCH1vGWJxfSeofSAs0K5PALDsaw`.Токен используется для управления ботом и отправки запросов к API бота. Токен должен храниться в закрытом доступе.
- Для более подробной настройки бота используйте команду `/mybots`, из списка ботов выберете имя пользователя нужного бота.

Создание и настройку Telegram бота также можно производить в меню ботов [@BotFather](https://t.me/botfather). Для перехода в меню ботов нажмите `Open` в чате [@BotFather](https://t.me/botfather)

### <a id="chatid-with-themes">Получение chat ID и thread ID для группы, разделенной на темы</a>

- Добавьте Telegram бота в группу.
- Перейдите в профиль темы, в которую бот будет отправлять уведомления.
- Ссылка на тему имеет вид `t.me/c/xxxxxxxxxx/yy`.
- `-100xxxxxxxxxx` - chat ID группы.
- `yy` - thread ID темы.

### <a id="chatid-without-themes">Получение chat ID и thread ID для группы, не разделенной на темы</a>

- Добавьте Telegram бота в группу.
- Отправьте в группу произвольное сообщение боту.
Например `/my_message @my_bot_username`.
- Перейдите по ссылке `https://api.telegram.org/botXXX:YYYY/getUpdates`.
Замените XXX:YYYY на токен бота.
- Найдите `"chat":{"id":-zzzzzzzzzz,...`
- `-zzzzzzzzzz` - chat ID группы.


### <a id="telegram-notifications">Шаблоны оповещений</a>

Система предоставляет возможность конфигурировать шаблоны оповещений, приходящих в Telegram. Для этого используются следующие поля:

- *IssueCommentCreatedMessageTemplate* - шаблон оповещения инженера технической поддержки о появлении нового комментария в заявке от клиента.
- *IssuePriorityUpdatedMessageTemplate* - шаблон оповещения инженера технической поддержки об обновлении приоритета заявки от клиента.
- *StaleIssueNotificationMessageTemplate* - шаблон оповещения инженера технической поддержки о заявке без новых комментариев от инженера технической поддержки в течение длительного времени

### <a id="parameters">Другие параметры</a>

- *Asignee username* - сопоставляет ID инженера технической поддержки в Okdesk с его username в Telegram.   Если оповещение отправляется по заявке, у которой ответственный сотрудник указан в *Assignee username*, то в оповещении будет использован его username.
- *MaxCommentLength* - максимальная длина комментария Oksesk, который будет отображен в оповещении (в символах). Если комментарий превышает указанную длину, он будет обрезан. Максимальное значение - *4096* символов.

## Инструкция для разработчиков для запуска:
1. Склонировать репозиторий и перейти в корень проекта
2. Установить .NET 10.0 SDK
3. docker compose up -d
4. $env:DOTNET_ENVIRONMENT="Development"
5. $env:CONSUL_KEY="Gems.TechSupportIssueNotifier/appsettings.Development.json" 
6. $env:CONSUL_HOST="http://localhost:8500/"
7. dotnet ef database update --project .\Gems.TechSupport.Persistence\ --startup-project .\Gems.TechSupport\
P.S запуск команды для миграций происходит из корня проекта
8. Запуск проекта из Visual Studio/Rider или командой `dotnet run --project .\Gems.TechSupport\` из корня проекта

## Logging
1. [Уровни логирования](#minimum-levels)
2. [Вывод логов](#log-output)

Приложение использует `Serilog` для записи логов.

### <a id="minimum-levels">Уровни логирования</a>

Базовый уровень логирования — `Debug`.

Переопределенные уровни:
<table>
    <tr>
        <th>Microsoft</th>
        <th>Information</th>
    </tr>
    <tr>
        <td>Microsoft.AspNetCore</td>
        <td>Information</td>
    </tr>
    <tr>
        <td>Microsoft.EntityFrameworkCore</td>
        <td>Information</td>
    </tr>
    <tr>
        <td>System</td>
        <td>Warning</td>
    </tr>
    <tr>
        <td>Polly</td>
        <td>Information</td>
    </tr>
    <tr>
        <td>Quartz</td>
        <td>Warning</td>
    </tr>
</table>

### <a id="log-output">Вывод логов</a>

В консоль выводятся все логи **кроме** сообщений по `Webhooks`.

Логи по `Webhooks` записываются в файл

Параметры файла:
- path - путь к файлу логов
- rollingInterval - период, по которому создаются новые файлы логов  
- retainedFileCountLimit - количесвто хранимых файлов логов
- fileSizeLimitBytes -  максимальный размер одного файла логов
- rollOnFileSizeLimit - при превышении размера создаётся новый файл

## Masking
В сервисе реализован механизм анонимизации клиентов.

По умолчанию обращение к клиенту в комментариях осуществляется по значению поля **Имя** из Okdesk.

Если полное имя клиента содержит слова, указанные в поле *Keywords*, то в комментариях используется его **полное имя** вместо сокращённого обращения.