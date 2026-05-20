using System.Windows.Forms;
using WindowsFormsApp1.Admin;
using WindowsFormsApp1.UI;

namespace WindowsFormsApp1
{
    public class ClassroomsForm : UserControl
    {
        private readonly ReferenceDataControl _content;

        public ClassroomsForm()
        {
            Dock = DockStyle.Fill;
            BackColor = AppTheme.Background;
            Padding = new Padding(12);
            _content = new ReferenceDataControl(ReferenceKind.Classrooms) { Dock = DockStyle.Fill };
            Controls.Add(_content);
        }

        public void LoadData() => _content.LoadData();
    }
}
