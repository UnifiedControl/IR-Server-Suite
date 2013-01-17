#region Copyright (C) 2005-2009 Team MediaPortal

// Copyright (C) 2005-2009 Team MediaPortal
// http://www.team-mediaportal.com
// 
// This Program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2, or (at your option)
// any later version.
// 
// This Program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with GNU Make; see the file COPYING.  If not, write to
// the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
// http://www.gnu.org/copyleft/gpl.html

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using IrssCommands;
using IrssUtils;
using IrssUtils.Forms;
using BlastIrDelegate = IrssUtils.BlastIrDelegate;

namespace MediaCenterBlaster
{
  internal partial class StbSetup : UserControl
  {
    #region Constants

    private const string ParameterInfo =
      @"%1 = Current channel number digit (-1 for Select/Pre-Change)
%2 = Full channel number string";

    #endregion Constants

    #region Variables

    private readonly int _cardId;

    #endregion Variables

    #region Properties

    public int CardId
    {
      get { return _cardId; }
    }

    public int PauseTime
    {
      get { return Decimal.ToInt32(numericUpDownPauseTime.Value); }
    }

    public bool SendSelect
    {
      get { return checkBoxSendSelect.Checked; }
    }

    public bool DoubleChannelSelect
    {
      get { return checkBoxDoubleSelect.Checked; }
    }

    public int RepeatChannelCommands
    {
      get { return Decimal.ToInt32(numericUpDownRepeat.Value); }
    }

    public int ChannelDigits
    {
      get
      {
        int chDigits = comboBoxChDigits.SelectedIndex;
        if (chDigits > 0)
          chDigits++;
        return chDigits;
      }
    }

    public int RepeatPauseTime
    {
      get { return Decimal.ToInt32(numericUpDownRepeatDelay.Value); }
    }

    public bool UsePreChangeCommand
    {
      get { return checkBoxUsePreChange.Checked; }
    }

    public string[] Digits
    {
      get
      {
        string[] _digits = new string[10];
        for (int i = 0; i < 10; i++)
          _digits[i] = listViewExternalCommands.Items[i].SubItems[1].Text;
        return _digits;
      }
    }

    public string SelectCommand
    {
      get { return listViewExternalCommands.Items[10].SubItems[1].Text; }
    }

    public string PreChangeCommand
    {
      get { return listViewExternalCommands.Items[11].SubItems[1].Text; }
    }

    #endregion Properties

    #region Constructor

    public StbSetup(int cardId)
    {
      InitializeComponent();

      _cardId = cardId;

      // setup command list
      string[] categoryList = new string[] { Processor.CategorySpecial, Processor.CategoryGeneral };
      PopulateCommandList(categoryList);

      // Setup commands combo box
      //comboBoxCommands.Items.Add(Common.UITextRun);
      //comboBoxCommands.Items.Add(Common.UITextSerial);
      //comboBoxCommands.Items.Add(Common.UITextWindowMsg);
      //comboBoxCommands.Items.Add(Common.UITextTcpMsg);
      //comboBoxCommands.Items.Add(Common.UITextHttpMsg);
      //comboBoxCommands.Items.Add(Common.UITextKeys);
      //comboBoxCommands.Items.Add(Common.UITextPopup);

      //string[] fileList = Tray.GetFileList(true);
      //if (fileList != null)
      //  comboBoxCommands.Items.AddRange(fileList);

      //comboBoxCommands.SelectedIndex = 0;

      // Setup command list
      ListViewItem item;
      string[] subItems = new string[2];
      for (int i = 0; i < 10; i++)
      {
        subItems[0] = "Digit " + i.ToString();
        subItems[1] = String.Empty;
        item = new ListViewItem(subItems);
        listViewExternalCommands.Items.Add(item);
      }

      subItems[0] = "Select";
      subItems[1] = String.Empty;
      item = new ListViewItem(subItems);
      listViewExternalCommands.Items.Add(item);

      subItems[0] = "PreChange";
      subItems[1] = String.Empty;
      item = new ListViewItem(subItems);
      listViewExternalCommands.Items.Add(item);

      SetToCard();
    }

    #endregion Constructor

    #region Public Methods

    // Set From Config
    public void SetToCard()
    {
      ExternalChannelConfig config = Tray.ExtChannelConfig;

      // Setup command list.
      for (int i = 0; i < 10; i++)
        listViewExternalCommands.Items[i].SubItems[1].Text = config.Digits[i];

      listViewExternalCommands.Items[10].SubItems[1].Text = config.SelectCommand;
      listViewExternalCommands.Items[11].SubItems[1].Text = config.PreChangeCommand;

      // Setup options.
      numericUpDownPauseTime.Value = config.PauseTime;
      checkBoxSendSelect.Checked = config.SendSelect;
      checkBoxDoubleSelect.Checked = config.DoubleChannelSelect;
      numericUpDownRepeat.Value = config.RepeatChannelCommands;

      checkBoxDoubleSelect.Enabled = checkBoxSendSelect.Checked;

      int channelDigitsSelect = config.ChannelDigits;
      if (channelDigitsSelect > 0)
        channelDigitsSelect--;
      comboBoxChDigits.SelectedIndex = channelDigitsSelect;

      checkBoxUsePreChange.Checked = config.UsePreChangeCommand;
      numericUpDownRepeatDelay.Value = new Decimal(config.RepeatPauseTime);
    }

    public void SetToConfig()
    {
      ExternalChannelConfig config = Tray.ExtChannelConfig;

      config.CardId = 0;

      config.PauseTime = Decimal.ToInt32(numericUpDownPauseTime.Value);
      config.SendSelect = checkBoxSendSelect.Checked;
      config.DoubleChannelSelect = checkBoxDoubleSelect.Checked;
      config.RepeatChannelCommands = Decimal.ToInt32(numericUpDownRepeat.Value);

      int chDigits = comboBoxChDigits.SelectedIndex;
      if (chDigits > 0)
        chDigits++;
      config.ChannelDigits = chDigits;

      config.RepeatPauseTime = Decimal.ToInt32(numericUpDownRepeatDelay.Value);
      config.UsePreChangeCommand = checkBoxUsePreChange.Checked;

      config.SelectCommand = listViewExternalCommands.Items[10].SubItems[1].Text;
      config.PreChangeCommand = listViewExternalCommands.Items[11].SubItems[1].Text;

      for (int i = 0; i < 10; i++)
        config.Digits[i] = listViewExternalCommands.Items[i].SubItems[1].Text;
    }

    public void SetToXml(string xmlFile)
    {
      if (xmlFile.Equals("Clear all", StringComparison.OrdinalIgnoreCase))
      {
        foreach (ListViewItem item in listViewExternalCommands.Items)
          item.SubItems[1].Text = String.Empty;

        return;
      }

      string fileName = Path.Combine(Common.FolderSTB, xmlFile + ".xml");

      XmlDocument doc = new XmlDocument();
      doc.Load(fileName);

      XmlNodeList nodeList = doc.DocumentElement.ChildNodes;

      string command;
      BlastCommand blastCommand;

      bool useForAllBlastCommands = false;
      string useForAllBlasterPort = String.Empty;

      int blastCommandCount = 0;
      for (int i = 0; i < 12; i++)
      {
        if (i == 10)
          command = XML.GetString(nodeList, "SelectCommand", String.Empty);
        else if (i == 11)
          command = XML.GetString(nodeList, "PreChangeCommand", String.Empty);
        else
          command = XML.GetString(nodeList, String.Format("Digit{0}", i), String.Empty);

        if (command.StartsWith(Common.CmdPrefixSTB, StringComparison.OrdinalIgnoreCase))
          blastCommandCount++;
      }

      for (int i = 0; i < 12; i++)
      {
        if (i == 10)
          command = XML.GetString(nodeList, "SelectCommand", String.Empty);
        else if (i == 11)
          command = XML.GetString(nodeList, "PreChangeCommand", String.Empty);
        else
          command = XML.GetString(nodeList, String.Format("Digit{0}", i), String.Empty);

        if (command.StartsWith(Common.CmdPrefixSTB, StringComparison.OrdinalIgnoreCase))
        {
          blastCommand = new BlastCommand(
            new BlastIrDelegate(Tray.BlastIR),
            Common.FolderSTB,
            Tray.TransceiverInformation.Ports,
            command.Substring(Common.CmdPrefixSTB.Length),
            blastCommandCount--);

          if (useForAllBlastCommands)
          {
            blastCommand.BlasterPort = useForAllBlasterPort;
            listViewExternalCommands.Items[i].SubItems[1].Text = Common.CmdPrefixSTB + blastCommand.CommandString;
          }
          else
          {
            if (blastCommand.ShowDialog(this) == DialogResult.OK)
            {
              if (blastCommand.UseForAll)
              {
                useForAllBlastCommands = true;
                useForAllBlasterPort = blastCommand.BlasterPort;
              }
              listViewExternalCommands.Items[i].SubItems[1].Text = Common.CmdPrefixSTB + blastCommand.CommandString;
            }
            else
            {
              blastCommand = new BlastCommand(
                new BlastIrDelegate(Tray.BlastIR),
                Common.FolderSTB,
                Tray.TransceiverInformation.Ports,
                command.Substring(Common.CmdPrefixSTB.Length));

              listViewExternalCommands.Items[i].SubItems[1].Text = Common.CmdPrefixSTB + blastCommand.CommandString;
            }
          }
        }
        else
        {
          listViewExternalCommands.Items[i].SubItems[1].Text = command;
        }
      }

      numericUpDownPauseTime.Value =
        new Decimal(XML.GetInt(nodeList, "PauseTime", Decimal.ToInt32(numericUpDownPauseTime.Value)));
      checkBoxUsePreChange.Checked = XML.GetBool(nodeList, "UsePreChangeCommand", checkBoxUsePreChange.Checked);
      checkBoxSendSelect.Checked = XML.GetBool(nodeList, "SendSelect", checkBoxSendSelect.Checked);
      checkBoxDoubleSelect.Checked = XML.GetBool(nodeList, "DoubleChannelSelect", checkBoxDoubleSelect.Checked);
      numericUpDownRepeat.Value =
        new Decimal(XML.GetInt(nodeList, "RepeatChannelCommands", Decimal.ToInt32(numericUpDownRepeat.Value)));
      numericUpDownRepeatDelay.Value =
        new Decimal(XML.GetInt(nodeList, "RepeatDelay", Decimal.ToInt32(numericUpDownRepeatDelay.Value)));

      int digitsWas = comboBoxChDigits.SelectedIndex;
      if (digitsWas > 0)
        digitsWas--;
      int digits = XML.GetInt(nodeList, "ChannelDigits", digitsWas);
      if (digits > 0)
        digits++;
      comboBoxChDigits.SelectedIndex = digits;
    }

    public void Save()
    {
      SetToConfig();
    }

    #endregion Public Methods

    #region Private Methods

    private void PopulateCommandList(string[] categories)
    {
      treeViewCommandList.Nodes.Clear();
      Dictionary<string, TreeNode> categoryNodes = new Dictionary<string, TreeNode>(categories.Length);

      // Create requested categories ...
      foreach (string category in categories)
      {
        TreeNode categoryNode = new TreeNode(category);
        //categoryNode.NodeFont = new Font(treeViewCommandList.Font, FontStyle.Underline);
        categoryNodes.Add(category, categoryNode);
      }

      List<Type> allCommands = new List<Type>();

      Type[] specialCommands = Processor.GetBuiltInCommands();
      allCommands.AddRange(specialCommands);

      Type[] libCommands = Processor.GetLibraryCommands();
      if (libCommands != null)
        allCommands.AddRange(libCommands);

      foreach (Type type in allCommands)
      {
        Command command = (Command)Activator.CreateInstance(type);

        string commandCategory = command.Category;

        if (categoryNodes.ContainsKey(commandCategory))
        {
          TreeNode newNode = new TreeNode(command.UserInterfaceText);
          newNode.Tag = type;

          categoryNodes[commandCategory].Nodes.Add(newNode);
        }
      }

      // Put all commands into tree view ...
      foreach (TreeNode treeNode in categoryNodes.Values)
        if (treeNode.Nodes.Count > 0)
          treeViewCommandList.Nodes.Add(treeNode);

      treeViewCommandList.SelectedNode = treeViewCommandList.Nodes[0];
      treeViewCommandList.SelectedNode.Expand();
    }

    private void treeViewCommandList_DoubleClick(object sender, EventArgs e)
    {
      if (treeViewCommandList.SelectedNode == null || treeViewCommandList.SelectedNode.Level == 0)
        return;

      Type commandType = treeViewCommandList.SelectedNode.Tag as Type;
      Command command = (Command)Activator.CreateInstance(commandType);

      if (!Tray.CommandProcessor.Edit(command, this)) return;

      listViewExternalCommands.SelectedItems[0].Tag = command;
      listViewExternalCommands.SelectedItems[0].SubItems[1].Text = command.UserDisplayText;
    }

    private void listViewExternalCommands_KeyDown(object sender, KeyEventArgs e)
    {
      if (listViewExternalCommands.SelectedIndices.Count > 0 && e.KeyCode == Keys.Delete)
        foreach (ListViewItem listViewItem in listViewExternalCommands.SelectedItems)
        {
          listViewItem.Tag = null;
          listViewItem.SubItems[1].Text = String.Empty;
        }
    }

    private void checkBoxSendSelect_CheckedChanged(object sender, EventArgs e)
    {
      checkBoxDoubleSelect.Enabled = checkBoxSendSelect.Checked;
    }

    private void listViewExternalCommands_DoubleClick(object sender, EventArgs e)
    {
      if (listViewExternalCommands.SelectedItems.Count != 1) return;

      ListViewItem item = listViewExternalCommands.SelectedItems[0];
      if (ReferenceEquals(item.Tag, null)) return;

      //if (item.Tag is MappedEvent)
      //{
      //  MessageBox.Show(this,
      //                  "The command is not available and can not be edited. Please check your commands directory in application folder or set a new command below.",
      //                  "Command unavailable", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      //  return;
      //}

      Command cmd = item.Tag as Command;
      if (ReferenceEquals(cmd, null)) return;

      if (!Tray.CommandProcessor.Edit(cmd, this)) return;

      item.Tag = cmd;
      item.SubItems[1].Text = cmd.UserDisplayText;

      //try
      //{
      //  string selected = listViewExternalCommands.SelectedItems[0].SubItems[1].Text;
      //  string newCommand = null;

        //if (selected.StartsWith(Common.CmdPrefixBlast, StringComparison.OrdinalIgnoreCase))
        //{
        //  string[] commands = Common.SplitBlastCommand(selected.Substring(Common.CmdPrefixBlast.Length));

        //  BlastCommand blastCommand = new BlastCommand(
        //    new BlastIrDelegate(Tray.BlastIR),
        //    Common.FolderIRCommands,
        //    Tray.TransceiverInformation.Ports,
        //    commands);

        //  if (blastCommand.ShowDialog(this) == DialogResult.OK)
        //    newCommand = Common.CmdPrefixBlast + blastCommand.CommandString;
        //}
        //else if (selected.StartsWith(Common.CmdPrefixSTB, StringComparison.OrdinalIgnoreCase))
        //{
        //  string[] commands = Common.SplitBlastCommand(selected.Substring(Common.CmdPrefixSTB.Length));

        //  BlastCommand blastCommand = new BlastCommand(
        //    new BlastIrDelegate(Tray.BlastIR),
        //    Common.FolderSTB,
        //    Tray.TransceiverInformation.Ports,
        //    commands);
        //  if (blastCommand.ShowDialog(this) == DialogResult.OK)
        //    newCommand = Common.CmdPrefixSTB + blastCommand.CommandString;
        //}






        //else if (selected.StartsWith(Common.CmdPrefixRun, StringComparison.OrdinalIgnoreCase))
        //{
        //  string[] commands = Common.SplitRunCommand(selected.Substring(Common.CmdPrefixRun.Length));

        //  ExternalProgram executeProgram = new ExternalProgram(commands, ParameterInfo);
        //  if (executeProgram.ShowDialog(this) == DialogResult.OK)
        //    newCommand = Common.CmdPrefixRun + executeProgram.CommandString;
        //}
        //else if (selected.StartsWith(Common.CmdPrefixSerial, StringComparison.OrdinalIgnoreCase))
        //{
        //  string[] commands = Common.SplitSerialCommand(selected.Substring(Common.CmdPrefixSerial.Length));

        //  SerialCommand serialCommand = new SerialCommand(commands, ParameterInfo);
        //  if (serialCommand.ShowDialog(this) == DialogResult.OK)
        //    newCommand = Common.CmdPrefixSerial + serialCommand.CommandString;
        //}
        //else if (selected.StartsWith(Common.CmdPrefixWindowMsg, StringComparison.OrdinalIgnoreCase))
        //{
        //  string[] commands = Common.SplitWindowMessageCommand(selected.Substring(Common.CmdPrefixWindowMsg.Length));

        //  MessageCommand messageCommand = new MessageCommand(commands);
        //  if (messageCommand.ShowDialog(this) == DialogResult.OK)
        //    newCommand = Common.CmdPrefixWindowMsg + messageCommand.CommandString;
        //}
        //else if (selected.StartsWith(Common.CmdPrefixTcpMsg, StringComparison.OrdinalIgnoreCase))
        //{
        //  string[] commands = Common.SplitTcpMessageCommand(selected.Substring(Common.CmdPrefixTcpMsg.Length));

        //  TcpMessageCommand tcpMessageCommand = new TcpMessageCommand(commands);
        //  if (tcpMessageCommand.ShowDialog(this) == DialogResult.OK)
        //    newCommand = Common.CmdPrefixTcpMsg + tcpMessageCommand.CommandString;
        //}
        //else if (selected.StartsWith(Common.CmdPrefixHttpMsg, StringComparison.OrdinalIgnoreCase))
        //{
        //  string[] commands = Common.SplitHttpMessageCommand(selected.Substring(Common.CmdPrefixHttpMsg.Length));

        //  HttpMessageCommand httpMessageCommand = new HttpMessageCommand(commands);
        //  if (httpMessageCommand.ShowDialog(this) == DialogResult.OK)
        //    newCommand = Common.CmdPrefixHttpMsg + httpMessageCommand.CommandString;
        //}
        //else if (selected.StartsWith(Common.CmdPrefixKeys, StringComparison.OrdinalIgnoreCase))
        //{
        //  KeysCommand keysCommand = new KeysCommand(selected.Substring(Common.CmdPrefixKeys.Length));

        //  if (keysCommand.ShowDialog(this) == DialogResult.OK)
        //    newCommand = Common.CmdPrefixKeys + keysCommand.CommandString;
        //}
        //else if (selected.StartsWith(Common.CmdPrefixPopup, StringComparison.OrdinalIgnoreCase))
        //{
        //  string[] commands = Common.SplitPopupCommand(selected.Substring(Common.CmdPrefixPopup.Length));

        //  PopupMessage popupMessage = new PopupMessage(commands);
        //  if (popupMessage.ShowDialog(this) == DialogResult.OK)
        //    newCommand = Common.CmdPrefixPopup + popupMessage.CommandString;
        //}

      //  if (!String.IsNullOrEmpty(newCommand))
      //    listViewExternalCommands.SelectedItems[0].SubItems[1].Text = newCommand;
      //}
      //catch (Exception ex)
      //{
      //  IrssLog.Error(ex);
      //  MessageBox.Show(this, ex.Message, "Failed to edit command", MessageBoxButtons.OK, MessageBoxIcon.Error);
      //}
    }

    #endregion Private Methods
  }
}