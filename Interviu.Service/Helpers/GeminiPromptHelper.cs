namespace Interviu.Service.Helpers;

/// <summary>
/// Gemini AI için prompt şablonlarını yöneten helper sınıfı
/// </summary>
public static class GeminiPromptHelper
{
    /// <summary>
    /// CV ve pozisyon bilgisine göre mülakat soruları oluşturmak için HR uzmanı prompt'u
    /// </summary>
    public static string CreateInterviewQuestionsPrompt(string cvText, string position, int questionCount)
    {
        return $@"Sen deneyimli bir İnsan Kaynakları uzmanısın. Aşağıdaki CV'yi ve başvurulan pozisyonu inceleyerek, adaya uygun {questionCount} adet mülakat sorusu oluştur.

**Başvurulan Pozisyon:** {position}

**Aday CV'si:**
{cvText}

**Görevin:**
1. CV'deki deneyim, beceri ve projeleri dikkate alarak pozisyona uygun sorular oluştur
2. Farklı zorluk seviyelerinde sorular hazırla (Kolay, Orta, Zor)
3. Teknik, davranışsal ve durumsal sorular dengeli bir şekilde dağıt
4. Her soruyu bir kategoriye ata (örn: Teknik, Davranışsal, Problem Çözme, Liderlik, vb.)

**ÇIKTI FORMATI (SADECE JSON, BAŞKA BİR ŞEY YAZMA):**
Çıktını SADECE aşağıdaki JSON formatında ver. Açıklama veya başka metin ekleme:

{{
  ""questions"": [
    {{
      ""questionText"": ""Soru metni buraya"",
      ""difficulty"": ""Easy|Medium|Hard"",
      ""category"": ""Kategori adı""
    }}
  ]
}}

**ÖNEMLİ:** 
- Yanıtın SADECE geçerli bir JSON olmalı
- Markdown code block (```) kullanma
- Sadece düz JSON metni döndür
- Tam olarak {questionCount} adet soru oluştur
- Her sorunun questionText, difficulty ve category alanları dolu olmalı";
    }
}

