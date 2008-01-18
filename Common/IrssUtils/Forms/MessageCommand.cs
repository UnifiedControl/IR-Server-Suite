using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace IrssUtils.Forms
{

  /// <summary>
  /// Message Command form.
  /// </summary>
  public partial class MessageCommand : Form
  {

    #region Properties

    /// <summary>
    /// Gets the command string.
    /// </summary>
    /// <value>The command string.</value>
    public string CommandString
    {
      get
      {
        string target = "ERROR";

        if (radioButtonActiveWindow.Checked)
        {
          target = Common.TargetActive;
          textBoxMsgTarget.Text = "*";
        }
        else if (radioButtonApplication.Checked)
        {
          target = Common.TargetApplication;
        }
        else if (radioButtonClass.Checked)
        {
          target = Common.TargetClass;
        }
        else if (radioButtonWindowTitle.Checked)
        {
          target = Common.TargetWindow;
        }

        return String.Format("{0}|{1}|{2}|{3}|{4}",
          target,
          textBoxMsgTarget.Text,
          numericUpDownMsg.Value.ToString(),
          numericUpDownWParam.Value.ToString(),
          numericUpDownLParam.Value.ToString());
      }
    }

    #endregion Properties

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageCommand"/> class.
    /// </summary>
    public MessageCommand() : this(new string[] { Common.TargetActive, String.Empty, ((int)Win32.WindowsMessage.WM_USER).ToString(), "0", "0" }) { }
    /// <summary>
    /// Initializes a new instance of the <see cref="MessageCommand"/> class.
    /// </summary>
    /// <param name="commands">The command elements.</param>
    public MessageCommand(string[] commands)
    {
      InitializeComponent();

      if (commands != null)
      {
        string target = commands[0].ToUpperInvariant();
        switch (target)
        {
          case Common.TargetActive:       radioButtonActiveWindow.Checked = true;   break;
          case Common.TargetApplication:  radioButtonApplication.Checked  = true;   break;
          case Common.TargetClass:        radioButtonClass.Checked        = true;   break;
          case Common.TargetWindow:       radioButtonWindowTitle.Checked  = true;   break;
          default:
            throw new ArgumentOutOfRangeException("commands", commands[0], "Invalid message target");
        }

        textBoxMsgTarget.Text     = commands[1];
        numericUpDownMsg.Value    = decimal.Parse(commands[2]);
        numericUpDownWParam.Value = decimal.Parse(commands[3]);
        numericUpDownLParam.Value = decimal.Parse(commands[4]);
      }
    }

    #endregion Constructors

    #region Controls

    private void buttonFindMsgApp_Click(object sender, EventArgs e)
    {
      if (radioButtonApplication.Checked)
      {
        OpenFileDialog find = new OpenFileDialog();
        find.Filter = "All files|*.*";
        find.Multiselect = false;
        find.Title = "Application to send message to";

        if (find.ShowDialog(this) == DialogResult.OK)
            textBoxMsgTarget.Text = find.FileName;
      }
      else if (radioButtonClass.Checked)
      {
        // TODO: Locate Class
      }
      else if (radioButtonWindowTitle.Checked)
      {
        WindowList windowList = new WindowList();
        if (windowList.ShowDialog(this) == DialogResult.OK)
          textBoxMsgTarget.Text = windowList.SelectedWindowTitle;
      }
    }

    private void buttonOK_Click(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.OK;
      this.Close();
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
      this.Close();
    }

    private void wMAPPToolStripMenuItem_Click(object sender, EventArgs e)
    {
      numericUpDownMsg.Value = new decimal((int)Win32.WindowsMessage.WM_APP);
    }
    private void wMUSERToolStripMenuItem_Click(object sender, EventArgs e)
    {
      numericUpDownMsg.Value = new decimal((int)Win32.WindowsMessage.WM_USER);
    }

    private void radioButtonActiveWindow_CheckedChanged(object sender, EventArgs e)
    {
      buttonFindMsgTarget.Enabled = false;
      textBoxMsgTarget.Enabled = false;
    }
    private void radioButtonApplication_CheckedChanged(object sender, EventArgs e)
    {
      buttonFindMsgTarget.Enabled = true;
      textBoxMsgTarget.Enabled = true;
    }
    private void radioButtonClass_CheckedChanged(object sender, EventArgs e)
    {
      buttonFindMsgTarget.Enabled = false;
      textBoxMsgTarget.Enabled = true;
    }
    private void radioButtonWindowTitle_CheckedChanged(object sender, EventArgs e)
    {
      buttonFindMsgTarget.Enabled = true;
      textBoxMsgTarget.Enabled = true;
    }

    #endregion Controls

  }

}
