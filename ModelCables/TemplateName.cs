using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MEM_AlbañileriaDiseño2020
{
    public partial class TemplateName : Form
    {

        public string nombre = "";
        public TemplateName()
        {
            InitializeComponent();
        }

        private void buttonSaveName_Click(object sender, EventArgs e)
        {
            nombre = textTemplateName.Text;
            TemplateName.ActiveForm.Close();
        }
    }
}
