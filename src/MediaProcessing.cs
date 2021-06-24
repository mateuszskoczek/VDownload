using System;
using System.Diagnostics;
using FFMpegCore;
using VDownload.Parsers;

namespace VDownload
{
    class MediaProcessing
    {
        // OPTIONS
        private static readonly FFOptions ffmpegOptions = new()
        {
            BinaryFolder = Global.Paths.FFMPEG,
            TemporaryFilesFolder = Global.Paths.TEMP,
        };


        // CONVERT MEDIA
        public static void Convert(string input, string output)
        {
            // Apply options
            GlobalFFOptions.Configure(ffmpegOptions);

            // Start convert
            Console.Write(TerminalOutput.Get(@"output\media_processing\convert\start.out", upSP: false, downSP: false));
            Stopwatch convertingTime = new();
            convertingTime.Start();
            try
            {
                FFMpegArguments.FromFileInput(input).OutputToFile(output).ProcessSynchronously();
            }
            catch (Exception e)
            {
                Console.WriteLine(TerminalOutput.Get(@"output\media_processing\convert\error\undefined.out",
                    args: new()
                    {
                        e.Message
                    }
                ));
                Environment.Exit(0);
            }
            convertingTime.Stop();
            Console.WriteLine(String.Format(" (Done in {0} seconds)", convertingTime.Elapsed.TotalSeconds));
        }

        // MERGE MEDIA
        public static void Merge(string inputVideo, string inputAudio, string output)
        {
            // Apply options
            GlobalFFOptions.Configure(ffmpegOptions);

            // Start merge
            Console.Write(TerminalOutput.Get(@"output\media_processing\merge\start.out", upSP: false, downSP: false));
            Stopwatch mergingTime = new();
            mergingTime.Start();
            try
            {
                FFMpeg.ReplaceAudio(inputVideo, inputAudio, output);
            }
            catch (Exception e)
            {
                Console.WriteLine(TerminalOutput.Get(@"output\media_processing\merge\error\undefined.out",
                    args: new()
                    {
                        e.Message
                    }
                ));
                Environment.Exit(0);
            }
            mergingTime.Stop();
            Console.WriteLine(String.Format(" (Done in {0} seconds)", mergingTime.Elapsed.TotalSeconds));
        }
    }
}
