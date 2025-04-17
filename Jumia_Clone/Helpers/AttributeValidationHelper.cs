namespace Jumia_Clone.Helpers
{

        /// <summary>
        /// Utility class for validating attribute values
        /// </summary>
        public static class AttributeValidationHelper
        {
            /// <summary>
            /// Validates a color value
            /// </summary>
            /// <param name="color">Color value to validate</param>
            /// <returns>True if the color is valid, false otherwise</returns>
            public static bool IsValidColor(string color)
            {
                if (string.IsNullOrWhiteSpace(color))
                    return false;

                // Hex color code validation
                if (color.StartsWith("#"))
                {
                    // Validate hex color codes (3 or 6 characters)
                    return System.Text.RegularExpressions.Regex.IsMatch(color, @"^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6})$");
                }

                // Pre-defined color names
                string[] validColors = new[]
                {
                "red", "blue", "green", "yellow", "black", "white",
                "orange", "purple", "pink", "brown", "gray",
                "cyan", "magenta", "silver", "gold"
            };

                return validColors.Contains(color.ToLower());
            }

            /// <summary>
            /// Validates a size value
            /// </summary>
            /// <param name="size">Size value to validate</param>
            /// <param name="possibleValues">Optional list of possible values</param>
            /// <returns>True if the size is valid, false otherwise</returns>
            public static bool IsValidSize(string size, string possibleValues = null)
            {
                if (string.IsNullOrWhiteSpace(size))
                    return false;

                // If possible values are provided, check against them
                if (!string.IsNullOrEmpty(possibleValues))
                {
                    var validSizes = possibleValues.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    return validSizes.Contains(size, StringComparer.OrdinalIgnoreCase);
                }

                // Generic size validation (customize as needed)
                string[] genericSizes = new[]
                {
                "XS", "S", "M", "L", "XL", "XXL",
                "XXXL", "4XL", "5XL"
            };

                return genericSizes.Contains(size, StringComparer.OrdinalIgnoreCase);
            }

            /// <summary>
            /// Validates a numeric value based on type
            /// </summary>
            /// <param name="value">Value to validate</param>
            /// <param name="type">Numeric type (int, decimal)</param>
            /// <returns>True if the value is valid, false otherwise</returns>
            public static bool IsValidNumeric(string value, string type = "int")
            {
                if (string.IsNullOrWhiteSpace(value))
                    return false;

                return type.ToLower() switch
                {
                    "int" => int.TryParse(value, out _),
                    "decimal" => decimal.TryParse(value, out _),
                    "float" => float.TryParse(value, out _),
                    "double" => double.TryParse(value, out _),
                    _ => false
                };
            }

            /// <summary>
            /// Validates a date value
            /// </summary>
            /// <param name="value">Date value to validate</param>
            /// <returns>True if the value is a valid date, false otherwise</returns>
            public static bool IsValidDate(string value)
            {
                return DateTime.TryParse(value, out _);
            }

            /// <summary>
            /// Validates a boolean value
            /// </summary>
            /// <param name="value">Boolean value to validate</param>
            /// <returns>True if the value is a valid boolean, false otherwise</returns>
            public static bool IsValidBoolean(string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return false;

                return new[] { "true", "false", "1", "0" }
                    .Contains(value.ToLower());
            }

            /// <summary>
            /// Validates a value against a list of possible values
            /// </summary>
            /// <param name="value">Value to validate</param>
            /// <param name="possibleValues">Comma-separated list of possible values</param>
            /// <param name="isMultiSelect">Whether multiple selections are allowed</param>
            /// <returns>True if the value is valid, false otherwise</returns>
            public static bool IsValidListValue(string value, string possibleValues, bool isMultiSelect = false)
            {
                if (string.IsNullOrWhiteSpace(value) || string.IsNullOrEmpty(possibleValues))
                    return false;

                var validValues = possibleValues.Split(',', StringSplitOptions.RemoveEmptyEntries);

                if (isMultiSelect)
                {
                    var selectedValues = value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    return selectedValues.All(v => validValues.Contains(v, StringComparer.OrdinalIgnoreCase));
                }

                return validValues.Contains(value, StringComparer.OrdinalIgnoreCase);
            }
        }
    
}
