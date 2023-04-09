
namespace PMC400_TEST
{
    partial class Form3
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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.button1 = new System.Windows.Forms.Button();
            this.bj = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.jg = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.zsl = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.qxx = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.bl = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.bj,
            this.jg,
            this.zsl,
            this.qxx,
            this.bl});
            this.dataGridView1.Location = new System.Drawing.Point(22, 30);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 23;
            this.dataGridView1.Size = new System.Drawing.Size(542, 523);
            this.dataGridView1.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(633, 30);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(152, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "球心像位置计算";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // bj
            // 
            this.bj.HeaderText = "曲率半径";
            this.bj.Name = "bj";
            // 
            // jg
            // 
            this.jg.HeaderText = "间隔";
            this.jg.Name = "jg";
            // 
            // zsl
            // 
            this.zsl.HeaderText = "折射率";
            this.zsl.Name = "zsl";
            // 
            // qxx
            // 
            this.qxx.HeaderText = "球心像位置";
            this.qxx.Name = "qxx";
            // 
            // bl
            // 
            this.bl.HeaderText = "倍率";
            this.bl.Name = "bl";
            // 
            // Form3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1059, 592);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.dataGridView1);
            this.Name = "Form3";
            this.Text = "Form3";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.DataGridViewTextBoxColumn bj;
        private System.Windows.Forms.DataGridViewTextBoxColumn jg;
        private System.Windows.Forms.DataGridViewTextBoxColumn zsl;
        private System.Windows.Forms.DataGridViewTextBoxColumn qxx;
        private System.Windows.Forms.DataGridViewTextBoxColumn bl;
    }
}