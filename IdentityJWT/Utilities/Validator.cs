using System.Text.RegularExpressions;

namespace PinPinServer.Utilities
{
    public static class Validator
    {
        public static bool IsValidHexColor(string input)
        {
            string regex = "^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$";
            return Regex.IsMatch(input, regex);
        }

        public static bool IsValidCurrency(string input)
        {
            string regex = "^[A-Z]{3}$";
            return Regex.IsMatch(input, regex);
        }
    }
}
