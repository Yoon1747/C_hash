// using System;
// using System.IO;
// using System.Net.Http;
// using System.Text;
// using System.Text.Json;
// using System.Threading.Tasks;

// public class AssistantService
// {
//     private const string OpenAIEndpoint = "https://api.openai.com/v1/chat/completions";
//     private readonly string apiKey;

//     public AssistantService()
// {
//     string? apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

//     if (string.IsNullOrWhiteSpace(apiKey))
//     {
//         Console.WriteLine("환경 변수 'OPENAI_API_KEY'가 설정되지 않았습니다.");
//         throw new InvalidOperationException("환경 변수 'OPENAI_API_KEY'가 설정되지 않았습니다.");
//     }

//     this.apiKey = apiKey;
// }
//     public async Task<(string FineTunedResponse, string ReportResponse)> GetGPTResponses(string inputText)
//     {
//         if (string.IsNullOrWhiteSpace(inputText))
//         {
//             Console.WriteLine("입력 텍스트가 비어 있습니다.");
//             return ("", ""); // 빈 문자열 반환
//         }

//         try
//         {
//             using (var client = new HttpClient())
//             {
//                 client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

//                 var requestBody = new
//                 {
//                     model = "gpt-3.5-turbo",
//                     messages = new[]
//                     {
//                         new { role = "system", content = "너는 공장 내의 위험상황 발생 시 매뉴얼 기반으로 대처법을 알려주는 safety assistant야. 너는 한국어로 간결하면서도 명확하게 대처법을 제공해줘, 그리고 보고서를 작성해줘." },
//                         new { role = "user", content = inputText },
//                         new { role = "system", content = "보고서 형식의 자세한 내용은 두 번째 응답으로 제공됩니다." }
//                     },
//                     max_tokens = 300,
//                     temperature = 0.7
//                 };

//                 var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

//                 var response = await client.PostAsync(OpenAIEndpoint, jsonContent);
//                 response.EnsureSuccessStatusCode();

//                 var responseContent = await response.Content.ReadAsStringAsync();
                
//                 // 디버깅: API 응답 출력
//                 Console.WriteLine($"OpenAI 응답: {responseContent}");

//                 var jsonResponse = JsonDocument.Parse(responseContent);
                
//                 // 응답 검증 및 처리
//                 var choices = jsonResponse.RootElement.GetProperty("choices");
//                 if (choices.GetArrayLength() < 2)
//                 {
//                     Console.WriteLine("OpenAI 응답에서 충분한 데이터가 없습니다.");
//                     return ("", "");
//                 }
    
//                 var fineTunedResponse = choices[0].GetProperty("message").GetProperty("content").GetString() ?? "";
//                 var reportResponse = choices[1].GetProperty("message").GetProperty("content").GetString() ?? "";
    
//                 return (fineTunedResponse, reportResponse);
//                 }
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine($"OpenAI API 호출 중 오류 발생: {ex.Message}");
//             return ("", ""); // 오류 시 빈 문자열 반환
//         }
//     }
// }