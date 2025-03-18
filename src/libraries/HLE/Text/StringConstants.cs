namespace HLE.Text;

public static class StringConstants
{
    public const string AlphabetUpperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public const string AlphabetLowerCase = "abcdefghijklmnopqrstuvwxyz";

    public const string Numbers = "0123456789";

    public const string AlphaNumerics = Numbers + AlphabetUpperCase + AlphabetLowerCase;

    public const string AlphaNumericsUpperCase = Numbers + AlphabetUpperCase;

    public const string AlphaNumericsLowerCase = Numbers + AlphabetLowerCase;

    public const string HexadecimalsUpperCase = "ABCDEF" + Numbers;

    public const string HexadecimalsLowerCase = "abcdef" + Numbers;
}
