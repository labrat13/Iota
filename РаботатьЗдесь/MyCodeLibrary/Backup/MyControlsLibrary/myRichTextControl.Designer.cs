namespace MyControlsLibrary
{
    partial class myRichTextControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.contextMenuStrip_RichEdit = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem_Undo = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_Redo = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_Cut = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_Copy = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_Paste = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_SelectAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_Delete = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_Cut = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_Copy = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_Paste = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_Undo = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_Redo = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_Date = new System.Windows.Forms.ToolStripButton();
            this.contextMenuStrip_RichEdit.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.richTextBox1.AcceptsTab = true;
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox1.ContextMenuStrip = this.contextMenuStrip_RichEdit;
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.Location = new System.Drawing.Point(3, 28);
            this.richTextBox1.MinimumSize = new System.Drawing.Size(50, 50);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ShortcutsEnabled = false;
            this.richTextBox1.Size = new System.Drawing.Size(327, 195);
            this.richTextBox1.TabIndex = 1;
            this.richTextBox1.Text = "";
            this.richTextBox1.SelectionChanged += new System.EventHandler(this.richTextBox1_SelectionChanged);
            this.richTextBox1.TextChanged += new System.EventHandler(this.richTextBox1_TextChanged);
            // 
            // contextMenuStrip_RichEdit
            // 
            this.contextMenuStrip_RichEdit.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_Undo,
            this.toolStripMenuItem_Redo,
            this.toolStripSeparator1,
            this.toolStripMenuItem_Cut,
            this.toolStripMenuItem_Copy,
            this.toolStripMenuItem_Paste,
            this.toolStripSeparator2,
            this.toolStripMenuItem_SelectAll,
            this.toolStripMenuItem_Delete});
            this.contextMenuStrip_RichEdit.Name = "contextMenuStrip_RichEdit";
            this.contextMenuStrip_RichEdit.Size = new System.Drawing.Size(153, 192);
            this.contextMenuStrip_RichEdit.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip_RichEdit_Opening);
            // 
            // toolStripMenuItem_Undo
            // 
            this.toolStripMenuItem_Undo.Image = global::MyControlsLibrary.Properties.Resources.Edit_Undo;
            this.toolStripMenuItem_Undo.Name = "toolStripMenuItem_Undo";
            this.toolStripMenuItem_Undo.Size = new System.Drawing.Size(152, 22);
            this.toolStripMenuItem_Undo.Text = "Откатить";
            this.toolStripMenuItem_Undo.Click += new System.EventHandler(this.toolStripMenuItem_Undo_Click);
            // 
            // toolStripMenuItem_Redo
            // 
            this.toolStripMenuItem_Redo.Image = global::MyControlsLibrary.Properties.Resources.Edit_Redo;
            this.toolStripMenuItem_Redo.Name = "toolStripMenuItem_Redo";
            this.toolStripMenuItem_Redo.Size = new System.Drawing.Size(152, 22);
            this.toolStripMenuItem_Redo.Text = "Вернуть";
            this.toolStripMenuItem_Redo.Click += new System.EventHandler(this.ToolStripMenuItem_Redo_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(149, 6);
            // 
            // toolStripMenuItem_Cut
            // 
            this.toolStripMenuItem_Cut.Image = global::MyControlsLibrary.Properties.Resources.Cut;
            this.toolStripMenuItem_Cut.Name = "toolStripMenuItem_Cut";
            this.toolStripMenuItem_Cut.Size = new System.Drawing.Size(152, 22);
            this.toolStripMenuItem_Cut.Text = "Вырезать";
            this.toolStripMenuItem_Cut.Click += new System.EventHandler(this.ToolStripMenuItem_Cut_Click);
            // 
            // toolStripMenuItem_Copy
            // 
            this.toolStripMenuItem_Copy.Image = global::MyControlsLibrary.Properties.Resources.Copy;
            this.toolStripMenuItem_Copy.Name = "toolStripMenuItem_Copy";
            this.toolStripMenuItem_Copy.Size = new System.Drawing.Size(152, 22);
            this.toolStripMenuItem_Copy.Text = "Скопировать";
            this.toolStripMenuItem_Copy.Click += new System.EventHandler(this.ToolStripMenuItem_Copy_Click);
            // 
            // toolStripMenuItem_Paste
            // 
            this.toolStripMenuItem_Paste.Image = global::MyControlsLibrary.Properties.Resources.Paste;
            this.toolStripMenuItem_Paste.Name = "toolStripMenuItem_Paste";
            this.toolStripMenuItem_Paste.Size = new System.Drawing.Size(152, 22);
            this.toolStripMenuItem_Paste.Text = "Вставить";
            this.toolStripMenuItem_Paste.Click += new System.EventHandler(this.ToolStripMenuItem_Paste_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(149, 6);
            // 
            // toolStripMenuItem_SelectAll
            // 
            this.toolStripMenuItem_SelectAll.Name = "toolStripMenuItem_SelectAll";
            this.toolStripMenuItem_SelectAll.Size = new System.Drawing.Size(152, 22);
            this.toolStripMenuItem_SelectAll.Text = "Выбрать все";
            this.toolStripMenuItem_SelectAll.Click += new System.EventHandler(this.ToolStripMenuItem_SelectAll_Click);
            // 
            // toolStripMenuItem_Delete
            // 
            this.toolStripMenuItem_Delete.Name = "toolStripMenuItem_Delete";
            this.toolStripMenuItem_Delete.Size = new System.Drawing.Size(152, 22);
            this.toolStripMenuItem_Delete.Text = "Удалить";
            this.toolStripMenuItem_Delete.Click += new System.EventHandler(this.ToolStripMenuItem_Delete_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.toolStrip1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.richTextBox1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(333, 226);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_Cut,
            this.toolStripButton_Copy,
            this.toolStripButton_Paste,
            this.toolStripSeparator3,
            this.toolStripButton_Undo,
            this.toolStripButton_Redo,
            this.toolStripSeparator4,
            this.toolStripButton_Date});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(333, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_Cut
            // 
            this.toolStripButton_Cut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_Cut.Image = global::MyControlsLibrary.Properties.Resources.Cut;
            this.toolStripButton_Cut.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_Cut.Name = "toolStripButton_Cut";
            this.toolStripButton_Cut.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_Cut.Text = "toolStripButton1";
            this.toolStripButton_Cut.ToolTipText = "Вырезать";
            this.toolStripButton_Cut.Click += new System.EventHandler(this.ToolStripMenuItem_Cut_Click);
            // 
            // toolStripButton_Copy
            // 
            this.toolStripButton_Copy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_Copy.Image = global::MyControlsLibrary.Properties.Resources.Copy;
            this.toolStripButton_Copy.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_Copy.Name = "toolStripButton_Copy";
            this.toolStripButton_Copy.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_Copy.Text = "toolStripButton1";
            this.toolStripButton_Copy.ToolTipText = "Скопировать";
            this.toolStripButton_Copy.Click += new System.EventHandler(this.ToolStripMenuItem_Copy_Click);
            // 
            // toolStripButton_Paste
            // 
            this.toolStripButton_Paste.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_Paste.Image = global::MyControlsLibrary.Properties.Resources.Paste;
            this.toolStripButton_Paste.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_Paste.Name = "toolStripButton_Paste";
            this.toolStripButton_Paste.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_Paste.Text = "toolStripButton2";
            this.toolStripButton_Paste.ToolTipText = "Вставить";
            this.toolStripButton_Paste.Click += new System.EventHandler(this.ToolStripMenuItem_Paste_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton_Undo
            // 
            this.toolStripButton_Undo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_Undo.Image = global::MyControlsLibrary.Properties.Resources.Edit_Undo;
            this.toolStripButton_Undo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_Undo.Name = "toolStripButton_Undo";
            this.toolStripButton_Undo.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_Undo.Text = "toolStripButton3";
            this.toolStripButton_Undo.ToolTipText = "Отменить";
            this.toolStripButton_Undo.Click += new System.EventHandler(this.toolStripMenuItem_Undo_Click);
            // 
            // toolStripButton_Redo
            // 
            this.toolStripButton_Redo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_Redo.Image = global::MyControlsLibrary.Properties.Resources.Edit_Redo;
            this.toolStripButton_Redo.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_Redo.Name = "toolStripButton_Redo";
            this.toolStripButton_Redo.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_Redo.Text = "toolStripButton1";
            this.toolStripButton_Redo.ToolTipText = "Повторить";
            this.toolStripButton_Redo.Click += new System.EventHandler(this.ToolStripMenuItem_Redo_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton_Date
            // 
            this.toolStripButton_Date.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_Date.Image = global::MyControlsLibrary.Properties.Resources.Date;
            this.toolStripButton_Date.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_Date.Name = "toolStripButton_Date";
            this.toolStripButton_Date.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_Date.Text = "toolStripButton4";
            this.toolStripButton_Date.ToolTipText = "Вставить дату";
            this.toolStripButton_Date.Click += new System.EventHandler(this.toolStripButton_Date_Click);
            // 
            // myRichTextControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "myRichTextControl";
            this.Size = new System.Drawing.Size(333, 226);
            this.contextMenuStrip_RichEdit.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_RichEdit;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Undo;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Redo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Cut;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Copy;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Paste;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_SelectAll;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Delete;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_Cut;
        private System.Windows.Forms.ToolStripButton toolStripButton_Copy;
        private System.Windows.Forms.ToolStripButton toolStripButton_Paste;
        private System.Windows.Forms.ToolStripButton toolStripButton_Undo;
        private System.Windows.Forms.ToolStripButton toolStripButton_Date;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton toolStripButton_Redo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
    }
}
