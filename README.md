# watchdog-browser
This application based on CEFSharp. Made on C# + WPF. It is a special browser and it's only purpose is to be reliable. This application made for one certain organization, so, all the documentation, comments, resources are in russian. Can be used as real life example of using CEFSharp.wpf with MVVM pattern. Solved wpf TabControl reloading trouble.


# Браузер Кобра Гарант - мониторинг
Приложение разработано для организации Кобра Гарант. Представляет веб-браузер на основе компонента CEFSharp.wpf с поддержкой вкладок и отслеживания состояния веб-ресурса.

Выбор CEFSharp обоснован тем, что он поддерживает 2way ssl


## Сборка:
1. Установите [Visual Studio 2015](https://www.visualstudio.com/ru/vs/community/)
2. Клонируйте репозиторий, или скачайте в виде ZIP архива.
3. Откройте проект в Visual Studio 2015.
4. Щелкните правой кнопкой по списку зависимостей, откройте менеджер NuGet и согласитесь восстановить пакеты
5. Нажмите F5

## Использование: 
+ Соберите приложение из исходных кодов.
+ Добавьте, если нужно, звуки предупреждения и ошибки, желательно в формате pcm wav, можно попробовать другие, поддержка не гарантирована
+ Если необходима поддержка отслеживания состояния веб ресурса, реализуйте js интерфейс, описан ниже.
+ Создайте файл настроек, по умолчанию он должен называться config.xml и лежать в папке с исполняемым файлом, путь и название можно изменить в файле WatchDogBrowser.exe.config, или в окне настроек приложения в Visual Studio

## JavaScript интерфейс
Для управления браузером доступен яваскрипт интерфейс. Если веб ресурс открыт в браузере, ему доступен объект cobraMonitor, у которого есть несколько методов.

 ```javascript

    var username = cobraMonitor.getUsername();//Возвращает имя пользователя из файла настроек браузера
    var password = cobraMonitor.getPassword();//Возвращает пароль пользователя из файла настроек браузера

    cobraMonitor.closeTab(url);//Отдаёт команду браузеру на закрытие вкладки
    cobraMonitor.setAlarm(integer);/*-1,0 или 1, другие значения принимаются равными единице, меняет цвет заголовка вкладки -1 - черный, 0 - зелёный, 1 - красный*/

    cobraMonitor.heartbeat(); /*необходимо вызывать в пределах определённого в настройках интервала времени, если включено отслеживание, если не вызвать, то будет выведена ошибка, а браузер будет пытаться перезагрузить страницу, или переключить зеркало*/

 ```

## Файл настроек

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<sites>
   <site 
     name="Заголовок страницы во время загрузки" 
     intervalHeartbeat="12"      
     intervalMirror="30" 
     intervalPageLoad="20"
     alertDelayTime="3"
     username="login" 
     password="pwd123" 
     watched="true" 
     message="Если подключение не восстановилось за десять минут, позвоните родителям"
     warningSoundPath="Sounds\alert_in_browser.wav"
     errorSoundPath="Sounds\alert_in_browser.wav">
       <mirrors>
           <mirror domain="mirror1.site.ru" protocol="https"/>
		   <mirror domain="mirror2.site.ru" protocol="https"/>
       </mirrors>
   </site>
</sites>
```
Обязательных настроек нет, любая может быть пропущена, кроме раздела **mirrors**

**username="login"** - имя пользователя, полезно для автоматической авторизации терминала

**password="pwd123"** - пароль, полезно для автоматической авторизации терминала

**watched="true"** - включено ли отслеживание страницы, может принимать значения **true**, или **false**.

**Настройки ниже зависят от предыдущей:**

**intervalHeartbeat="12"** - интервал в секундах, в пределах которого ожидается _heartbeat_ из js интерфейса, если не успевает, то происходит перезагрузка страницы

**intervalMirror="30"** - интервал в секундах, если не будет _heartbeat_ в пределах этого времени, то будет переключено зеркало.

**intervalPageLoad="20"** - таймаут начальной загрузки страницы в секундах, если в его пределах не будет _heartbeat_, то будет произведена перезагрузка, или переключение зеркала

**alertDelayTime="3"** - задержка(в секундах) воспроизведения звука ошибки после появления сообщения, полезно, чтобы не нервировать операторов.

**message="Сообщение для оператора"** - сообщение, которое будет показано в случае таймаута по _heartbeat_

**warningSoundPath="Sounds\alert_in_browser.wav"** - путь к файлу со звуком предупреждения, воспроизводится при смене зеркала

**errorSoundPath="Sounds\alert_in_browser.wav"** - путь к файлу со звуком ошибки, воспроизводится при появлении окна ошибки


