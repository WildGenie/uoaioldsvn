namespace UOAI_TestApp
{
    partial class FilterBuilder
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
            this.chkName = new System.Windows.Forms.TextBox();
            this.chkDword2 = new System.Windows.Forms.TextBox();
            this.chkDword1 = new System.Windows.Forms.TextBox();
            this.chEnumPar = new System.Windows.Forms.ComboBox();
            this.chkFilterType = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.lstFilterLists = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // chkName
            // 
            this.chkName.Location = new System.Drawing.Point(61, 120);
            this.chkName.Name = "chkName";
            this.chkName.Size = new System.Drawing.Size(191, 20);
            this.chkName.TabIndex = 12;
            // 
            // chkDword2
            // 
            this.chkDword2.Location = new System.Drawing.Point(61, 94);
            this.chkDword2.Name = "chkDword2";
            this.chkDword2.Size = new System.Drawing.Size(191, 20);
            this.chkDword2.TabIndex = 11;
            // 
            // chkDword1
            // 
            this.chkDword1.Location = new System.Drawing.Point(61, 68);
            this.chkDword1.Name = "chkDword1";
            this.chkDword1.Size = new System.Drawing.Size(191, 20);
            this.chkDword1.TabIndex = 10;
            // 
            // chEnumPar
            // 
            this.chEnumPar.FormattingEnabled = true;
            this.chEnumPar.Location = new System.Drawing.Point(61, 41);
            this.chEnumPar.Name = "chEnumPar";
            this.chEnumPar.Size = new System.Drawing.Size(191, 21);
            this.chEnumPar.TabIndex = 9;
            // 
            // chkFilterType
            // 
            this.chkFilterType.FormattingEnabled = true;
            this.chkFilterType.Location = new System.Drawing.Point(61, 14);
            this.chkFilterType.Name = "chkFilterType";
            this.chkFilterType.Size = new System.Drawing.Size(191, 21);
            this.chkFilterType.TabIndex = 8;
            this.chkFilterType.SelectedIndexChanged += new System.EventHandler(this.chkFilterType_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "Type";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "enumpar";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(18, 71);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(39, 13);
            this.label3.TabIndex = 15;
            this.label3.Text = "uintpar";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 97);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(45, 13);
            this.label4.TabIndex = 16;
            this.label4.Text = "uintpar2";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(24, 123);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(33, 13);
            this.label5.TabIndex = 17;
            this.label5.Text = "name";
            // 
            // lstFilterLists
            // 
            this.lstFilterLists.FormattingEnabled = true;
            this.lstFilterLists.Location = new System.Drawing.Point(61, 146);
            this.lstFilterLists.Name = "lstFilterLists";
            this.lstFilterLists.Size = new System.Drawing.Size(159, 21);
            this.lstFilterLists.TabIndex = 18;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(17, 149);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(38, 13);
            this.label6.TabIndex = 19;
            this.label6.Text = "filterlist";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(226, 146);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(26, 20);
            this.button1.TabIndex = 20;
            this.button1.Text = "...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(217, 172);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(38, 23);
            this.button2.TabIndex = 21;
            this.button2.Text = "ok";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button3.Location = new System.Drawing.Point(148, 173);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(63, 22);
            this.button3.TabIndex = 22;
            this.button3.Text = "Cancel";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // FilterBuilder
            // 
            this.AcceptButton = this.button2;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button3;
            this.ClientSize = new System.Drawing.Size(267, 204);
            this.ControlBox = false;
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.lstFilterLists);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.chkName);
            this.Controls.Add(this.chkDword2);
            this.Controls.Add(this.chkDword1);
            this.Controls.Add(this.chEnumPar);
            this.Controls.Add(this.chkFilterType);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "FilterBuilder";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Filter";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox chkName;
        private System.Windows.Forms.TextBox chkDword2;
        private System.Windows.Forms.TextBox chkDword1;
        private System.Windows.Forms.ComboBox chEnumPar;
        private System.Windows.Forms.ComboBox chkFilterType;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox lstFilterLists;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
    }
}