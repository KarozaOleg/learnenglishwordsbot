# Telegram bot for learning english words
Based on library [Telegram.Bot](https://github.com/TelegramBots/Telegram.Bot)
## Logic of work
- the bot has different sets, each set has n amount of pairs
- each pair in set represent as a pair of two words "Engish version"+"Other language version"
- user are able to choose sets for learning
- learning based on everyday repeating, at the every morning bot will send a 10 pairs which user have to translate and choose correct version
- pair considering as learned when user will choose 3 times correct answer
- set consiring as learned when all pairs in set are learned
## Check it online
- find the bot in telegram search - [@LearnEnglishW0rdsBot](https://t.me/LearnEnglishW0rdsBot)
- send `/start` message
# Usage notes
## Create new set
in progress...
## Create new pair on set
in progress...
## Force launch of scheduled task
send `GET` request on address
```
https://YOUR_BOT_INSTANCE_URL/createTasksToLearn
```
# Deploy notes
## Rollout database structure
Use `database/scripts/initialize_db.sql` script
## Check port number
appsettings.json
```
 "Kestrel": {
    "EndPoints": {
      "Http": {
        "Url": "http://*:80"
      }
    }
  }
```
## Setup database connection and schedule
appsettings.*.json example
```
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=localhost;Port=5432;Database=dbname;Username=user;Password=password"
  },
  "BotSettings": {
    "BotToken": "",
    "Socks5Host": "",
    "Socks5Port": 9276,
    "Socks5Username": "",
    "Socks5Password": ""
  },
  "JobTriggersSettigns": {
    "CreateTasksToLearnJob": "0 0 9 ? * * *"
  }
}
```
## Set webhook
```
https://api.telegram.org/botYOUR_BOT_TOKEN/setWebhook?url=YOUR_ENDPOINT_URL
```
## Build, deploy
```
docker build -t learnbot -f LearnEnglishWordsBot/Dockerfile .
docker run -d --name learnbot --network="host" image_id
```