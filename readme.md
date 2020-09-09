This is telegram bot for learning english words

build, deploy
```
docker build -t learnbot -f LearnEnglishWordsBot/LearnEnglishWordsBot/Dockerfile .
docker run -d --name learnbot --network="host" image_id
```

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
    "CreateTasksToLearn": "0 0 9 ? * * *"
  }
}
```
