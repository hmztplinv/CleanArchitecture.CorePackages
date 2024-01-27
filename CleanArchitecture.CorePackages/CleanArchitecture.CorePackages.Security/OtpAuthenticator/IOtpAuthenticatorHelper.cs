public interface IOtpAuthenticatorHelper
{
    public Task<byte[]> GenerateKeyAsync();
    public Task<string> ConvertSecretKeyToStringAsync(byte[] secretKey);
    public Task<bool> VerifyCodeAsync(byte[] secretKey, string code);
}