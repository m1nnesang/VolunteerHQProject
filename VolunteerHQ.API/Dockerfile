FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY VolunteerHQ.Core/VolunteerHQ.Core.csproj VolunteerHQ.Core/
COPY VolunteerHQ.Infrastructure/VolunteerHQ.Infrastructure.csproj VolunteerHQ.Infrastructure/
COPY VolunteerHQ.API/VolunteerHQ.API.csproj VolunteerHQ.API/
RUN dotnet restore VolunteerHQ.API/VolunteerHQ.API.csproj
COPY VolunteerHQ.Core/ VolunteerHQ.Core/
COPY VolunteerHQ.Infrastructure/ VolunteerHQ.Infrastructure/
COPY VolunteerHQ.API/ VolunteerHQ.API/
RUN dotnet publish VolunteerHQ.API/VolunteerHQ.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "VolunteerHQ.API.dll"]
