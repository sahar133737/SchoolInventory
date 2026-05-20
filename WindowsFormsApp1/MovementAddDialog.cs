using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using WindowsFormsApp1.Services;
using WindowsFormsApp1.UI;
using WindowsFormsApp1.Utils;

namespace WindowsFormsApp1
{
    public class DispositionOperationDialog : Form
    {
        private readonly InventoryManager _inventory;
        private readonly MovementManager _movements;
        private readonly string _performedBy;
        private readonly string _userRole;
        private bool _isLoading;

        private ComboBox cbInventory;
        private ComboBox cbType;
        private ComboBox cbFromRoom;
        private ComboBox cbToRoom;
        private ComboBox cbFromPerson;
        private ComboBox cbToPerson;
        private DateTimePicker dtpDate;
        private TextBox txtDoc;
        private TextBox txtReason;
        private Label lblCurrentLocation;
        private Panel panelToRoom;
        private Panel panelToPerson;
        private Panel panelFromRoom;
        private Panel panelFromPerson;
        private Button btnOk;

        public DispositionOperationDialog(InventoryManager inventory, MovementManager movements, string performedBy, string userRole)
        {
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _movements = movements ?? throw new ArgumentNullException(nameof(movements));
            _performedBy = performedBy ?? "";
            _userRole = userRole ?? "";

            if (!AuthorizationHelper.CanRegisterDisposition(_userRole))
                throw new UnauthorizedAccessException("Недостаточно прав для оформления распоряжений.");

            Text = "Оформление распоряжения имуществом";
            Size = new Size(640, 540);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false;
            BackColor = AppTheme.Background;

            try
            {
                BuildUi();
                LoadLookups();
            }
            catch (Exception ex)
            {
                ExceptionHandler.ShowError("Не удалось открыть форму распоряжения", ex, this);
                Load += DispositionDialog_LoadAbort;
            }
        }

        private void DispositionDialog_LoadAbort(object sender, EventArgs e)
        {
            Load -= DispositionDialog_LoadAbort;
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void BuildUi()
        {
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(12) };
            int y = 8, gap = 36;

            void AddLabel(string t, int top) =>
                scroll.Controls.Add(new Label { Text = t, Left = 12, Top = top, Width = 170, Font = AppTheme.FontUi });
            void AddCtrl(Control c, int top) { c.Left = 190; c.Top = top; c.Width = 400; scroll.Controls.Add(c); }

            AddLabel("Имущество:", y);
            cbInventory = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cbInventory.SelectedIndexChanged += CbInventory_SelectedIndexChanged;
            AddCtrl(cbInventory, y);
            y += gap;

            lblCurrentLocation = new Label
            {
                Left = 190,
                Top = y,
                Width = 400,
                Height = 36,
                Font = AppTheme.FontUi,
                ForeColor = AppTheme.TextSecondary
            };
            scroll.Controls.Add(lblCurrentLocation);
            y += 40;

            AddLabel("Вид распоряжения:", y);
            cbType = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            cbType.SelectedIndexChanged += (s, e) => { if (!_isLoading) UpdateFieldsForType(); };
            AddCtrl(cbType, y);
            y += gap;

            panelFromRoom = AddComboRow(scroll, "Кабинет (было):", ref cbFromRoom, y, out y);
            panelToRoom = AddComboRow(scroll, "Кабинет (стало):", ref cbToRoom, y, out y);
            panelFromPerson = AddComboRow(scroll, "МОЛ (было):", ref cbFromPerson, y, out y);
            panelToPerson = AddComboRow(scroll, "МОЛ (стало):", ref cbToPerson, y, out y);

            AddLabel("Дата распоряжения:", y);
            dtpDate = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today,
                MinDate = new DateTime(1990, 1, 1),
                MaxDate = DateTime.Today
            };
            AddCtrl(dtpDate, y);
            y += gap;

            AddLabel("№ распоряжения / акта:", y);
            txtDoc = new TextBox();
            AppTheme.ApplyToTextBox(txtDoc);
            InputMaskHelper.ApplyDocumentNumberMask(txtDoc);
            AddCtrl(txtDoc, y);
            y += gap;

            AddLabel("Основание:", y);
            txtReason = new TextBox { Height = 56, Multiline = true };
            AppTheme.ApplyToTextBox(txtReason);
            InputMaskHelper.ApplyMultilineTextMask(txtReason);
            AddCtrl(txtReason, y);
            y += 64;

            btnOk = new Button { Text = "Оформить", Left = 390, Top = y, Width = 100, DialogResult = DialogResult.None };
            var btnCancel = new Button { Text = "Отмена", Left = 500, Top = y, Width = 90, DialogResult = DialogResult.Cancel };
            AppTheme.ApplyToButton(btnOk, true);
            AppTheme.ApplyToButton(btnCancel);
            btnOk.Click += BtnOk_Click;
            scroll.Controls.Add(btnOk);
            scroll.Controls.Add(btnCancel);

            Controls.Add(scroll);
            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }

        private static Panel AddComboRow(Panel parent, string label, ref ComboBox cb, int y, out int nextY)
        {
            var panel = new Panel { Left = 0, Top = y, Width = 620, Height = 34 };
            parent.Controls.Add(new Label { Text = label, Left = 12, Top = 6, Width = 170, Font = AppTheme.FontUi });
            cb = new ComboBox { Left = 190, Top = 4, Width = 400, DropDownStyle = ComboBoxStyle.DropDownList };
            AppTheme.ApplyToComboBox(cb);
            panel.Controls.Add(cb);
            parent.Controls.Add(panel);
            nextY = y + 36;
            return panel;
        }

        private void LoadLookups()
        {
            _isLoading = true;
            try
            {
                _movements.EnsureSchema();

                var invDisplay = new DataTable();
                invDisplay.Columns.Add("Id", typeof(int));
                invDisplay.Columns.Add("Display", typeof(string));

                var inv = _inventory.GetAllInventory();
                foreach (DataRow r in inv.Rows)
                {
                    if (string.Equals(r["Status"]?.ToString(), "Списан", StringComparison.OrdinalIgnoreCase))
                        continue;
                    invDisplay.Rows.Add(r["InventoryID"], $"{r["InventoryNumber"]} — {r["ItemName"]}");
                }

                if (invDisplay.Rows.Count == 0)
                {
                    btnOk.Enabled = false;
                    lblCurrentLocation.Text = "Нет доступного имущества для распоряжения (все списано или реестр пуст).";
                    cbInventory.Enabled = false;
                }
                else
                {
                    ComboBoxHelper.Bind(cbInventory, invDisplay, "Display", "Id");
                    cbInventory.SelectedIndex = 0;
                }

                ComboBoxHelper.Bind(cbFromRoom, _inventory.GetClassrooms(), "Name", "ClassroomID", "— не указан —", 0);
                ComboBoxHelper.Bind(cbToRoom, _inventory.GetClassrooms(), "Name", "ClassroomID", "— не указан —", 0);
                ComboBoxHelper.Bind(cbFromPerson, _inventory.GetResponsiblePersons(), "Name", "PersonID", "— не указан —", 0);
                ComboBoxHelper.Bind(cbToPerson, _inventory.GetResponsiblePersons(), "Name", "PersonID", "— не указан —", 0);

                var types = _movements.GetMovementTypes();
                if (types.Rows.Count == 0)
                {
                    ExceptionHandler.ShowWarning("Справочник видов распоряжений пуст. Перезапустите приложение или обратитесь к администратору.", this);
                    btnOk.Enabled = false;
                }
                else
                {
                    ComboBoxHelper.Bind(cbType, types, "TypeName", "TypeID");
                    cbType.SelectedIndex = 0;
                }

                if (cbInventory.Enabled && cbInventory.Items.Count > 0)
                    LoadCurrentLocation();
                UpdateFieldsForType();
            }
            catch (Exception ex)
            {
                ExceptionHandler.ShowError("Ошибка загрузки данных формы", ex, this);
                btnOk.Enabled = false;
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void CbInventory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isLoading) return;
            LoadCurrentLocation();
        }

        private void LoadCurrentLocation()
        {
            try
            {
                int? id = ComboBoxHelper.GetSelectedInt(cbInventory);
                if (!id.HasValue)
                {
                    lblCurrentLocation.Text = "";
                    return;
                }

                var row = _movements.GetInventoryCurrentLocation(id.Value);
                if (row == null)
                {
                    lblCurrentLocation.Text = "Данные об объекте не найдены.";
                    return;
                }

                lblCurrentLocation.Text = $"Сейчас: кабинет — {FormatCell(row["ClassroomName"])}, МОЛ — {FormatCell(row["PersonName"])}, статус — {FormatCell(row["Status"])}";

                int? roomId = row["ClassroomID"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["ClassroomID"]);
                int? personId = row["ResponsiblePersonID"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["ResponsiblePersonID"]);
                ComboBoxHelper.SelectById(cbFromRoom, roomId);
                ComboBoxHelper.SelectById(cbFromPerson, personId);
            }
            catch (Exception ex)
            {
                lblCurrentLocation.Text = "Не удалось загрузить текущее местоположение.";
                Logger.Error("Ошибка загрузки местоположения имущества", ex);
            }
        }

        private static string FormatCell(object value) =>
            value == null || value == DBNull.Value || string.IsNullOrWhiteSpace(value.ToString()) ? "не указано" : value.ToString();

        private void UpdateFieldsForType()
        {
            string type = cbType.Text ?? "";
            bool isMove = type.IndexOf("Перемещение", StringComparison.OrdinalIgnoreCase) >= 0;
            bool isMol = type.IndexOf("МОЛ", StringComparison.OrdinalIgnoreCase) >= 0;
            bool isIssue = type.IndexOf("Выдача", StringComparison.OrdinalIgnoreCase) >= 0;
            bool isReturn = type.IndexOf("Возврат", StringComparison.OrdinalIgnoreCase) >= 0;
            bool isWriteOff = type.IndexOf("Списание", StringComparison.OrdinalIgnoreCase) >= 0;

            panelFromRoom.Visible = panelFromPerson.Visible = !isWriteOff;
            panelToRoom.Visible = isMove || isIssue || isReturn;
            panelToPerson.Visible = isMol || isIssue;

            if (isWriteOff)
            {
                panelToRoom.Visible = false;
                panelToPerson.Visible = false;
            }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            btnOk.Enabled = false;
            try
            {
                int? invId = ComboBoxHelper.GetSelectedInt(cbInventory);
                int? typeId = ComboBoxHelper.GetSelectedInt(cbType);
                string typeName = cbType.Text ?? "";
                int? fromRoom = ComboBoxHelper.GetSelectedInt(cbFromRoom);
                int? toRoom = ComboBoxHelper.GetSelectedInt(cbToRoom);
                int? fromPerson = ComboBoxHelper.GetSelectedInt(cbFromPerson);
                int? toPerson = ComboBoxHelper.GetSelectedInt(cbToPerson);
                string doc = InputMaskHelper.SanitizeText(txtDoc.Text, 50);
                string reason = InputMaskHelper.SanitizeText(txtReason.Text, 500);

                var validation = DispositionValidator.Validate(
                    invId, typeId, typeName, fromRoom, toRoom, fromPerson, toPerson,
                    dtpDate.Value, doc, reason);

                if (!validation.IsValid)
                {
                    ExceptionHandler.ShowWarning(validation.Message, this);
                    FocusField(validation.FirstInvalidField);
                    return;
                }

                bool ok = _movements.RegisterMovement(
                    invId.Value, typeId.Value,
                    fromRoom, toRoom, fromPerson, toPerson,
                    dtpDate.Value, doc, reason, _performedBy);

                if (ok)
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                    ExceptionHandler.ShowWarning("Не удалось сохранить распоряжение.", this);
            }
            catch (Exception ex)
            {
                ExceptionHandler.ShowError("Ошибка оформления распоряжения", ex, this);
            }
            finally
            {
                btnOk.Enabled = cbInventory.Enabled;
            }
        }

        private void FocusField(string field)
        {
            switch (field)
            {
                case "inventory": cbInventory.Focus(); break;
                case "type": cbType.Focus(); break;
                case "document": txtDoc.Focus(); break;
                case "date": dtpDate.Focus(); break;
                case "reason": txtReason.Focus(); break;
                case "toRoom": cbToRoom.Focus(); break;
                case "toPerson": cbToPerson.Focus(); break;
            }
        }
    }
}
