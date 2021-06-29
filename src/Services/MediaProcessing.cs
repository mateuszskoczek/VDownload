using System;
using System.IO;
using System.Threading.Tasks;
using FFMpegCore;

namespace VDownload
{
    class MediaProcessing
    {
        #region CONSTANTS

        private static readonly FFOptions ffmpegOptions = new()
        {
            BinaryFolder = Global.Paths.FFMPEG,
            TemporaryFilesFolder = Global.Paths.TEMP,
        }; // OPTIONS
        private static readonly string mergeAVOutputExtension = "mp4"; // MERGEAV OUTPUT FILE EXTENSION

        #endregion CONSTANTS





        #region PROCESSING

        // CONVERT FILE
        public static async Task<string> Convert(string input, string extension, string videoID)
        {
            // Apply options
            GlobalFFOptions.Configure(ffmpegOptions);

            // Output path
            string output = String.Format(@"{0}\converted_{1}.{2}", Global.Paths.TEMP, videoID, extension);

            // Convert
            await FFMpegArguments.FromFileInput(input)
                                 .OutputToFile(output)
                                 .ProcessAsynchronously();

            // Return output path
            return output;
        }

        // MERGE FILES
        public static string MergeAV(string inputVideo, string inputAudio, string videoID)
        {
            // Apply options
            GlobalFFOptions.Configure(ffmpegOptions);

            // Output path
            string output = String.Format(@"{0}\merged_{1}.{2}", Global.Paths.TEMP, videoID, mergeAVOutputExtension);

            // Start merge
            FFMpeg.ReplaceAudio(inputVideo, inputAudio, output);

            // Return output path
            return output;
        }

        // TRIM FILE
        public static async Task<string> Trim(string input, double start, double end, string videoID)
        {
            // Apply options
            GlobalFFOptions.Configure(ffmpegOptions);

            // Output path
            string output = String.Format(@"{0}\trimmed_{1}.{2}", Global.Paths.TEMP, videoID, Path.GetExtension(input));

            // Start trim
            await FFMpegArguments.FromFileInput(input)
                                 .OutputToFile(output, true, options => options
                                    .WithCustomArgument(String.Format("-ss {0}", start.ToString()))
                                    .WithCustomArgument(String.Format("-t {0}", end.ToString()))
                                 )
                                 .ProcessAsynchronously();

            // Return output path
            return output;
        }

        #endregion PROCESSING
    }
}
