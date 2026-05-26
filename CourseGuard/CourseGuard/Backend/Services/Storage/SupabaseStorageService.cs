using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CourseGuard.Backend.Config;

namespace CourseGuard.Backend.Services.Storage
{
    public class SupabaseStorageService
    {
        private readonly string _supabaseUrl;
        private readonly string _anonKey;
        private readonly HttpClient _httpClient;

        public SupabaseStorageService(string? supabaseUrl = null, string? anonKey = null)
        {
            _supabaseUrl = string.IsNullOrWhiteSpace(supabaseUrl)
                ? AppEnvironment.GetRequired("SUPABASE_URL")
                : supabaseUrl;

            _anonKey = string.IsNullOrWhiteSpace(anonKey)
                ? AppEnvironment.GetRequired("SUPABASE_ANON_KEY")
                : anonKey;

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("apikey", _anonKey);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _anonKey);
        }

        /// <summary>
        /// Tải hình ảnh vi phạm lên Supabase Storage bucket 'exam-violations'
        /// </summary>
        /// <param name="imageBytes">Dữ liệu ảnh dạng mảng byte</param>
        /// <param name="fileName">Tên file (vd: violation_123.jpg)</param>
        /// <returns>Đường dẫn công khai (Public URL) của file ảnh đã tải lên</returns>
        public async Task<string> UploadViolationImageAsync(byte[] imageBytes, string fileName)
        {
            string bucketName = "exam-violations";
            string endpoint = $"{_supabaseUrl.TrimEnd('/')}/storage/v1/object/{bucketName}/{fileName}";

            using var content = new ByteArrayContent(imageBytes);
            content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

            var response = await _httpClient.PostAsync(endpoint, content);
            
            if (response.IsSuccessStatusCode)
            {
                // Trả về public URL
                return $"{_supabaseUrl.TrimEnd('/')}/storage/v1/object/public/{bucketName}/{fileName}";
            }

            string errorResponse = await response.Content.ReadAsStringAsync();
            throw new Exception($"Lỗi upload ảnh lên Supabase: {(int)response.StatusCode} - {errorResponse}");
        }
    }
}
