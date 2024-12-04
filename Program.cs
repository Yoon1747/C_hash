using System;
using System.IO;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("음성 기반 GPT 질의응답 시작...");

        // 1. 녹음
        var recorder = new AudioRecorder();
        string audioFilePath = "user_input.wav";
        recorder.RecordAudio(audioFilePath, 10);

        // 2. STT: 음성 파일을 텍스트로 변환
        var sttService = new STTService();
        string userQuery = sttService.TranscribeAudioToText(audioFilePath);

        if (string.IsNullOrWhiteSpace(userQuery))
        {
            Console.WriteLine("STT 변환 실패. 프로그램을 종료합니다.");
            return;
        }

        Console.WriteLine($"사용자 입력 텍스트: {userQuery}");

        // 3. OpenAI GPT 질의응답
        // string openAIKey = "your-openai-api-key"; // OpenAI API 키
        var assistantService = new AssistantService(); // OpenAI API 키를 생성자에서 환경 변수로 관리
        var (fineTunedResponse, reportResponse) = await assistantService.GetGPTResponses(userQuery);

        if (string.IsNullOrWhiteSpace(fineTunedResponse) || string.IsNullOrWhiteSpace(reportResponse))
        {
            Console.WriteLine("GPT 응답을 가져오지 못했습니다. 프로그램을 종료합니다.");
            return;
        }

        Console.WriteLine($"GPT 응답 (Fine-tuned): {fineTunedResponse}");
        Console.WriteLine($"GPT 응답 (Report): {reportResponse}");

        // 4. TTS: Fine-tuned 대답을 음성으로 변환 및 출력
        var ttsService = new TTSService();
        string responseAudioPath = "fine_tuned_response.mp3";
        ttsService.ConvertTextToSpeech(fineTunedResponse, responseAudioPath);

        // 5. 보고서 저장
        string reportFilePath = "report.docx";
        SaveReportToDocx(reportFilePath, reportResponse);

        Console.WriteLine($"프로그램 완료. 보고서는 '{reportFilePath}'에 저장되었고, 음성은 '{responseAudioPath}'로 저장되었습니다.");
    }

    static void SaveReportToDocx(string filePath, string content)
    {
        using (WordprocessingDocument wordDoc = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
        {
            var mainPart = wordDoc.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = new Body();

            // Add text content to the document
            body.Append(new Paragraph(new Run(new Text(content))));
            mainPart.Document.Append(body);
            mainPart.Document.Save();
        }
    }
}