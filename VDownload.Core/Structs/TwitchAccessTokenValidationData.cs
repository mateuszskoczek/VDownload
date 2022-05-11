using System;

namespace VDownload.Core.Structs
{
    public struct TwitchAccessTokenValidationData
    {
        #region CONSTRUCTORS

        public TwitchAccessTokenValidationData(string accessToken, bool isValid, string login, DateTime? expirationDate)
        {
            AccessToken = accessToken;
            IsValid = isValid;
            Login = login;
            ExpirationDate = expirationDate;
        }

        public static TwitchAccessTokenValidationData Null = new TwitchAccessTokenValidationData(string.Empty, false, string.Empty, null);

        #endregion



        #region PROPERTIES

        public string AccessToken { get; private set; }
        public bool IsValid { get; private set; }
        public string Login { get; private set; }
        public DateTime? ExpirationDate { get; private set; }

        #endregion
    }
}
