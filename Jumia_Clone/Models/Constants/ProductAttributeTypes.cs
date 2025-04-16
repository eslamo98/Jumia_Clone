namespace Jumia_Clone.Models.Constants
{
    public static class ProductAttributeTypes
    {
        public const string Text = "text";
        public const string Number = "number";
        public const string Decimal = "decimal";
        public const string Date = "date";
        public const string Dropdown = "dropdown";
        public const string Multiselect = "multiselect";
        public const string Radio = "radio";
        public const string Checkbox = "checkbox";
        public const string Color = "color";
        public const string Size = "size";
        public const string Material = "material";
        public const string Range = "range";

        public static bool IsValidType(string type)
        {
            return new[]
            {
            Text, Number, Decimal, Date,
            Dropdown, Multiselect, Radio, Checkbox,
            Color, Size, Material, Range
        }.Contains(type);
        }
    }
}
