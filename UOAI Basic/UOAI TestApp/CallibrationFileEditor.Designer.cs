namespace UOAI_TestApp
{
    partial class CallibrationFileEditor
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
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.actionlists = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.actions = new System.Windows.Forms.ComboBox();
            this.sequences = new System.Windows.Forms.ComboBox();
            this.codeblocks = new System.Windows.Forms.ComboBox();
            this.filterlists = new System.Windows.Forms.ComboBox();
            this.filters = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(76, 11);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(76, 26);
            this.button1.TabIndex = 0;
            this.button1.Text = "save";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(158, 11);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(76, 25);
            this.button2.TabIndex = 1;
            this.button2.Text = "load";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // actionlists
            // 
            this.actionlists.FormattingEnabled = true;
            this.actionlists.Location = new System.Drawing.Point(76, 44);
            this.actionlists.Name = "actionlists";
            this.actionlists.Size = new System.Drawing.Size(158, 21);
            this.actionlists.TabIndex = 2;
            this.actionlists.SelectedIndexChanged += new System.EventHandler(this.actionlists_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 47);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "ActionLists";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(28, 72);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Actions";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(36, 183);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(34, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Filters";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(20, 154);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(50, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "FilterLists";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 99);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(61, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "Sequences";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 126);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(64, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "CodeBlocks";
            // 
            // actions
            // 
            this.actions.FormattingEnabled = true;
            this.actions.Location = new System.Drawing.Point(76, 69);
            this.actions.Name = "actions";
            this.actions.Size = new System.Drawing.Size(158, 21);
            this.actions.TabIndex = 9;
            this.actions.SelectedIndexChanged += new System.EventHandler(this.actions_SelectedIndexChanged);
            // 
            // sequences
            // 
            this.sequences.FormattingEnabled = true;
            this.sequences.Location = new System.Drawing.Point(76, 96);
            this.sequences.Name = "sequences";
            this.sequences.Size = new System.Drawing.Size(158, 21);
            this.sequences.TabIndex = 10;
            this.sequences.SelectedIndexChanged += new System.EventHandler(this.sequences_SelectedIndexChanged);
            // 
            // codeblocks
            // 
            this.codeblocks.FormattingEnabled = true;
            this.codeblocks.Location = new System.Drawing.Point(76, 123);
            this.codeblocks.Name = "codeblocks";
            this.codeblocks.Size = new System.Drawing.Size(158, 21);
            this.codeblocks.TabIndex = 11;
            this.codeblocks.SelectedIndexChanged += new System.EventHandler(this.codeblocks_SelectedIndexChanged);
            // 
            // filterlists
            // 
            this.filterlists.FormattingEnabled = true;
            this.filterlists.Location = new System.Drawing.Point(76, 151);
            this.filterlists.Name = "filterlists";
            this.filterlists.Size = new System.Drawing.Size(158, 21);
            this.filterlists.TabIndex = 12;
            this.filterlists.SelectedIndexChanged += new System.EventHandler(this.filterlists_SelectedIndexChanged);
            // 
            // filters
            // 
            this.filters.FormattingEnabled = true;
            this.filters.Location = new System.Drawing.Point(76, 180);
            this.filters.Name = "filters";
            this.filters.Size = new System.Drawing.Size(158, 21);
            this.filters.TabIndex = 13;
            this.filters.SelectedIndexChanged += new System.EventHandler(this.filters_SelectedIndexChanged);
            // 
            // CallibrationFileEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(248, 216);
            this.Controls.Add(this.filters);
            this.Controls.Add(this.filterlists);
            this.Controls.Add(this.codeblocks);
            this.Controls.Add(this.sequences);
            this.Controls.Add(this.actions);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.actionlists);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Name = "CallibrationFileEditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CallibrationFile Editor";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ComboBox actionlists;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox actions;
        private System.Windows.Forms.ComboBox sequences;
        private System.Windows.Forms.ComboBox codeblocks;
        private System.Windows.Forms.ComboBox filterlists;
        private System.Windows.Forms.ComboBox filters;
    }
}