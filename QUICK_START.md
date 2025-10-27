# 🚀 Gemini AI Entegrasyonu - Hızlı Başlangıç

## ⚡ 3 Adımda Test Et

### 1️⃣ API'yi Başlat
```bash
cd /Users/alperenavci/RiderProjects/Interviu/Interviu.WebApi
dotnet run
```

API şu adreste çalışacak: `http://localhost:5000`

### 2️⃣ Postman Collection'ı Import Et

1. Postman'i aç
2. **Import** butonuna tıkla
3. `Interviu_API.postman_collection.json` dosyasını seç
4. Collection import edildi! ✅

### 3️⃣ Test Et

**Sırayla çalıştır:**

1. **Login** - Token al ✅
2. **Start Interview with CV (Gemini AI)** - CV yükle 🤖
   - Variables sekmesinde `userId` değerini güncelle
   - Body'de CV dosyası seç
3. **Get Interview Details** - Gemini'nin ürettiği soruları gör 📝

---

## 📝 Gemini API Key Kontrolü

`appsettings.Development.json` dosyasını kontrol edin:

```json
{
  "Gemini": {
    "ApiKey": "AIzaSyCrYHO0-5RReczPK37vsYPEGcaxHLOih8o",  // ✅ Mevcut
    "ModelId": "gemini-1.5-flash"
  }
}
```

✅ **API Key hazır!**

---

## 🧪 Test CV Hazırlama

Basit bir test CV'si oluşturun:

### test_cv.pdf veya test_cv.docx

```
ALİ YILMAZ
Senior Backend Developer

DENEYIM:
- 6 yıl .NET Core ile backend geliştirme
- Mikroservis mimarisi tasarımı ve implementasyonu
- RESTful API geliştirme
- PostgreSQL, MongoDB veritabanı yönetimi
- Docker, Kubernetes ile container orchestration

TEKNİK YETKİNLİKLER:
- C#, .NET 8, ASP.NET Core
- Entity Framework Core
- Clean Architecture, SOLID prensipleri
- Git, CI/CD
- Azure, AWS cloud platformları

EĞİTİM:
- Bilgisayar Mühendisliği (2015-2019)

PROJELER:
- E-ticaret platformu (mikroservis mimarisi)
- Finans sektörü için ödeme sistemi
- CRM yazılımı backend altyapısı
```

**Bu CV'yi kullandığınızda Gemini AI şöyle sorular üretecek:**

✅ "Mikroservis mimarisindeki en büyük zorluklarınız nelerdi?"
✅ "Clean Architecture prensiplerini projelerinizde nasıl uyguladınız?"
✅ ".NET 8'deki yeni özellikleri kullandınız mı?"
✅ "Container orchestration'da karşılaştığınız sorunlar ve çözümler?"

---

## 🎯 Beklenen Sonuç

**Postman'de şunu göreceksiniz:**

```json
{
  "id": "3fa85f64-...",
  "position": "Senior Backend Developer",
  "status": "ONGOING",
  "interviewQuestions": [
    {
      "question": {
        "questionText": "Can you describe your experience with microservices...",
        "category": "Technical",
        "difficulty": "Medium"
      }
    }
    // ... toplam 10 soru
  ]
}
```

---

## 📚 Detaylı Döküman

- 📮 **Postman Guide**: `POSTMAN_GUIDE.md`
- 📖 **Tam Döküman**: `README.md`

---

## ❓ Sorun mu var?

### API başlamıyor
```bash
# Paketleri restore et
dotnet restore

# Veritabanını güncelle
dotnet ef database update --project Interviu.Data --startup-project Interviu.WebApi
```

### Gemini API hatası
- API key'i kontrol edin
- Google AI Studio quota'sını kontrol edin: https://aistudio.google.com/

### 401 Unauthorized
- Login endpoint'ini çağırın
- Token'ı Authorization header'a ekleyin

---

## 🎉 Başarı!

Sistem çalışıyor ve Gemini AI sorular üretiyor! 🚀🤖

**İyi testler!** 

