FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj and restore as distinct layers
COPY KairusBot.csproj ./
RUN dotnet restore "./KairusBot.csproj"

# copy everything else and publish
COPY . ./
RUN dotnet publish "./KairusBot.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Render will route traffic to the port defined by $PORT.
# The app also defaults to 10000 if $PORT is not set.
ENV PORT=10000

COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "KairusBot.dll"]

