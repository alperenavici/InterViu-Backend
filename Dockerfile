FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Solution ve project dosyalarını kopyala
COPY *.sln .
COPY */*.csproj ./

# Proje yapısını yeniden oluştur
RUN for file in $(ls *.csproj); do mkdir -p ${file%.*}/ && mv $file ${file%.*}/; done

# NuGet paketlerini restore et
RUN dotnet restore

# Tüm kaynak kodu kopyala
COPY . .

# Uygulamayı build et ve publish et
RUN dotnet publish -c Release -o out

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 80

# Ana proje DLL'ini belirtin (Web API/MVC projesi)
ENTRYPOINT ["dotnet", "Interviu.WebApi.dll"]