using Google.Cloud.TextToSpeech.V1;
using System;
using System.IO;

public class TTSService
{
    // Google Cloud Text-to-Speech API를 사용하여 텍스트를 음성으로 변환
    public void ConvertTextToSpeech(string text, string outputFilePath, string languageCode = "en-US", string voiceName = "en-US-Wavenet-D")
    {
        try
        {
            // Text-to-Speech 클라이언트 생성
            var client = TextToSpeechClient.Create();

            // 요청 구성
            var input = new SynthesisInput
            {
                Text = text
            };

            var voice = new VoiceSelectionParams
            {
                LanguageCode = languageCode,
                Name = voiceName
            };

            var audioConfig = new AudioConfig
            {
                AudioEncoding = AudioEncoding.Mp3
            };

            // API 호출
            var response = client.SynthesizeSpeech(input, voice, audioConfig);

            // 응답 데이터를 파일로 저장
            using (var output = File.Create(outputFilePath))
            {
                response.AudioContent.WriteTo(output);
            }

            Console.WriteLine($"텍스트가 음성으로 변환되어 파일로 저장되었습니다: {outputFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"TTS 변환 중 오류 발생: {ex.Message}");
        }
    }
}