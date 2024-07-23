FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["CloudCtrl.Kubernetes.Agent.csproj", "./"]
RUN dotnet restore "CloudCtrl.Kubernetes.Agent.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "CloudCtrl.Kubernetes.Agent.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CloudCtrl.Kubernetes.Agent.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CloudCtrl.Kubernetes.Agent.dll"]
