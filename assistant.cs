using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class AssistantService
{
    private const string OpenAIEndpoint = "https://api.openai.com/v1/chat/completions";
    private readonly string apiKey;

    public AssistantService()
    {
        // 환경 변수에서 OpenAI API 키 가져오기
        apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                 ?? throw new InvalidOperationException("환경 변수 'OPENAI_API_KEY'가 설정되지 않았습니다.");
    }

    public async Task<(string FineTunedResponse, string ReportResponse)> GetGPTResponses(string inputText)
    {
        if (string.IsNullOrWhiteSpace(inputText))
        {
            Console.WriteLine("입력 텍스트가 비어 있습니다.");
            return ("입력이 비어 있습니다.", "입력이 비어 있습니다.");
        }

        try
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var requestBody = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[]
                    {
                        new { role = "system", content = "너는 공장 내의 위험상황 발생 시 매뉴얼 기반으로 대처법을 알려주는 safety assistant야. 너는 한국어로 간결하면서도 명확하게 대처법을 제공해줘, 그리고 보고서를 작성해줘."},
                        new { role = "system", content = "파이프의 결함 종류에는 0단계, 1단계, 2단계, 3단계가 있어. 결함이라고 판단하는 기준은 파이프의 크랙이 1인치(2.5cm)이상 있을 때, 결함이라고 판단할거야."},
                        new { role = "system", content = "0단계는 크랙이 1인치(2.5cm)미만으로 결함으로 판단하지 않는 단계야."},
                        new { role = "system", content = "1단계는 크랙이 1인치(2.5cm)이상으로 결함으로 판단하는 단계야. 이에 따른 조치사항으로는 일시적 수리로써 손상된 파이프 또는 부식된 부분에 대해 임시 수리는 인가된 설계에 따라 수행. 예) 용접된 전체 감싸는 슬리브 또는 박스형 덮개를 적용"},
                        new { role = "system", content = "2단계는 크랙이 3인치(7.62cm)이상으로 결함으로 판단하는 단계야. 이에 따른 조치사항으로는 비용접 수리로써 국소적으로 얇아진 부분이나 선형 결함에 대해 볼트 클램프, 비금속 복합 랩 또는 메탈릭 에폭시 랩을 적용"},
                        new { role = "system", content = "3단계는 크랙이 5인치(12.7cm)이상으로 결함으로 판단하는 단계야. 이에 따른 조치사항으로는 영구적 수리로써 결함이 있는 구역을 원통형 절단 및 대체 부품으로 교체, 필요한 경우 삽입 패치를 적용"},
                        new { role = "system", content = "결함이 1단계 미만일 경우에는 보고서를 작성하지 말고, 1단계 이상일 경우에만 보고서 형식에 맞게 보고서를 작성해줘."},
                        new { role = "system", content = "지금은 테스트 단계니까 어떤 텍스트를 받아도 그 텍스트에 맞게 대답하지말고, 크랙 2단계에 해당했을 때의 대처 방법에 대해 대답해줘."}
                        // new { role = "user", content = inputText }
                    },
                    max_tokens = 300,
                    temperature = 0.7
                };

                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                var response = await client.PostAsync(OpenAIEndpoint, jsonContent);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();

                // 디버깅: OpenAI 응답 출력
                Console.WriteLine($"OpenAI 응답: {responseContent}");

                var jsonResponse = JsonDocument.Parse(responseContent);

                var choices = jsonResponse.RootElement.GetProperty("choices");

                if (choices.GetArrayLength() == 0)
                {
                    Console.WriteLine("OpenAI 응답에서 충분한 데이터가 없습니다.");
                    return ("응답 부족", "응답 부족");
                }

                // 첫 번째 응답 처리
                var fineTunedResponse = choices[0].GetProperty("message").GetProperty("content").GetString() ?? "기본 응답 없음";

                // 두 번째 응답이 없을 경우 첫 번째 응답 재사용
                var reportResponse = (choices.GetArrayLength() > 1)
                    ? choices[1].GetProperty("message").GetProperty("content").GetString() ?? "기본 보고서 없음"
                    : fineTunedResponse;

                return (fineTunedResponse, reportResponse);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"OpenAI API 호출 중 오류 발생: {ex.Message}");
            return ("오류 발생", "오류 발생");
        }
    }
}