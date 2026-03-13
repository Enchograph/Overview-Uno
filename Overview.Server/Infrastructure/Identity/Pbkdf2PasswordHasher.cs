using System.Security.Cryptography;

namespace Overview.Server.Infrastructure.Identity;

public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        Span<byte> salt = stackalloc byte[SaltSize];
        RandomNumberGenerator.Fill(salt);

        Span<byte> key = stackalloc byte[KeySize];
        Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            key,
            Iterations,
            HashAlgorithmName.SHA256);

        return string.Create(
            7 + 1 + salt.Length * 2 + 1 + key.Length * 2,
            (salt.ToArray(), key.ToArray()),
            static (span, state) =>
            {
                var value = $"PBKDF2.{Convert.ToHexString(state.Item1)}.{Convert.ToHexString(state.Item2)}";
                value.AsSpan().CopyTo(span);
            });
    }

    public bool Verify(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        var segments = passwordHash.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length != 3 || !string.Equals(segments[0], "PBKDF2", StringComparison.Ordinal))
        {
            return false;
        }

        byte[] salt;
        byte[] expectedKey;

        try
        {
            salt = Convert.FromHexString(segments[1]);
            expectedKey = Convert.FromHexString(segments[2]);
        }
        catch (FormatException)
        {
            return false;
        }

        var actualKey = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            expectedKey.Length);

        return CryptographicOperations.FixedTimeEquals(actualKey, expectedKey);
    }
}
