using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Globalization;

namespace MyControlsLibrary
{

    /// <summary>
    /// NFT- вроде готов и тестирован немного.
    /// Контрол ричтекстбокса с скрываемым тулбаром сверху и встроенным контекстным меню.
    /// С поддержкой натаскивания файлов как веб-ссылок и запуском их из контрола.
    /// Этот контрол для Инвентарь и подобных случаев.
    /// </summary>
    /// <remarks>
    /// В RichEditControl сейчас отключены комбинации клавиш, вроде Ctrl+C.
    /// Это потому что если их включить, пользователь будет вставлять файлы, а они вставляются контролом как OLE объекты.
    /// И не сохраняются потом, конечно, так как вставляются целиком, а не ссылками.
    /// Ввести комбинации клавиш в контекстное меню я пробовал, но они работают только после открытия меню, и больше никак.
    /// Поэтому я вообще отключил все комбинации клавиш для контрола, и можно работать только через контекстное меню или тулбар.
    /// </remarks>
    public partial class myRichTextControl : UserControl
    {
        //see ms-help://MS.VSCC.v90/MS.MSDNQTR.v90.en/dv_fxmclictl/html/3225f2ef-c6d9-4bd4-9d3e-2219e58edbf2.htm for RichText class
        
        //TODO: наделать кнопки для тулбара.
        //Для них нужны иконки, эти иконки тогда добавить и в контекстное меню тоже.
        //и потом надо динамически включать и выключать кнопки, как в контекстном меню в обработчиках выключаются пункты меню
        //чтобы включать и выключать кнопки тулбара, нужно тут использовать событие TextChanged


        public myRichTextControl()
        {
            InitializeComponent();

            //enable drag-drop for richtextbox
            this.richTextBox1.AllowDrop = true;
            this.richTextBox1.DragEnter += new DragEventHandler(richTextBox1_DragEnter);
            this.richTextBox1.DragDrop += new DragEventHandler(richTextBox1_DragDrop);
            this.richTextBox1.EnableAutoDragDrop = false;

            return;
        }


        /// <summary>
        /// RichTextBox контрол
        /// </summary>
        public RichTextBox TextBox
        {
            get { return this.richTextBox1; }
        }
        /// <summary>
        /// Показывать ли полосу кнопок над текстбоксом
        /// </summary>
        public bool ToolStrip_Visible
        {
            get { return this.toolStrip1.Visible; }
            set { this.toolStrip1.Visible = value; }
        }
        /// <summary>
        /// Read-only режим для текста
        /// </summary>
        public bool TextReadOnly
        {
            get { return this.richTextBox1.ReadOnly; }
            set { this.richTextBox1.ReadOnly = value; }
        }

        #region RichTextBox handlers

        private void richTextBox1_SelectionChanged(object sender, EventArgs e)
        {
            reenableToolBarButtons();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            reenableToolBarButtons();
        }



        void richTextBox1_DragDrop(object sender, DragEventArgs e)
        {
            //if textbox is readonly, abort content modification
            if (this.richTextBox1.ReadOnly == true) return;

            //paste from dragdrop data to control
            ClipboardManager2.RichTextBoxPaste(e.Data, this.richTextBox1);

            return;
        }

        void richTextBox1_DragEnter(object sender, DragEventArgs e)
        {

            //if textbox is readonly, abort content modification
            if (this.richTextBox1.ReadOnly == true)
                e.Effect = DragDropEffects.None;

            if (e.Data.GetDataPresent(DataFormats.Text)) //for texts
                e.Effect = DragDropEffects.Copy;
            else if (e.Data.GetDataPresent(DataFormats.FileDrop)) //for files from explorer
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        #endregion

        #region Context menu handlers and ToolBar handlers - готовы

        private void contextMenuStrip_RichEdit_Opening(object sender, CancelEventArgs e)
        {

            //проверить, есть ли в контроле выделенный текст
            bool isSelectedText = (this.richTextBox1.SelectionLength > 0);

            //if textbox is readonly, abort content modification
            //оказалось, контрол позволяет вставку, даже если установлен его режим ReadOnly
            if (this.richTextBox1.ReadOnly == true)
            {
                this.toolStripMenuItem_Redo.Enabled = false;
                this.toolStripMenuItem_Undo.Enabled = false;
                this.toolStripMenuItem_Paste.Enabled = false;
                this.toolStripMenuItem_Cut.Enabled = false;
                this.toolStripMenuItem_Copy.Enabled = isSelectedText;
                this.toolStripMenuItem_Delete.Enabled = false;
            }
            else
            {
                // check to see if we can cut/copy, paste anything; then enable/disable menu items
                // as required
                IDataObject iData = Clipboard.GetDataObject();

                //enable Paste item - включить вставку файлов 
                this.toolStripMenuItem_Paste.Enabled =
                    ((iData.GetDataPresent(DataFormats.Text, true)) || (iData.GetDataPresent(DataFormats.FileDrop, true)));

                this.toolStripMenuItem_Redo.Enabled = this.richTextBox1.CanRedo;
                this.toolStripMenuItem_Undo.Enabled = this.richTextBox1.CanUndo;
                this.toolStripMenuItem_Copy.Enabled = isSelectedText;
                this.toolStripMenuItem_Cut.Enabled = isSelectedText;
                this.toolStripMenuItem_Delete.Enabled = (this.richTextBox1.Text.Length > 0);

            }
        }

        private void toolStripMenuItem_Undo_Click(object sender, EventArgs e)
        {
            if (this.richTextBox1.ReadOnly == false)
                this.richTextBox1.Undo();
        }

        private void ToolStripMenuItem_Redo_Click(object sender, EventArgs e)
        {

            if (this.richTextBox1.ReadOnly == false)
                this.richTextBox1.Redo();
        }

        private void ToolStripMenuItem_Cut_Click(object sender, EventArgs e)
        {

            if (this.richTextBox1.ReadOnly == false)
                this.richTextBox1.Cut();

        }

        private void ToolStripMenuItem_Copy_Click(object sender, EventArgs e)
        {
            this.richTextBox1.Copy();

        }

        private void ToolStripMenuItem_Paste_Click(object sender, EventArgs e)
        {
            if (this.richTextBox1.ReadOnly == false)
            {
                //for texts
                IDataObject daob = Clipboard.GetDataObject();
                //вставить текст или ссылки на файлы
                ClipboardManager2.RichTextBoxPaste(daob, this.richTextBox1);
            }
        }

        private void ToolStripMenuItem_SelectAll_Click(object sender, EventArgs e)
        {
            this.richTextBox1.SelectAll();
        }

        private void ToolStripMenuItem_Delete_Click(object sender, EventArgs e)
        {
            if (this.richTextBox1.ReadOnly == false)
                this.richTextBox1.Clear();
        }

        /// <summary>
        /// Вставить дату в текущую позицию контрола
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton_Date_Click(object sender, EventArgs e)
        {
            String datext = DateTime.Now.ToString(CultureInfo.GetCultureInfo("ru-RU"));
            ClipboardManager2.RichTextBoxPaste(this.richTextBox1, datext + " ");
        }

        private void reenableToolBarButtons()
        {
            //тут включаем и выключаем кнопки тулбара, только если он виден.
            //и это тормозит работу - надо перерисовывать кнопки.
            if (this.toolStrip1.Visible == true)
            {
                //проверить, есть ли в контроле выделенный текст
                bool isSelectedText = (this.richTextBox1.SelectionLength > 0);
                //if textbox is readonly, abort content modification
                //оказалось, контрол позволяет вставку, даже если установлен его режим ReadOnly
                if (this.richTextBox1.ReadOnly == true)
                {
                    this.toolStripButton_Redo.Enabled = false;
                    this.toolStripButton_Undo.Enabled = false;
                    this.toolStripButton_Paste.Enabled = false;
                    this.toolStripButton_Cut.Enabled = false;
                    this.toolStripButton_Copy.Enabled = isSelectedText;
                    this.toolStripButton_Date.Enabled = false;
                }
                else
                {
                    // check to see if we can cut/copy, paste anything; then enable/disable menu items
                    // as required
                    IDataObject iData = Clipboard.GetDataObject();

                    //enable Paste item - включить вставку файлов 
                    this.toolStripButton_Paste.Enabled =
                        ((iData.GetDataPresent(DataFormats.Text, true)) || (iData.GetDataPresent(DataFormats.FileDrop, true)));

                    this.toolStripButton_Redo.Enabled = this.richTextBox1.CanRedo;
                    this.toolStripButton_Undo.Enabled = this.richTextBox1.CanUndo;
                    this.toolStripButton_Copy.Enabled = isSelectedText;
                    this.toolStripButton_Cut.Enabled = isSelectedText;
                    this.toolStripButton_Date.Enabled = true;

                }
            }
        }

        #endregion








    }
}

/* Пример использования в форме:

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            //set values to control
            this.myRichTextControl1.TextReadOnly = false;
            this.myRichTextControl1.TextBox.AppendText("Это текст для всяких текстовых целей\nОн свежий, сочный, и содержит веб-ссылки: \nms-help://MS.VSCC.v90/MS.MSDNQTR.v90.en/fxref_system.windows.forms/html/6b0155be-5a8d-971b-d5f2-5f739c4bb96b.htm\nИ они не работают.");
            //событие нельзя прицепить из дизайнера, создаем обработчик вручную.
            this.myRichTextControl1.TextBox.TextChanged += new EventHandler(TextBox_TextChanged);
            this.myRichTextControl1.TextBox.LinkClicked += new LinkClickedEventHandler(TextBox_LinkClicked);
            this.myRichTextControl1.TextBox.Protected += new EventHandler(TextBox_Protected);
        }
        //это событие если текст (часть текста) защищен от изменения, это в разметке RTF устанавливается тег
        void TextBox_Protected(object sender, EventArgs e)
        {
            MessageBox.Show("Text is protected");
        }

        void TextBox_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            MessageBox.Show("Link: " + e.LinkText);
        }

        void TextBox_TextChanged(object sender, EventArgs e)
        {
            //Application.DoEvents();
            //this.myRichTextControl1.TextBox.AppendText("text changed");
        }
    }
*/

