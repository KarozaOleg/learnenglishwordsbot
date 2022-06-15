FROM mcr.microsoft.com/dotnet/core/sdk:3.1-bionic AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-bionic AS build
WORKDIR /src
COPY ["LearnEnglishWordsBot/LearnEnglishWordsBot.csproj", "LearnEnglishWordsBot/"]

COPY . .

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