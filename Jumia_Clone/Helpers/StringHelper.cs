namespace Jumia_Clone.Helpers
{

    public static class StringHelper
    {
        public static string GetUniquePath(string input)
        {
            // Generate a new GUID and remove hyphens to make it cleaner
            string guid = Guid.NewGuid().ToString("N"); // "N" format removes hyphens

            // If input is longer than 10 characters, take first 10 chars
            if (input != null && input.Length > 10)
            {
                return input.Substring(0, 10) + guid;
            }

            // For shorter strings or null, return as-is with GUID
            return (input ?? "") + guid;
        }
    }
}