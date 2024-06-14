﻿using System.Security.Cryptography;

namespace ODK.Core.Cryptography;

public static class RandomStringGenerator
{
    public static string Generate(int length)
    {
        byte[] randomNumber = new byte[length];
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
