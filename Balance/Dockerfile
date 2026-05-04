FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Balance/Balance.csproj", "Balance/"]
RUN dotnet restore "Balance/Balance.csproj"
COPY . .
WORKDIR "/src/Balance"
RUN dotnet build "Balance.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Balance.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Balance.dll"]
