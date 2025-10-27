# 📮 Interviu API - Postman Test Guide

Bu döküman, Gemini AI entegrasyonlu Interviu API'sini Postman ile test etmeniz için hazırlanmıştır.

## 🚀 Başlangıç

### Base URL
```
http://localhost:5000
```
veya
```
https://localhost:5001
```

---

## 📝 Test Sırası

### 1️⃣ **Giriş Yapma (Login) - Token Alma**

**Endpoint:** `POST /api/Auth/login`

**Headers:**
```
Content-Type: application/json
```

**Body (JSON):**
```json
{
  "email": "admin@example.com",
  "password": "Admin123!"
}
```

**Expected Response (200 OK):**
```json
{
  "access_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expires_in": 3600
}
```

**⚠️ ÖNEMLİ:** `access_token` değerini kopyalayın, sonraki isteklerde kullanacaksınız!

---

### 2️⃣ **CV ile Mülakat Başlatma (Gemini AI) - ANA TEST**

**Endpoint:** `POST /api/Interviews/start-with-cv`

**Headers:**
```
Authorization: Bearer YOUR_ACCESS_TOKEN_HERE
Content-Type: multipart/form-data
```

**Body (form-data):**
| Key | Type | Value | Required |
|-----|------|-------|----------|
| `cvFile` | File | `[Bir PDF veya DOCX dosyası seçin]` | ✅ Yes |
| `position` | Text | `Senior Backend Developer` | ✅ Yes |
| `questionCount` | Text | `10` | ❌ No (default: 10) |

**💡 NOT:** `userId` parametresi **GEREKLİ DEĞİL**! UserId otomatik olarak giriş yapmış Admin kullanıcısından (JWT token'dan) alınır.

**Postman'de Form-Data Nasıl Ayarlanır:**

1. **Body** sekmesine gidin
2. **form-data** seçin
3. Key'leri ekleyin:
   - `cvFile`: Type'ı **File** seçin, Browse ile CV dosyanızı yükleyin
   - `position`: Type **Text** olarak bırakın, değer girin (örn: "Senior Backend Developer")
   - `questionCount`: Type **Text** olarak bırakın, değer girin (isteğe bağlı, varsayılan: 10)

**Expected Response (201 Created):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "position": "Senior Backend Developer",
  "userId": "user-id-here",
  "cvId": null,
  "status": "ONGOING",
  "startedAt": "2024-01-15T10:30:00Z",
  "completedAt": null,
  "overallScore": null,
  "overallFeedback": null,
  "interviewQuestions": [
    {
      "questionId": "q1-guid",
      "question": {
        "id": "q1-guid",
        "questionText": "Can you explain your experience with microservices architecture?",
        "category": "Technical",
        "difficulty": "Medium",
        "createdAt": "2024-01-15T10:30:00Z"
      },
      "answerText": null
    }
    // ... daha fazla soru
  ]
}
```

**Bu İstek Ne Yapar?**
1. ✅ CV dosyasını okur (PDF/DOCX)
2. ✅ CV metnini çıkarır
3. ✅ **Gemini AI'a gönderir**
4. ✅ Pozisyona ve CV'ye özel **10 soru üretir**
5. ✅ Soruları veritabanına kaydeder
6. ✅ Mülakatı başlatır

---

### 3️⃣ **Mülakat Detaylarını Getirme**

**Endpoint:** `GET /api/Interviews/{interviewId}`

**Headers:**
```
Authorization: Bearer YOUR_ACCESS_TOKEN_HERE
```

**URL Parametreleri:**
- `{interviewId}`: Mülakat ID'si (önceki response'dan aldığınız `id` değeri)

**Örnek URL:**
```
GET /api/Interviews/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

**Expected Response (200 OK):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "position": "Senior Backend Developer",
  "status": "ONGOING",
  "interviewQuestions": [
    {
      "questionId": "q1-guid",
      "question": {
        "questionText": "Gemini AI tarafından üretilen soru...",
        "category": "Technical",
        "difficulty": "Medium"
      },
      "answerText": null
    }
  ]
}
```

---

### 4️⃣ **Soruya Cevap Gönderme**

**Endpoint:** `POST /api/Interviews/submit-answer`

**Headers:**
```
Authorization: Bearer YOUR_ACCESS_TOKEN_HERE
Content-Type: application/json
```

**Body (JSON):**
```json
{
  "interviewId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "questionId": "q1-guid",
  "answerText": "I have 5 years of experience working with microservices..."
}
```

**Expected Response (204 No Content)**
- Başarılı ise body boş döner

---

### 5️⃣ **Mülakatı Tamamlama**

**Endpoint:** `POST /api/Interviews/{interviewId}/complete`

**Headers:**
```
Authorization: Bearer YOUR_ACCESS_TOKEN_HERE
Content-Type: application/json
```

**URL Parametreleri:**
- `{interviewId}`: Tamamlanacak mülakat ID'si

**Body (JSON):**
```json
{
  "overallScore": 85.5,
  "overallFeedback": "Candidate demonstrated strong technical skills and good problem-solving abilities."
}
```

**Expected Response (200 OK):**
```json
{
  "message": "Mülakat başarıyla tamamlandı."
}
```

---

### 6️⃣ **Kullanıcının Tüm Mülakatlarını Listeleme**

**Endpoint:** `GET /api/Interviews/my-interviews`

**Headers:**
```
Authorization: Bearer YOUR_ACCESS_TOKEN_HERE
```

**Expected Response (200 OK):**
```json
[
  {
    "id": "interview-id-1",
    "position": "Senior Backend Developer",
    "status": "ONGOING",
    "startedAt": "2024-01-15T10:30:00Z",
    "overallScore": null
  },
  {
    "id": "interview-id-2",
    "position": "Frontend Developer",
    "status": "COMPLETED",
    "startedAt": "2024-01-14T09:00:00Z",
    "overallScore": 85.5
  }
]
```

---

## 🔐 Authorization Nasıl Ayarlanır?

### Postman'de Token Kullanımı:

1. **Authorization** sekmesine gidin
2. **Type:** `Bearer Token` seçin
3. **Token:** Login'den aldığınız `access_token` değerini yapıştırın

### Alternatif: Collection Level Authorization

Tüm istekler için tek seferde token ayarlamak:

1. Collection'a sağ tık > **Edit**
2. **Authorization** sekmesi
3. **Type:** `Bearer Token`
4. **Token:** `{{authToken}}` (variable kullanarak)
5. Login response'ında otomatik set etmek için **Tests** sekmesine:

```javascript
if (pm.response.code === 200) {
    var jsonData = pm.response.json();
    pm.collectionVariables.set("authToken", jsonData.access_token);
}
```

---

## 🧪 Test Senaryosu

### Tam Flow Testi:

```
1. POST /api/Auth/login
   ↓ (Token'ı al)
   
2. POST /api/Interviews/start-with-cv
   ↓ (CV dosyası + position + userId gönder)
   ↓ (Interview ID'yi al)
   
3. GET /api/Interviews/{interviewId}
   ↓ (Gemini'nin ürettiği soruları gör)
   
4. POST /api/Interviews/submit-answer
   ↓ (Her soru için cevap gönder)
   
5. POST /api/Interviews/{interviewId}/complete
   ↓ (Mülakatı tamamla)
   
6. GET /api/Interviews/my-interviews
   ↓ (Tüm mülakatları listele)
```

---

## 📁 Test CV Dosyası Hazırlama

Basit bir test CV'si (PDF veya DOCX):

```
JOHN DOE
Senior Software Engineer

EXPERIENCE:
- 5 years of backend development with .NET Core
- Microservices architecture and cloud deployment (AWS, Azure)
- RESTful API design and implementation
- Database design (PostgreSQL, MongoDB)

SKILLS:
- C#, .NET 8, Entity Framework
- Docker, Kubernetes
- CI/CD pipelines
- Agile/Scrum methodologies

EDUCATION:
- Bachelor's in Computer Science

PROJECTS:
- Built scalable e-commerce platform handling 1M+ requests/day
- Implemented microservices migration for legacy monolith
```

Bu içeriği bir Word veya PDF dosyasına kaydedin ve test için kullanın.

---

## ❗ Sık Karşılaşılan Hatalar

### 1. `401 Unauthorized`
**Neden:** Token geçersiz veya eksik
**Çözüm:** Login yapıp yeni token alın

### 2. `Gemini:ApiKey configuration is missing`
**Neden:** API key ayarlanmamış
**Çözüm:** `appsettings.Development.json` dosyasında API key'i kontrol edin

### 3. `400 Bad Request - CV dosyası gereklidir`
**Neden:** cvFile parametresi gönderilmemiş
**Çözüm:** Body'de form-data ile dosya ekleyin

### 4. `Gemini AI'dan soru üretilemedi`
**Neden:** API key hatalı veya quota aşımı
**Çözüm:** 
- Google AI Studio'dan API key'i kontrol edin
- Quota'yı kontrol edin: https://aistudio.google.com/

---

## 🎯 Başarı Kriterleri

✅ Login başarılı ve token alındı
✅ CV dosyası yüklendi
✅ Gemini AI sorular üretti
✅ Sorular veritabanına kaydedildi
✅ Mülakat başlatıldı
✅ Response'da üretilen sorular görüldü

---

## 📊 Gemini API Kullanım İstatistikleri

Gemini API kullanımınızı kontrol etmek için:
👉 https://aistudio.google.com/app/apikey

**Free Tier Limits:**
- 60 requests/minute
- 1500 requests/day
- gemini-1.5-flash model kullanıyorsunuz

---

## 💡 İpuçları

1. **Logging:** API çalıştırırken console loglarını takip edin
2. **Debug:** `_logger` çıktılarını kontrol edin
3. **Gemini Response:** Log'larda Gemini'nin döndürdüğü JSON'ı görebilirsiniz
4. **Test CV:** Gerçekçi bir CV kullanın, daha iyi sorular üretilir

---

## 🔗 Faydalı Linkler

- **Gemini API Key:** https://aistudio.google.com/app/apikey
- **Gemini Dökümantasyon:** https://ai.google.dev/docs
- **Postman Download:** https://www.postman.com/downloads/

---

**🎉 Test için hazırsınız! Başarılar!**

