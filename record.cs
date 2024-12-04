using System;
using System.IO;
using System.Runtime.InteropServices;

public class AudioRecorder
{
    const string PortAudioLib = "libportaudio.dylib";

    [DllImport(PortAudioLib)]
    public static extern int Pa_Initialize();

    [DllImport(PortAudioLib)]
    public static extern int Pa_Terminate();

    [DllImport(PortAudioLib)]
    public static extern IntPtr Pa_GetErrorText(int errorCode);

    [DllImport(PortAudioLib)]
    public static extern int Pa_OpenDefaultStream(
        out IntPtr stream,
        int numInputChannels,
        int numOutputChannels,
        int sampleFormat,
        double sampleRate,
        uint framesPerBuffer,
        IntPtr streamCallback,
        IntPtr userData);

    [DllImport(PortAudioLib)]
    public static extern int Pa_StartStream(IntPtr stream);

    [DllImport(PortAudioLib)]
    public static extern int Pa_StopStream(IntPtr stream);

    [DllImport(PortAudioLib)]
    public static extern int Pa_CloseStream(IntPtr stream);

    [DllImport(PortAudioLib)]
    public static extern int Pa_ReadStream(IntPtr stream, IntPtr buffer, uint frames);

    public void RecordAudio(string outputFilePath, int secondsToRecord = 5)
    {
        const int sampleRate = 16000;
        const int numChannels = 1;
        const int framesPerBuffer = 1024;
        const int sampleFormat = 8;

        IntPtr stream;
        int err;

        err = Pa_Initialize();
        if (err != 0)
        {
            Console.WriteLine($"PortAudio 초기화 실패: {Marshal.PtrToStringAnsi(Pa_GetErrorText(err))}");
            return;
        }
        Console.WriteLine("PortAudio 초기화 성공");

        err = Pa_OpenDefaultStream(
            out stream,
            numChannels,
            0,
            sampleFormat,
            sampleRate,
            (uint)framesPerBuffer,
            IntPtr.Zero,
            IntPtr.Zero
        );

        if (err != 0)
        {
            Console.WriteLine($"스트림 열기 실패: {Marshal.PtrToStringAnsi(Pa_GetErrorText(err))}");
            Pa_Terminate();
            return;
        }
        Console.WriteLine("스트림 열기 성공");

        err = Pa_StartStream(stream);
        if (err != 0)
        {
            Console.WriteLine($"스트림 시작 실패: {Marshal.PtrToStringAnsi(Pa_GetErrorText(err))}");
            Pa_CloseStream(stream);
            Pa_Terminate();
            return;
        }
        Console.WriteLine("녹음을 시작합니다...");

        int totalFrames = sampleRate * secondsToRecord;
        short[] audioData = new short[totalFrames];
        GCHandle audioHandle = GCHandle.Alloc(audioData, GCHandleType.Pinned);

        for (int i = 0; i < totalFrames; i += framesPerBuffer)
        {
            uint framesToRead = (uint)Math.Min(framesPerBuffer, totalFrames - i);
            err = Pa_ReadStream(stream, audioHandle.AddrOfPinnedObject() + i * sizeof(short), framesToRead);

            if (err != 0)
            {
                Console.WriteLine($"스트림 읽기 실패: {Marshal.PtrToStringAnsi(Pa_GetErrorText(err))}");
                break;
            }
        }

        audioHandle.Free();

        Pa_StopStream(stream);
        Pa_CloseStream(stream);
        Pa_Terminate();
        Console.WriteLine("녹음이 완료되었습니다.");

        SaveToWav(outputFilePath, audioData, numChannels, sampleRate);
    }

    private void SaveToWav(string outputFilePath, short[] audioData, int numChannels, int sampleRate)
    {
        using (FileStream fs = new FileStream(outputFilePath, FileMode.Create))
        using (BinaryWriter writer = new BinaryWriter(fs))
        {
            WriteWavHeader(writer, numChannels, sampleRate, 16, audioData.Length * sizeof(short));
            foreach (var sample in audioData)
            {
                writer.Write(sample);
            }
        }
        Console.WriteLine($"파일이 '{outputFilePath}'로 저장되었습니다.");
    }

    private void WriteWavHeader(BinaryWriter writer, int numChannels, int sampleRate, int bitsPerSample, int dataSize)
    {
        writer.Write(new char[] { 'R', 'I', 'F', 'F' });
        writer.Write(36 + dataSize);
        writer.Write(new char[] { 'W', 'A', 'V', 'E' });
        writer.Write(new char[] { 'f', 'm', 't', ' ' });
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)numChannels);
        writer.Write(sampleRate);
        writer.Write(sampleRate * numChannels * bitsPerSample / 8);
        writer.Write((short)(numChannels * bitsPerSample / 8));
        writer.Write((short)bitsPerSample);
        writer.Write(new char[] { 'd', 'a', 't', 'a' });
        writer.Write(dataSize);
    }
}