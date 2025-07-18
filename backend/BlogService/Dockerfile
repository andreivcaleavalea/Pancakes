FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["BlogService.csproj", "."]
RUN dotnet restore "BlogService.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "BlogService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BlogService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV ASPNETCORE_URLS=http://+:5001
ENTRYPOINT ["dotnet", "BlogService.dll"]
