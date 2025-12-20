using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using WindowsFormsApp1.Utils;

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
		private ComboBox cbState;
		private Button btnOk;
		private Button btnCancel;
		private string photosFolder; // больше не используется, оставлено для совместимости

		public InventoryEditDialog(InventoryManager inventoryManager, int inventoryId)
		{
			this.inventoryManager = inventoryManager;
			this.inventoryId = inventoryId;
			InitializeComponents();
			LoadLookups();
			LoadEntity();
			this.KeyPreview = true;
			this.KeyDown += InventoryEditDialog_KeyDown;
		}

		private void InitializeComponents()
		{
			Text = "Изменение инвентаря";
			StartPosition = FormStartPosition.CenterParent;
			Size = new Size(900, 600);
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
			cbState = new ComboBox { Left = 180, Top = 336, Width = 470, DropDownStyle = ComboBoxStyle.DropDownList };
			cbState.Items.AddRange(new string[] { "Отличное", "Хорошее", "Удовлетворительное", "Требует ремонта", "Неисправное", "Списано" });

			btnOk = new Button { Text = "Сохранить", Left = 680, Top = 440, Width = 100, DialogResult = DialogResult.None };
			btnCancel = new Button { Text = "Отмена", Left = 790, Top = 480, Width = 90, DialogResult = DialogResult.Cancel };

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
				lblState, cbState,
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
			string currentState = Convert.ToString(r["CurrentState"]);
			if (!string.IsNullOrEmpty(currentState) && cbState.Items.Contains(currentState))
			{
				cbState.SelectedItem = currentState;
			}
			else if (cbState.Items.Count > 0)
			{
				cbState.SelectedIndex = 0;
			}

			// Фото более не поддерживается
		}

		private void BtnOk_Click(object sender, EventArgs e)
		{
			if (Utils.ValidationHelper.IsNullOrEmpty(txtName.Text))
			{
				MessageBox.Show("Введите наименование.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				txtName.Focus();
				return;
			}

			if (!Utils.ValidationHelper.IsValidInventoryNumber(txtInvNumber.Text))
			{
				MessageBox.Show("Инвентарный номер может содержать только буквы, цифры и дефисы.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				txtInvNumber.Focus();
				return;
			}

			if (!Utils.ValidationHelper.IsValidPrice(Convert.ToDecimal(numPrice.Value)))
			{
				MessageBox.Show("Цена должна быть в диапазоне от 0 до 100 000 000.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				numPrice.Focus();
				return;
			}

			try
			{
				Utils.Logger.Info($"Попытка обновления инвентаря с ID: {inventoryId}");
				bool ok = inventoryManager.UpdateInventoryItem(
					inventoryId,
					ValidationHelper.CleanString(txtName.Text),
					ValidationHelper.CleanString(txtDescription.Text),
					Convert.ToInt32(cbCategory.SelectedValue),
					Convert.ToInt32(cbClassroom.SelectedValue),
					Convert.ToInt32(cbPerson.SelectedValue),
					ValidationHelper.CleanString(txtInvNumber.Text),
					dpPurchaseDate.Value.Date,
					Convert.ToDecimal(numPrice.Value),
					cbState.SelectedItem?.ToString() ?? string.Empty
				);

				if (ok)
				{
					Utils.Logger.Info($"Инвентарь с ID {inventoryId} успешно обновлен: {txtName.Text}");
					DialogResult = DialogResult.OK;
					Close();
				}
				else
				{
					Utils.Logger.Warning($"Не удалось обновить запись инвентаря с ID: {inventoryId}");
					MessageBox.Show("Не удалось сохранить изменения.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
			catch (Exception ex)
			{
				Utils.Logger.Error($"Ошибка при обновлении инвентаря с ID: {inventoryId}", ex);
				MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void BtnLoadPhoto_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog openDialog = new OpenFileDialog())
			{
				openDialog.Filter = "Изображения (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif|Все файлы (*.*)|*.*";
				openDialog.Title = "Выберите фотографию";

				if (openDialog.ShowDialog() == DialogResult.OK)
				{
					try
					{
						photoPath = openDialog.FileName;
						pictureBox.Image = Image.FromFile(photoPath);
					}
					catch (Exception ex)
					{
						MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}
		}

		private void BtnRemovePhoto_Click(object sender, EventArgs e)
		{
			photoPath = null;
			pictureBox.Image = null;
		}

		private void InventoryEditDialog_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F1)
			{
				using (HelpForm helpForm = new HelpForm(HelpContext.EditInventory))
				{
					helpForm.ShowDialog(this);
				}
				e.Handled = true;
			}
		}

		private string photoPath;
		private PictureBox pictureBox;
	}
}


