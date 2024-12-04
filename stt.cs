using Google.Cloud.Speech.V1;
using System;
using System.IO;

public class STTService
{
    public string TranscribeAudioToText(string audioFilePath)
{
    if (!File.Exists(audioFilePath))
    {
        Console.WriteLine($"오디오 파일이 존재하지 않습니다: {audioFilePath}");
        return ""; // 빈 문자열 반환
    }

    try
    {
        var speechClient = SpeechClient.Create();

        var config = new RecognitionConfig
        {
            Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
            SampleRateHertz = 16000,
            LanguageCode = "ko-KR", // 한국어
            AlternativeLanguageCodes = { "en-US" } // 대체 언어
        };

        var audio = RecognitionAudio.FromFile(audioFilePath);

        Console.WriteLine("STT 요청을 시작합니다...");

        var response = speechClient.Recognize(config, audio);

        string transcript = "";
        
        foreach (var result in response.Results)
            {
                foreach (var alternative in result.Alternatives)
                {
                    transcript += alternative.Transcript + "\n";
                }
            }

        return transcript; // 텍스트 반환
    }
    catch (Exception ex)
    {
        Console.WriteLine($"STT 변환 중 오류 발생: {ex.Message}");
        return ""; // 오류 시 빈 문자열 반환
    }
}
}