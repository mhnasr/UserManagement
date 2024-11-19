using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace UserManagement.Services
{
    public class SMSService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;

        public SMSService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["SmsService:ApiKey"];
            _baseUrl = configuration["SmsService:BaseUrl"];
        }

        public async Task<string> SendSmsAsync(string message, string receiverNumber, string senderNumber)
        {
            try
            {
                // ساخت URL برای ارسال پیامک
                message = message + "\n لغو 11";
                string url = $"{_baseUrl}?note_arr[]={message}&api_key={_apiKey}&receiver_number={receiverNumber}&sender_number={senderNumber}";

                // ارسال درخواست GET
                var response = await _httpClient.GetAsync(url);

                // بررسی وضعیت HTTP
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return $"خطای سرور: {response.StatusCode} - {errorContent}";
                }

                // بررسی محتوای پاسخ
                var responseContent = await response.Content.ReadAsStringAsync();
                if (responseContent.Contains("error") || responseContent.Contains("insufficient balance"))
                {
                    return "موجودی کافی نیست یا خطای دیگری رخ داده است.";
                }

                return "Success"; // پیامک ارسال شد
            }
            catch (HttpRequestException ex)
            {
                // خطاهای مربوط به اتصال اینترنت
                return "ارتباط با اینترنت برقرار نیست. لطفاً اتصال اینترنت خود را بررسی کنید.";
            }
            catch (TaskCanceledException ex)
            {
                // خطاهای Timeout
                return "زمان درخواست به پایان رسید. لطفاً دوباره تلاش کنید.";
            }
            catch (Exception ex)
            {
                // سایر خطاها
                return "خطای نامشخصی رخ داده است. لطفاً دوباره تلاش کنید.";
            }
        }

    }
}
