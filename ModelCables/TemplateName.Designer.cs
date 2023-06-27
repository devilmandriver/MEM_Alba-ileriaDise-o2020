namespace MEM_AlbañileriaDiseño
{
    partial class TemplateName
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textTemplateName = new System.Windows.Forms.TextBox();
            this.buttonSaveName = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textTemplateName
            // 
            this.textTemplateName.Location = new System.Drawing.Point(14, 26);
            this.textTemplateName.Name = "textTemplateName";
            this.textTemplateName.Size = new System.Drawing.Size(398, 20);
            this.textTemplateName.TabIndex = 0;
            // 
            // buttonSaveName
            // 
            this.buttonSaveName.Location = new System.Drawing.Point(172, 62);
            this.buttonSaveName.Name = "buttonSaveName";
            this.buttonSaveName.Size = new System.Drawing.Size(90, 26);
            this.buttonSaveName.TabIndex = 1;
            this.buttonSaveName.Text = "Guardar";
            this.buttonSaveName.UseVisualStyleBackColor = true;
            this.buttonSaveName.Click += new System.EventHandler(this.buttonSaveName_Click);
            // 
            // TemplateName
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(435, 100);
            this.Controls.Add(this.buttonSaveName);
            this.Controls.Add(this.textTemplateName);
            this.Name = "TemplateName";
            this.Text = "TemplateName";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textTemplateName;
        private System.Windows.Forms.Button buttonSaveName;
    }
}