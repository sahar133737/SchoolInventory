using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public class InventoryAddDialog : Form
    {
        private readonly InventoryManager inventoryManager;
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

        public InventoryAddDialog(InventoryManager inventoryManager)
        {
            this.inventoryManager = inventoryManager;
            InitializeComponents();
            LoadLookups();
        }

        private void InitializeComponents()
        {
            Text = "Добавление инвентаря";
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
            DataTable cats = inventoryManager.GetCategories();
            cbCategory.DataSource = cats;
            cbCategory.DisplayMember = "CategoryName";
            cbCategory.ValueMember = "CategoryID";

            DataTable rooms = inventoryManager.GetClassrooms();
            cbClassroom.DataSource = rooms;
            cbClassroom.DisplayMember = "Name";
            cbClassroom.ValueMember = "ClassroomID";

            DataTable people = inventoryManager.GetResponsiblePersons();
            cbPerson.DataSource = people;
            cbPerson.DisplayMember = "Name";
            cbPerson.ValueMember = "PersonID";
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
                bool ok = inventoryManager.AddInventoryItem(
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
                    MessageBox.Show("Не удалось сохранить запись.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}


