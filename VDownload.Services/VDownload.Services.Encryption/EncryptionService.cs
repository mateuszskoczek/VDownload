using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Security;
using System.Runtime.InteropServices;

namespace VDownload.Services.Encryption
{
    public interface IEncryptionService
    {
        byte[] Decrypt(byte[] ciphertext);
        byte[] Encrypt(byte[] plaintext);
    }



    public class EncryptionService : IEncryptionService
    {
        #region PUBLIC METHODS

        public byte[] Encrypt(byte[] plaintext)
        {
            return ProtectedData.Protect(plaintext, null, DataProtectionScope.CurrentUser);
        }

        public byte[] Decrypt(byte[] ciphertext)
        {
            return ProtectedData.Unprotect(ciphertext, null, DataProtectionScope.CurrentUser);
        }

        #endregion
    }
}
