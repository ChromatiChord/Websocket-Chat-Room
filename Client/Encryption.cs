using System;
using System.Security.Cryptography;
using System.Text;

public class Encryption
{

    private RSACryptoServiceProvider _rsa = new RSACryptoServiceProvider();

    public (string publicKey, string privateKey) GenerateKeyPair() {

        return (_rsa.ToXmlString(false), _rsa.ToXmlString(true));
    }

    public string EncryptString(string inputText, string key) {
        
        byte[] dataToEncrypt = Encoding.UTF8.GetBytes(inputText);
        _rsa.FromXmlString(key);
        byte[] encryptedData = _rsa.Encrypt(dataToEncrypt, false);

        return Convert.ToBase64String(encryptedData);

    }

    public string DecryptString(string inputText, string key) {

        _rsa.FromXmlString(key);
        byte[] dataToDecrypt = Convert.FromBase64String(inputText);
        byte[] decryptedData = _rsa.Decrypt(dataToDecrypt, false);
        return Encoding.UTF8.GetString(decryptedData);

    }
}
