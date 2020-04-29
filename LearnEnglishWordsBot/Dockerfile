FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["LearnEnglishWordsBot/LearnEnglishWordsBot/LearnEnglishWordsBot.csproj", "LearnEnglishWordsBot/"]
COPY ["Bot.Core/Bot.Core/Bot.Core.csproj", "Bot.Core/"]

COPY . .

WORKDIR "/src/Bot.Core"
RUN dotnet restore "Bot.Core.csproj"
RUN dotnet build -c Release -o /app/build

WORKDIR "/src/LearnEnglishWordsBot"
RUN dotnet restore "LearnEnglishWordsBot.csproj"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final

ARG ASPNETCORE_ENVIRONMENT
ENV ASPNETCORE_ENVIRONMENT ${ASPNETCORE_ENVIRONMENT:-Production}
ENV TZ=Europe/Minsk

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LearnEnglishWordsBot.dll"]