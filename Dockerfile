# שלב 1: בנייה (Build)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# העתקת קבצי הפרויקט ושחזור חבילות (NuGet)
COPY . ./
RUN dotnet restore

# בנייה ופרסום של האפליקציה
RUN dotnet publish -c Release -o out

# שלב 2: הרצה (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# הגדרת הפורט ש-Railway מצפה לו
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# הרצת האפליקציה (שימי לב לשם ה-DLL שלך!)
ENTRYPOINT ["dotnet", "BinyanAv.PublicGateway.dll"]