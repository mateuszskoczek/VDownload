using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDownload.Sources.Twitch.Authentication.Models;

namespace VDownload.Sources.Twitch.Authentication
{
    public class TwitchValidationResult
    {
        #region PROPERTIES

        public bool Success { get; private set; }
        public int? FailStatusCode { get; private set; }
        public string? FailMessage { get; private set; }
        public TwitchValidationTokenData TokenData { get; private set; }
        public DateTime ValidationDate { get; private set; }

        #endregion



        #region CONSTRUCTORS

        private TwitchValidationResult()
        {
            ValidationDate = DateTime.Now;
        }
        
        internal TwitchValidationResult(ValidateResponseFail fail)
        {
            Success = false;
            FailStatusCode = fail.Status;
            FailMessage = fail.Message;
        }

        internal TwitchValidationResult(ValidateResponseSuccess success)
        {
            Success = true;
            TokenData = new TwitchValidationTokenData(success);
        }

        #endregion
    }
}
