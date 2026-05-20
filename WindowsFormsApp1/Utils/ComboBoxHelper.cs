using System;
using System.Data;
using System.Windows.Forms;

namespace WindowsFormsApp1.Utils
{
    public static class ComboBoxHelper
    {
        public static void Bind(ComboBox combo, DataTable data, string displayMember, string valueMember, string allItemsText = null, object allItemsValue = null)
        {
            if (combo == null) throw new ArgumentNullException(nameof(combo));
            DataTable source = data ?? new DataTable();
            if (!string.IsNullOrEmpty(allItemsText) && source.Columns.Contains(valueMember))
            {
                source = source.Copy();
                var row = source.NewRow();
                row[displayMember] = allItemsText;
                row[valueMember] = allItemsValue ?? 0;
                source.Rows.InsertAt(row, 0);
            }
            combo.DataSource = source;
            combo.DisplayMember = displayMember;
            combo.ValueMember = valueMember;
        }

        public static int? GetSelectedInt(ComboBox combo)
        {
            if (combo == null || combo.SelectedIndex < 0)
                return null;

            object value = combo.SelectedValue;
            if (value == null || value == DBNull.Value)
                return GetFromSelectedItem(combo);

            if (value is int intVal)
                return intVal;

            if (value is short s) return s;
            if (value is long l) return (int)l;
            if (value is decimal d) return (int)d;

            if (value is DataRowView rowView && !string.IsNullOrEmpty(combo.ValueMember) && rowView.Row.Table.Columns.Contains(combo.ValueMember))
            {
                object cell = rowView[combo.ValueMember];
                if (cell == null || cell == DBNull.Value) return null;
                return Convert.ToInt32(cell);
            }

            try
            {
                return Convert.ToInt32(value);
            }
            catch
            {
                return GetFromSelectedItem(combo);
            }
        }

        private static int? GetFromSelectedItem(ComboBox combo)
        {
            if (combo.SelectedItem is DataRowView drv && !string.IsNullOrEmpty(combo.ValueMember)
                && drv.Row.Table.Columns.Contains(combo.ValueMember))
            {
                object cell = drv[combo.ValueMember];
                if (cell == null || cell == DBNull.Value) return null;
                return Convert.ToInt32(cell);
            }
            return null;
        }

        public static void SelectById(ComboBox combo, int? id)
        {
            if (combo == null || !id.HasValue || id.Value <= 0)
            {
                if (combo != null && combo.Items.Count > 0)
                    combo.SelectedIndex = 0;
                return;
            }

            for (int i = 0; i < combo.Items.Count; i++)
            {
                combo.SelectedIndex = i;
                int? current = GetSelectedInt(combo);
                if (current == id.Value)
                    return;
            }
            if (combo.Items.Count > 0)
                combo.SelectedIndex = 0;
        }
    }
}
