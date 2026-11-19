
using System.Security.Cryptography;

namespace HotelManagement.Application.Reservations;

internal static class ConfirmationCodeGenerator
{
    
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";


    public static string Generate()
    {
        Span<char> chars = stackalloc char[6];
        for (var i = 0; i < chars.Length; i++)
            
            chars[i] = Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)];

        return "RES-" + new string(chars);
    }
}