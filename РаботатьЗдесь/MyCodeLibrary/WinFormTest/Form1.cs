using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WinFormTest
{
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
}
