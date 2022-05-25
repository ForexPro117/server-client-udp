namespace ClientForm
{
    partial class Form2
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
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.WinLabel = new System.Windows.Forms.Label();
            this.LoseLabel = new System.Windows.Forms.Label();
            this.DrawLabel = new System.Windows.Forms.Label();
            this.PaperBtn = new System.Windows.Forms.Button();
            this.ScissorsBtn = new System.Windows.Forms.Button();
            this.RockBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.8F);
            this.button1.Location = new System.Drawing.Point(184, 76);
            this.button1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(123, 29);
            this.button1.TabIndex = 0;
            this.button1.Text = "Отключиться";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F);
            this.label1.Location = new System.Drawing.Point(26, 347);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(296, 18);
            this.label1.TabIndex = 1;
            this.label1.Text = "Для игры необходимо минимум 2 игрока";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F);
            this.label3.Location = new System.Drawing.Point(181, 56);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(129, 18);
            this.label3.TabIndex = 3;
            this.label3.Text = "Игроков онлайн: ";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F);
            this.label4.Location = new System.Drawing.Point(26, 10);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(84, 18);
            this.label4.TabIndex = 4;
            this.label4.Text = "Ваше имя: ";
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(26, 134);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(293, 210);
            this.richTextBox1.TabIndex = 8;
            this.richTextBox1.Text = "";
            // 
            // WinLabel
            // 
            this.WinLabel.AutoSize = true;
            this.WinLabel.Location = new System.Drawing.Point(26, 39);
            this.WinLabel.Name = "WinLabel";
            this.WinLabel.Size = new System.Drawing.Size(48, 13);
            this.WinLabel.TabIndex = 9;
            this.WinLabel.Text = "Побед:0";
            // 
            // LoseLabel
            // 
            this.LoseLabel.AutoSize = true;
            this.LoseLabel.Location = new System.Drawing.Point(26, 61);
            this.LoseLabel.Name = "LoseLabel";
            this.LoseLabel.Size = new System.Drawing.Size(74, 13);
            this.LoseLabel.TabIndex = 10;
            this.LoseLabel.Text = "Поражений:0";
            // 
            // DrawLabel
            // 
            this.DrawLabel.AutoSize = true;
            this.DrawLabel.Location = new System.Drawing.Point(26, 84);
            this.DrawLabel.Name = "DrawLabel";
            this.DrawLabel.Size = new System.Drawing.Size(52, 13);
            this.DrawLabel.TabIndex = 11;
            this.DrawLabel.Text = "Ничьих:0";
            // 
            // PaperBtn
            // 
            this.PaperBtn.BackgroundImage = global::ClientForm.Properties.Resources.sciss;
            this.PaperBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.PaperBtn.FlatAppearance.BorderSize = 0;
            this.PaperBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.PaperBtn.ForeColor = System.Drawing.SystemColors.Control;
            this.PaperBtn.Location = new System.Drawing.Point(237, 388);
            this.PaperBtn.Name = "PaperBtn";
            this.PaperBtn.Size = new System.Drawing.Size(73, 71);
            this.PaperBtn.TabIndex = 13;
            this.PaperBtn.UseVisualStyleBackColor = false;
            // 
            // ScissorsBtn
            // 
            this.ScissorsBtn.BackgroundImage = global::ClientForm.Properties.Resources.paper;
            this.ScissorsBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ScissorsBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ScissorsBtn.ForeColor = System.Drawing.SystemColors.Control;
            this.ScissorsBtn.Location = new System.Drawing.Point(132, 388);
            this.ScissorsBtn.Name = "ScissorsBtn";
            this.ScissorsBtn.Size = new System.Drawing.Size(73, 71);
            this.ScissorsBtn.TabIndex = 12;
            this.ScissorsBtn.UseVisualStyleBackColor = false;
            // 
            // RockBtn
            // 
            this.RockBtn.BackgroundImage = global::ClientForm.Properties.Resources.rock;
            this.RockBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.RockBtn.FlatAppearance.BorderSize = 0;
            this.RockBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.RockBtn.ForeColor = System.Drawing.SystemColors.Control;
            this.RockBtn.Location = new System.Drawing.Point(29, 388);
            this.RockBtn.Name = "RockBtn";
            this.RockBtn.Size = new System.Drawing.Size(74, 71);
            this.RockBtn.TabIndex = 5;
            this.RockBtn.UseVisualStyleBackColor = false;
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(345, 465);
            this.Controls.Add(this.PaperBtn);
            this.Controls.Add(this.ScissorsBtn);
            this.Controls.Add(this.RockBtn);
            this.Controls.Add(this.DrawLabel);
            this.Controls.Add(this.LoseLabel);
            this.Controls.Add(this.WinLabel);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button1);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "Form2";
            this.Text = "Игра КНБ";
            this.Load += new System.EventHandler(this.Form2_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button RockBtn;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Label WinLabel;
        private System.Windows.Forms.Label LoseLabel;
        private System.Windows.Forms.Label DrawLabel;
        private System.Windows.Forms.Button ScissorsBtn;
        private System.Windows.Forms.Button PaperBtn;
    }
}