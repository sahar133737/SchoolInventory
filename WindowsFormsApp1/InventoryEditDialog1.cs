using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public class InventoryEditDialog : Form
    {
        private readonly InventoryManager inventoryManager;
        private readonly int inventoryId;
        private TextBox txtName;
        private TextBox txtDescription;
        private ComboBox cbCategory;
        private ComboBox cbClassroom;
        private ComboBox cbPerson;
        private TextBox txtInvNumber;
        private DateTimePicker dpPurchaseDate;
        private NumericUpDown numPrice;
        private TextBox txtState;
        private Button btnOk;
        private Button btnCancel;

        public InventoryEditDialog(InventoryManager inventoryManager, int inventoryId)
        {
            this.inventoryManager = inventoryManager;
            this.inventoryId = inventoryId;
            InitializeComponents();
            LoadLookups();
            LoadEntity();
        }

        private void InitializeComponents()
        {
            Text = "Изменение инвентаря";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(700, 520);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var lblName = new Label { Text = "Наименование:", Left = 20, Top = 20, Width = 150 };
            txtName = new TextBox { Left = 180, Top = 16, Width = 470 };

            var lblDesc = new Label { Text = "Описание:", Left = 20, Top = 60, Width = 150 };
            txtDescription = new TextBox { Left = 180, Top = 56, Width = 470 };

            var lblCategory = new Label { Text = "Категория:", Left = 20, Top = 100, Width = 150 };
            cbCategory = new ComboBox { Left = 180, Top = 96, Width = 470, DropDownStyle = ComboBoxStyle.DropDownList };

            var lblClassroom = new Label { Text = "Кабинет:", Left = 20, Top = 140, Width = 150 };
            cbClassroom = new ComboBox { Left = 180, Top = 136, Width = 470, DropDownStyle = ComboBoxStyle.DropDownList };

            var lblPerson = new Label { Text = "Ответственный:", Left = 20, Top = 180, Width = 150 };
            cbPerson = new ComboBox { Left = 180, Top = 176, Width = 470, DropDownStyle = ComboBoxStyle.DropDownList };

            var lblInv = new Label { Text = "Инв. номер:", Left = 20, Top = 220, Width = 150 };
            txtInvNumber = new TextBox { Left = 180, Top = 216, Width = 200 };

            var lblDate = new Label { Text = "Дата покупки:", Left = 20, Top = 260, Width = 150 };
            dpPurchaseDate = new DateTimePicker { Left = 180, Top = 256, Width = 200, Format = DateTimePickerFormat.Short };

            var lblPrice = new Label { Text = "Цена:", Left = 20, Top = 300, Width = 150 };
            numPrice = new NumericUpDown { Left = 180, Top = 296, Width = 200, DecimalPlaces = 2, Maximum = 10000000, Minimum = 0 };

            var lblState = new Label { Text = "Состояние:", Left = 20, Top = 340, Width = 150 };
            txtState = new TextBox { Left = 180, Top = 336, Width = 470 };

            btnOk = new Button { Text = "Сохранить", Left = 450, Top = 400, Width = 100, DialogResult = DialogResult.None };
            btnCancel = new Button { Text = "Отмена", Left = 560, Top = 400, Width = 90, DialogResult = DialogResult.Cancel };

            btnOk.Click += BtnOk_Click;

            Controls.AddRange(new Control[]
            {
                lblName, txtName,
                lblDesc, txtDescription,
                lblCategory, cbCategory,
                lblClassroom, cbClassroom,
                lblPerson, cbPerson,
                lblInv, txtInvNumber,
                lblDate, dpPurchaseDate,
                lblPrice, numPrice,
                lblState, txtState,
                btnOk, btnCancel
            });
        }

        private void LoadLookups()
        {
            cbCategory.DataSource = inventoryManager.GetCategories();
            cbCategory.DisplayMember = "CategoryName";
            cbCategory.ValueMember = "CategoryID";

            cbClassroom.DataSource = inventoryManager.GetClassrooms();
            cbClassroom.DisplayMember = "Name";
            cbClassroom.ValueMember = "ClassroomID";

            cbPerson.DataSource = inventoryManager.GetResponsiblePersons();
            cbPerson.DisplayMember = "Name";
            cbPerson.ValueMember = "PersonID";
        }

        private void LoadEntity()
        {
            DataTable dt = inventoryManager.GetInventoryForEdit(inventoryId);
            if (dt.Rows.Count == 0) { Close(); return; }
            DataRow r = dt.Rows[0];
            txtName.Text = Convert.ToString(r["ItemName"]);
            txtDescription.Text = Convert.ToString(r["Description"]);
            cbCategory.SelectedValue = Convert.ToInt32(r["CategoryID"]);
            cbClassroom.SelectedValue = Convert.ToInt32(r["ClassroomID"]);
            cbPerson.SelectedValue = Convert.ToInt32(r["ResponsiblePersonID"]);
            txtInvNumber.Text = Convert.ToString(r["InventoryNumber"]);
            dpPurchaseDate.Value = Convert.ToDateTime(r["PurchaseDate"]);
            numPrice.Value = Convert.ToDecimal(r["PurchasePrice"]);
            txtState.Text = Convert.ToString(r["CurrentState"]);
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите наименование.");
                return;
            }

            try
            {
                bool ok = inventoryManager.UpdateInventoryItem(
                    inventoryId,
                    txtName.Text.Trim(),
                    txtDescription.Text.Trim(),
                    Convert.ToInt32(cbCategory.SelectedValue),
                    Convert.ToInt32(cbClassroom.SelectedValue),
                    Convert.ToInt32(cbPerson.SelectedValue),
                    txtInvNumber.Text.Trim(),
                    dpPurchaseDate.Value.Date,
                    Convert.ToDecimal(numPrice.Value),
                    txtState.Text.Trim()
                );

                if (ok)
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    MessageBox.Show("Не удалось сохранить изменения.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}


