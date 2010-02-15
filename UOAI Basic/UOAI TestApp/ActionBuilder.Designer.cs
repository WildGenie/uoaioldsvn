namespace UOAI_TestApp
{
    partial class ActionBuilder
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
            this.ActionName = new System.Windows.Forms.TextBox();
            this.uintpar = new System.Windows.Forms.TextBox();
            this.enumpar = new System.Windows.Forms.ComboBox();
            this.boolpar = new System.Windows.Forms.CheckBox();
            this.actTypes = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.seqPar = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.actlist = new System.Windows.Forms.ComboBox();
            this.ontrue = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.onfalse = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.filterlist = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.knownpar2 = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // ActionName
            // 
            this.ActionName.Location = new System.Drawing.Point(57, 115);
            this.ActionName.Name = "ActionName";
            this.ActionName.Size = new System.Drawing.Size(181, 20);
            this.ActionName.TabIndex = 25;
            // 
            // uintpar
            // 
            this.uintpar.Location = new System.Drawing.Point(57, 89);
            this.uintpar.Name = "uintpar";
            this.uintpar.Size = new System.Drawing.Size(181, 20);
            this.uintpar.TabIndex = 24;
            // 
            // enumpar
            // 
            this.enumpar.FormattingEnabled = true;
            this.enumpar.Location = new System.Drawing.Point(57, 62);
            this.enumpar.Name = "enumpar";
            this.enumpar.Size = new System.Drawing.Size(181, 21);
            this.enumpar.TabIndex = 23;
            // 
            // boolpar
            // 
            this.boolpar.AutoSize = true;
            this.boolpar.Location = new System.Drawing.Point(57, 39);
            this.boolpar.Name = "boolpar";
            this.boolpar.Size = new System.Drawing.Size(116, 17);
            this.boolpar.TabIndex = 22;
            this.boolpar.Text = "Boolean Parameter";
            this.boolpar.UseVisualStyleBackColor = true;
            // 
            // actTypes
            // 
            this.actTypes.FormattingEnabled = true;
            this.actTypes.Location = new System.Drawing.Point(57, 12);
            this.actTypes.Name = "actTypes";
            this.actTypes.Size = new System.Drawing.Size(181, 21);
            this.actTypes.TabIndex = 21;
            this.actTypes.SelectedIndexChanged += new System.EventHandler(this.actTypes_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(27, 13);
            this.label1.TabIndex = 26;
            this.label1.Text = "type";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 65);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 13);
            this.label2.TabIndex = 27;
            this.label2.Text = "known";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 92);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(39, 13);
            this.label3.TabIndex = 28;
            this.label3.Text = "uintpar";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(18, 118);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(33, 13);
            this.label4.TabIndex = 29;
            this.label4.Text = "name";
            // 
            // seqPar
            // 
            this.seqPar.FormattingEnabled = true;
            this.seqPar.Location = new System.Drawing.Point(57, 141);
            this.seqPar.Name = "seqPar";
            this.seqPar.Size = new System.Drawing.Size(128, 21);
            this.seqPar.TabIndex = 30;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(27, 144);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(24, 13);
            this.label5.TabIndex = 31;
            this.label5.Text = "seq";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(196, 142);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(47, 19);
            this.button1.TabIndex = 32;
            this.button1.Text = "...";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(185, 302);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(53, 23);
            this.button2.TabIndex = 33;
            this.button2.Text = "OK";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button3.Location = new System.Drawing.Point(129, 303);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(50, 23);
            this.button3.TabIndex = 34;
            this.button3.Text = "Cancel";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // actlist
            // 
            this.actlist.FormattingEnabled = true;
            this.actlist.Location = new System.Drawing.Point(57, 168);
            this.actlist.Name = "actlist";
            this.actlist.Size = new System.Drawing.Size(128, 21);
            this.actlist.TabIndex = 35;
            // 
            // ontrue
            // 
            this.ontrue.FormattingEnabled = true;
            this.ontrue.Location = new System.Drawing.Point(57, 195);
            this.ontrue.Name = "ontrue";
            this.ontrue.Size = new System.Drawing.Size(128, 21);
            this.ontrue.TabIndex = 36;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(17, 168);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(34, 13);
            this.label6.TabIndex = 37;
            this.label6.Text = "actlist";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(14, 195);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(37, 13);
            this.label7.TabIndex = 38;
            this.label7.Text = "ontrue";
            // 
            // onfalse
            // 
            this.onfalse.FormattingEnabled = true;
            this.onfalse.Location = new System.Drawing.Point(57, 219);
            this.onfalse.Name = "onfalse";
            this.onfalse.Size = new System.Drawing.Size(128, 21);
            this.onfalse.TabIndex = 39;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(10, 222);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(41, 13);
            this.label8.TabIndex = 40;
            this.label8.Text = "onfalse";
            // 
            // filterlist
            // 
            this.filterlist.FormattingEnabled = true;
            this.filterlist.Location = new System.Drawing.Point(57, 248);
            this.filterlist.Name = "filterlist";
            this.filterlist.Size = new System.Drawing.Size(128, 21);
            this.filterlist.TabIndex = 41;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(29, 251);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(22, 13);
            this.label9.TabIndex = 42;
            this.label9.Text = "flist";
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(196, 168);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(47, 19);
            this.button4.TabIndex = 43;
            this.button4.Text = "...";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(196, 195);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(47, 19);
            this.button5.TabIndex = 44;
            this.button5.Text = "...";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(196, 220);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(47, 19);
            this.button6.TabIndex = 45;
            this.button6.Text = "...";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(196, 248);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(47, 19);
            this.button7.TabIndex = 46;
            this.button7.Text = "...";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // knownpar2
            // 
            this.knownpar2.FormattingEnabled = true;
            this.knownpar2.Location = new System.Drawing.Point(57, 275);
            this.knownpar2.Name = "knownpar2";
            this.knownpar2.Size = new System.Drawing.Size(181, 21);
            this.knownpar2.TabIndex = 47;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(7, 278);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(45, 13);
            this.label10.TabIndex = 48;
            this.label10.Text = "known2";
            // 
            // ActionBuilder
            // 
            this.AcceptButton = this.button2;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button3;
            this.ClientSize = new System.Drawing.Size(250, 331);
            this.ControlBox = false;
            this.Controls.Add(this.label10);
            this.Controls.Add(this.knownpar2);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.filterlist);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.onfalse);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.ontrue);
            this.Controls.Add(this.actlist);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.seqPar);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ActionName);
            this.Controls.Add(this.uintpar);
            this.Controls.Add(this.enumpar);
            this.Controls.Add(this.boolpar);
            this.Controls.Add(this.actTypes);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "ActionBuilder";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ActionBuilder";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox ActionName;
        private System.Windows.Forms.TextBox uintpar;
        private System.Windows.Forms.ComboBox enumpar;
        private System.Windows.Forms.CheckBox boolpar;
        private System.Windows.Forms.ComboBox actTypes;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox seqPar;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.ComboBox actlist;
        private System.Windows.Forms.ComboBox ontrue;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox onfalse;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox filterlist;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.ComboBox knownpar2;
        private System.Windows.Forms.Label label10;
    }
}