using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace WindowsFormsApp1.Utils
{
    public static class InputMaskHelper
    {
        public static void ApplyDocumentNumberMask(TextBox textBox, int maxLength = 50)
        {
            textBox.MaxLength = maxLength;
            textBox.KeyPress += (s, e) =>
            {
                if (char.IsControl(e.KeyChar)) return;
                if (char.IsLetterOrDigit(e.KeyChar) || e.KeyChar == '-' || e.KeyChar == '/' || e.KeyChar == '_')
                    return;
                e.Handled = true;
            };
        }

        public static void ApplyMultilineTextMask(TextBox textBox, int maxLength = 500)
        {
            textBox.MaxLength = maxLength;
            textBox.KeyPress += (s, e) =>
            {
                if (char.IsControl(e.KeyChar)) return;
                if (e.KeyChar == '<' || e.KeyChar == '>')
                {
                    e.Handled = true;
                    return;
                }
            };
        }

        public static void ApplySearchMask(TextBox textBox, int maxLength = 100)
        {
            textBox.MaxLength = maxLength;
        }

        public static string SanitizeText(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;
            value = Regex.Replace(value.Trim(), @"\s+", " ");
            if (value.Length > maxLength)
                value = value.Substring(0, maxLength);
            return value;
        }
    }
}
