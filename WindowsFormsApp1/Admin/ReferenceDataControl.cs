using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using WindowsFormsApp1;
using WindowsFormsApp1.UI;
using WindowsFormsApp1.Utils;

namespace WindowsFormsApp1.Admin
{
    public enum ReferenceKind { Classrooms, ResponsiblePersons }

    public class ReferenceDataControl : UserControl
    {
        private readonly ReferenceKind _kind;
        private readonly DbController _db = new DbController();
        private DataGridView grid;
        private DataTable _data;

        public ReferenceDataControl(ReferenceKind kind)
        {
            _kind = kind;
            Dock = DockStyle.Fill;
            BuildUi();
            LoadData();
        }

        private void BuildUi()
        {
            BackColor = AppTheme.Background;
            var header = new Label
            {
                Text = _kind == ReferenceKind.Classrooms ? "Справочник кабинетов" : "Справочник ответственных лиц",
                Dock = DockStyle.Top,
                Height = 28,
                Font = AppTheme.FontHeader,
                ForeColor = AppTheme.Primary
            };
            var toolbar = new ToolbarPanel();
            var btnAdd = new Button { Text = "Добавить" };
            var btnEdit = new Button { Text = "Изменить" };
            var btnDel = new Button { Text = "Удалить" };
            var btnRef = new Button { Text = "Обновить" };
            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDel.Click += BtnDel_Click;
            btnRef.Click += (s, e) => LoadData();
            toolbar.AddButton(btnAdd, true);
            toolbar.AddButton(btnEdit);
            toolbar.AddButton(btnDel);
            toolbar.AddButton(btnRef);

            grid = new DataGridView { Dock = DockStyle.Fill };
            AppTheme.ApplyToDataGridView(grid);
            Controls.Add(grid);
            Controls.Add(toolbar);
            Controls.Add(header);
        }

        public void LoadData()
        {
            try
            {
                if (_kind == ReferenceKind.Classrooms)
                {
                    _data = _db.GetData("SELECT ClassroomID, RoomNumber AS [Номер], RoomName AS [Наименование] FROM Classrooms ORDER BY RoomNumber");
                }
                else
                {
                    _data = _db.GetData("SELECT PersonID, LastName AS [Фамилия], FirstName AS [Имя] FROM ResponsiblePersons ORDER BY LastName");
                }
                grid.DataSource = _data;
                GridHelper.HideTechnicalColumns(grid);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int? SelectedId()
        {
            if (grid.CurrentRow == null) return null;
            string col = _kind == ReferenceKind.Classrooms ? "ClassroomID" : "PersonID";
            return Convert.ToInt32(grid.CurrentRow.Cells[col].Value);
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (_kind == ReferenceKind.Classrooms)
            {
                string num = Prompt("Номер кабинета");
                string name = Prompt("Наименование");
                if (num == null) return;
                _db.ExecuteNonQuery("INSERT INTO Classrooms (RoomNumber, RoomName) VALUES (@n,@name)",
                    new[] { new SqlParameter("@n", num), new SqlParameter("@name", name ?? num) });
            }
            else
            {
                string ln = Prompt("Фамилия");
                string fn = Prompt("Имя");
                if (ln == null) return;
                _db.ExecuteNonQuery("INSERT INTO ResponsiblePersons (LastName, FirstName) VALUES (@l,@f)",
                    new[] { new SqlParameter("@l", ln), new SqlParameter("@f", fn ?? "") });
            }
            LoadData();
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            int? id = SelectedId();
            if (!id.HasValue) return;
            if (_kind == ReferenceKind.Classrooms)
            {
                string num = Prompt("Номер кабинета");
                string name = Prompt("Наименование");
                if (num == null) return;
                _db.ExecuteNonQuery("UPDATE Classrooms SET RoomNumber=@n, RoomName=@name WHERE ClassroomID=@id",
                    new[] { new SqlParameter("@n", num), new SqlParameter("@name", name ?? num), new SqlParameter("@id", id.Value) });
            }
            else
            {
                string ln = Prompt("Фамилия");
                string fn = Prompt("Имя");
                if (ln == null) return;
                _db.ExecuteNonQuery("UPDATE ResponsiblePersons SET LastName=@l, FirstName=@f WHERE PersonID=@id",
                    new[] { new SqlParameter("@l", ln), new SqlParameter("@f", fn ?? ""), new SqlParameter("@id", id.Value) });
            }
            LoadData();
        }

        private void BtnDel_Click(object sender, EventArgs e)
        {
            int? id = SelectedId();
            if (!id.HasValue) return;
            if (MessageBox.Show("Удалить запись?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            try
            {
                if (_kind == ReferenceKind.Classrooms)
                    _db.ExecuteNonQuery("DELETE FROM Classrooms WHERE ClassroomID=@id", new[] { new SqlParameter("@id", id.Value) });
                else
                    _db.ExecuteNonQuery("DELETE FROM ResponsiblePersons WHERE PersonID=@id", new[] { new SqlParameter("@id", id.Value) });
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось удалить: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static string Prompt(string title)
        {
            using (var f = new Form { Text = title, Size = new Size(360, 130), FormBorderStyle = FormBorderStyle.FixedDialog, StartPosition = FormStartPosition.CenterParent })
            {
                var tb = new TextBox { Left = 16, Top = 16, Width = 310 };
                var ok = new Button { Text = "OK", Left = 180, Top = 52, DialogResult = DialogResult.OK };
                f.Controls.AddRange(new Control[] { tb, ok });
                f.AcceptButton = ok;
                return f.ShowDialog() == DialogResult.OK ? tb.Text.Trim() : null;
            }
        }
    }
}
