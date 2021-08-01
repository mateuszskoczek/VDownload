// System
using System.Diagnostics;
using System.Threading.Tasks;

// Internal
using VDownload.Core.Enums.FFmpeg;



namespace VDownload.Core.Services
{
    class FFmpeg
    {
        #region MAIN

        // BASE
        public string Arguments = "-y ";


        // START
        public async Task StartAsync()
        {
            await Task.Run(() =>
            {
                var process = new Process
                {
                    StartInfo =
                    {
                        FileName = $@"{Config.Read("ffmpeg_executables_path")}\ffmpeg",
                        Arguments = Arguments,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };
                process.Start();
                process.WaitForExit();
            });

        }

        #endregion



        #region ARGUMENTS

        // INPUT
        public void Input(string path)
        {
            Arguments += $"-i \"{path}\" ";
        }


        // TRIM AT START
        public void TrimStart(double seconds)
        {
            Arguments += $"-ss {seconds.ToString().Replace(',', '.')} ";
        }


        // TRIM AT END
        public void TrimEnd(double seconds)
        {
            Arguments += $"-t {seconds.ToString().Replace(',', '.')} ";
        }

        
        // DISABLE SPECIFIED CHANNELS
        public void DisableChannel(Channel channel)
        {
            if (channel == Channel.Both)
            {
                
            }
            else
            {
                Arguments += $"-{(channel == Channel.Video ? 'v' : 'a')}n ";
            }
        }
        

        // COPY SPECIFIED CHANNELS
        public void CopyChannel(Channel channel)
        {
            if (channel == Channel.Both)
            {
                Arguments += $"-c copy ";
            }
            else
            {
                Arguments += $"-c:{(channel == Channel.Video ? 'v' : 'a')} copy ";
            }
        }


        // SPEED PRESET
        public void Speed(Speed speed)
        {
            Arguments += $"-preset {speed.ToString().ToLower()} ";
        }


        // AVOID NEGATIVE TS
        public void AvoidNegativeTS(AvoidNegativeTS value)
        {
            Arguments += $"-avoid_negative_ts {StringC.BumperToSnakeCase(value.ToString())} ";
        }


        // OUTPUT
        public void Output(string path)
        {
            Arguments += $"\"{path}\" ";
        }

        #endregion
    }
}
