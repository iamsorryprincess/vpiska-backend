# vpiska-backend
____

## WebSocket эвента
____

Url для подключения - **ws://185.189.167.147:5000/event?access_token=qweasd&eventId=E9F6D9A2-2FF4-4A15-96EB-7C13F47F9CA8**    
access_token - jwt token юзера, если он есть, если нет (юзер не зареган), то не передавать параметр токена    
eventId - id эвента    

### сообщения приходящие с бэка
____

**usersCountUpdated/6**: 
обновление кол-ва юзеров в евенте, после слэша идет число юзеров в эвенте
____

**chatMessage/{"userId": "qweasd", "userName": "qweasd", "userImageId": "qweasd", "message": "lol ahaha"}**: 
сообщение в чате, после слэша json
____

**mediaAdded/E9F6D9A2-2FF4-4A15-96EB-7C13F47F9CA8**: 
был добавлен медиа контент (фото или видео), после слэша идет id медиа контента в firebase
____

**mediaRemoved/E9F6D9A2-2FF4-4A15-96EB-7C13F47F9CA8**: 
был удален медиа контент, после слэша идет id медиа контента в firebase
____

**closeEvent/**: 
эвент был закрыт, после слэша ничего нет, чисто нотификация что эвент был закрыт, после этого бэк разрывает соединение
____

### сообщения для отправки в бэк
____

**chatMessage/message**: 
сообщение в чат где message - строка
____

## WebSocket range событий
____

Url для подключения - **ws://185.189.167.147:5000/range**

### сообщения приходящие с бэка
____

**eventUpdated/{"eventId": "qweasd", "address": "moskvagovno", "usersCount": 2, "coordinates": {"x": 24.03441142, "y": 24.03441142}}**: 
обновление инфы по эвенту, после слэша json
____

**eventCreated/{"eventId": "qweasd", "name": "test" "usersCount": 2, "coordinates": {"x": 24.03441142, "y": 24.03441142}}**: 
создан эвент, после слэша json
____

**eventClosed/E9F6D9A2-2FF4-4A15-96EB-7C13F47F9CA8**: 
эвент закрыт, после слэша id эвента

### сообщения для отправки в бэк
____

**changeRange/{horizontalRange: 0.78003213, verticalRange: 0.9722341, coordinates: {x: 0, y: 0}}**: 
изменение позиции или рэнджа чела
____
