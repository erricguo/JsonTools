namespace JsonTools
{
    partial class Form7
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
            this.cbo_Ver = new System.Windows.Forms.ComboBox();
            this.rtb01 = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tb_Date = new System.Windows.Forms.TextBox();
            this.btn_OK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cbo_Ver
            // 
            this.cbo_Ver.Font = new System.Drawing.Font("YaHei Consolas Hybrid", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cbo_Ver.FormattingEnabled = true;
            this.cbo_Ver.Location = new System.Drawing.Point(54, 20);
            this.cbo_Ver.Name = "cbo_Ver";
            this.cbo_Ver.Size = new System.Drawing.Size(110, 25);
            this.cbo_Ver.TabIndex = 0;
            this.cbo_Ver.SelectedIndexChanged += new System.EventHandler(this.cbo_Ver_SelectedIndexChanged);
            // 
            // rtb01
            // 
            this.rtb01.Font = new System.Drawing.Font("YaHei Consolas Hybrid", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.rtb01.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.rtb01.Location = new System.Drawing.Point(8, 55);
            this.rtb01.Name = "rtb01";
            this.rtb01.ReadOnly = true;
            this.rtb01.Size = new System.Drawing.Size(566, 164);
            this.rtb01.TabIndex = 1;
            this.rtb01.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("YaHei Consolas Hybrid", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(12, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "版號";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("YaHei Consolas Hybrid", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(198, 24);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 17);
            this.label2.TabIndex = 3;
            this.label2.Text = "更新日期";
            // 
            // tb_Date
            // 
            this.tb_Date.Font = new System.Drawing.Font("YaHei Consolas Hybrid", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tb_Date.Location = new System.Drawing.Point(268, 20);
            this.tb_Date.Name = "tb_Date";
            this.tb_Date.ReadOnly = true;
            this.tb_Date.Size = new System.Drawing.Size(82, 25);
            this.tb_Date.TabIndex = 4;
            // 
            // btn_OK
            // 
            this.btn_OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn_OK.Font = new System.Drawing.Font("YaHei Consolas Hybrid", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_OK.Location = new System.Drawing.Point(492, 20);
            this.btn_OK.Name = "btn_OK";
            this.btn_OK.Size = new System.Drawing.Size(82, 25);
            this.btn_OK.TabIndex = 5;
            this.btn_OK.Text = "離開";
            this.btn_OK.UseVisualStyleBackColor = true;
            // 
            // Form7
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(580, 224);
            this.Controls.Add(this.btn_OK);
            this.Controls.Add(this.tb_Date);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.rtb01);
            this.Controls.Add(this.cbo_Ver);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Form7";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "版本更新說明";
            this.Load += new System.EventHandler(this.Form7_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbo_Ver;
        private System.Windows.Forms.RichTextBox rtb01;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tb_Date;
        private System.Windows.Forms.Button btn_OK;
    }
}