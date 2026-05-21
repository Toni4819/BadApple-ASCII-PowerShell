using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        string exeDir = AppDomain.CurrentDomain.BaseDirectory;
        string video = Path.Combine(exeDir, "BadApple.mp4");
        string outFile = Path.Combine(exeDir, "BadApple_ASCII.ps1");

        if (!File.Exists(video))
        {
            Console.WriteLine("Video not found: " + video);
            return;
        }

        // =========================
        // FINAL CONFIG (STABLE 4:3)
        // =========================

        const int width = 80;
        const int height = 30;
        const int fps = 12;

        int frameSize = width * height;

        string ramp = " .:-=+*#%@";

        char[] map = new char[256];

        for (int i = 0; i < 256; i++)
            map[i] = ramp[i * (ramp.Length - 1) / 255];

        string vf = $"scale={width}:{height},fps={fps},format=gray";

        var psi = new ProcessStartInfo(
            "ffmpeg",
            $"-v error -nostdin -i \"{video}\" -vf {vf} -f rawvideo -pix_fmt gray -"
        )
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var ffmpeg = Process.Start(psi);

        if (ffmpeg == null)
        {
            Console.WriteLine("FFmpeg failed");
            return;
        }

        // drain stderr (prevents freeze)
        _ = Task.Run(async () =>
        {
            char[] buf = new char[2048];
            while (!ffmpeg.StandardError.EndOfStream)
                await ffmpeg.StandardError.ReadAsync(buf, 0, buf.Length);
        });

        using Stream stdout = ffmpeg.StandardOutput.BaseStream;

        using StreamWriter writer = new StreamWriter(
            outFile,
            false,
            new UTF8Encoding(false),
            1024 * 1024
        );

        await writer.WriteLineAsync("$frames = @(");

        byte[] frame = new byte[frameSize];

        bool first = true;
        int frameIndex = 0;

        Console.WriteLine("Generating...");

        while (await ReadFrame(stdout, frame, frameSize))
        {
            StringBuilder sb = new StringBuilder(width * height);

            for (int y = 0; y < height; y++)
            {
                int row = y * width;

                for (int x = 0; x < width; x++)
                    sb.Append(map[frame[row + x]]);

                sb.Append('\n');
            }

            string text = sb.ToString()
                .Replace("`", "``")
                .Replace("\"", "`\"");

            if (!first)
                await writer.WriteLineAsync(",");

            await writer.WriteLineAsync($"@\"\n{text}\"@");

            first = false;
            frameIndex++;

            if (frameIndex % 100 == 0)
                Console.WriteLine($"Frames: {frameIndex}");
        }

        await writer.WriteLineAsync(")");

        // CLEAN PLAYER
        await writer.WriteLineAsync(@"
$frameMs = 66
[Console]::CursorVisible = $false
Clear-Host

foreach($f in $frames)
{
    [Console]::SetCursorPosition(0,0)
    Write-Host $f -NoNewline
    Start-Sleep -Milliseconds $frameMs
}
");

        Console.WriteLine("Done");
        Console.WriteLine($"Frames: {frameIndex}");
        Console.WriteLine($"Output: {outFile}");
    }

    static async Task<bool> ReadFrame(Stream s, byte[] buffer, int size)
    {
        int offset = 0;

        while (offset < size)
        {
            int read = await s.ReadAsync(buffer, offset, size - offset);
            if (read == 0)
                return false;

            offset += read;
        }

        return true;
    }
}