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
using System.Text;
using System.Xml;

namespace MPUtils
{
  /// <summary>
  /// External Channel Changing configuration file for tuning Set Top Boxes in MediaPortal.
  /// </summary>
  public class ExternalChannelConfig
  {
    #region Constants

    private const int DefaultCardID = 0;
    private const int DefaultChannelDigits = 0;

    private const bool DefaultDoubleChannelSelect = false;
    private const int DefaultPauseTime = 500;
    private const int DefaultRepeatChannelCommands = 0;
    private const int DefaultRepeatPauseTime = 2000;
    private const bool DefaultSendSelect = false;
    private const bool DefaultUsePreChangeCommand = false;

    #endregion Constants

    #region Variables

    private readonly string _fileName;

    private int _cardID;
    private int _channelDigits;
    private string[] _digits;

    private bool _doubleChannelSelect;
    private int _pauseTime;
    private string _preChangeCommand;
    private int _repeatChannelCommands;
    private int _repeatPauseTime;

    private string _selectCommand;
    private bool _sendSelect;
    private bool _usePreChangeCommand;

    #endregion Variables

    #region Properties

    /// <summary>
    /// Gets the name of the file used to store this configuration.
    /// </summary>
    /// <value>The name of the file.</value>
    public string FileName
    {
      get { return _fileName; }
    }

    /// <summary>
    /// Gets or sets the card id.
    /// </summary>
    /// <value>The card id.</value>
    public int CardId
    {
      get { return _cardID; }
      set { _cardID = value; }
    }

    /// <summary>
    /// Gets or sets the pause time.
    /// </summary>
    /// <value>The pause time.</value>
    public int PauseTime
    {
      get { return _pauseTime; }
      set { _pauseTime = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to send a select command.
    /// </summary>
    /// <value><c>true</c> if send select; otherwise, <c>false</c>.</value>
    public bool SendSelect
    {
      get { return _sendSelect; }
      set { _sendSelect = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to send the select command twice.
    /// </summary>
    /// <value><c>true</c> if sending channel select twice; otherwise, <c>false</c>.</value>
    public bool DoubleChannelSelect
    {
      get { return _doubleChannelSelect; }
      set { _doubleChannelSelect = value; }
    }

    /// <summary>
    /// Gets or sets the flag to repeat channel commands.
    /// </summary>
    /// <value>The flag to repeat channel commands.</value>
    public int RepeatChannelCommands
    {
      get { return _repeatChannelCommands; }
      set { _repeatChannelCommands = value; }
    }

    /// <summary>
    /// Gets or sets the channel digit count.
    /// </summary>
    /// <value>The number of channel digits.</value>
    public int ChannelDigits
    {
      get { return _channelDigits; }
      set { _channelDigits = value; }
    }

    /// <summary>
    /// Gets or sets the pause time between repeats.
    /// </summary>
    /// <value>The repeat pause time.</value>
    public int RepeatPauseTime
    {
      get { return _repeatPauseTime; }
      set { _repeatPauseTime = value; }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to use a pre-change command.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if using a pre-change command; otherwise, <c>false</c>.
    /// </value>
    public bool UsePreChangeCommand
    {
      get { return _usePreChangeCommand; }
      set { _usePreChangeCommand = value; }
    }

    /// <summary>
    /// Gets or sets the digit commands.
    /// </summary>
    /// <value>The digit commands.</value>
    public string[] Digits
    {
      get { return _digits; }
      set { _digits = value; }
    }

    /// <summary>
    /// Gets or sets the select command.
    /// </summary>
    /// <value>The select command.</value>
    public string SelectCommand
    {
      get { return _selectCommand; }
      set { _selectCommand = value; }
    }

    /// <summary>
    /// Gets or sets the pre-change command.
    /// </summary>
    /// <value>The pre-change command.</value>
    public string PreChangeCommand
    {
      get { return _preChangeCommand; }
      set { _preChangeCommand = value; }
    }

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="ExternalChannelConfig"/> class.
    /// </summary>
    /// <param name="fileName">Name of the configuration file.</param>
    public ExternalChannelConfig(string fileName)
    {
      _fileName = fileName;

      _cardID = DefaultCardID;

      _pauseTime = DefaultPauseTime;
      _sendSelect = DefaultSendSelect;
      _doubleChannelSelect = DefaultDoubleChannelSelect;
      _repeatChannelCommands = DefaultRepeatChannelCommands;
      _channelDigits = DefaultChannelDigits;
      _repeatPauseTime = DefaultRepeatPauseTime;
      _usePreChangeCommand = DefaultUsePreChangeCommand;

      _selectCommand = String.Empty;
      _preChangeCommand = String.Empty;
      _digits = new string[10];

      for (int i = 0; i < 10; i++)
        _digits[i] = String.Empty;
    }

    #endregion Constructor

    /// <summary>
    /// Saves this instance to its configuration file.
    /// </summary>
    public void Save()
    {
      using (XmlTextWriter writer = new XmlTextWriter(_fileName, Encoding.UTF8))
      {
        writer.Formatting = Formatting.Indented;
        writer.Indentation = 1;
        writer.IndentChar = (char) 9;
        writer.WriteStartDocument(true);
        writer.WriteStartElement("config"); // <config>

        writer.WriteElementString("PauseTime", PauseTime.ToString());
        writer.WriteElementString("UsePreChangeCommand", UsePreChangeCommand.ToString());
        writer.WriteElementString("SendSelect", SendSelect.ToString());
        writer.WriteElementString("DoubleChannelSelect", DoubleChannelSelect.ToString());
        writer.WriteElementString("ChannelDigits", ChannelDigits.ToString());
        writer.WriteElementString("RepeatChannelCommands", RepeatChannelCommands.ToString());
        writer.WriteElementString("RepeatDelay", RepeatPauseTime.ToString());

        writer.WriteElementString("SelectCommand", SelectCommand);
        writer.WriteElementString("PreChangeCommand", PreChangeCommand);

        for (int i = 0; i < 10; i++)
          writer.WriteElementString("Digit" + i, Digits[i]);

        writer.WriteEndElement(); // </config>
        writer.WriteEndDocument();
      }
    }

    private static string GetString(XmlDocument doc, string element, string defaultValue)
    {
      if (String.IsNullOrEmpty(element))
        return defaultValue;

      XmlNode node = doc.DocumentElement.SelectSingleNode(element);
      if (node == null)
        return defaultValue;

      return node.InnerText;
    }

    private static int GetInt(XmlDocument doc, string element, int defaultValue)
    {
      if (String.IsNullOrEmpty(element))
        return defaultValue;

      XmlNode node = doc.DocumentElement.SelectSingleNode(element);
      if (node == null)
        return defaultValue;

      int returnValue;
      if (int.TryParse(node.InnerText, out returnValue))
        return returnValue;

      return defaultValue;
    }

    private static bool GetBool(XmlDocument doc, string element, bool defaultValue)
    {
      if (String.IsNullOrEmpty(element))
        return defaultValue;

      XmlNode node = doc.DocumentElement.SelectSingleNode(element);
      if (node == null)
        return defaultValue;

      bool returnValue;
      if (bool.TryParse(node.InnerText, out returnValue))
        return returnValue;

      return defaultValue;
    }

    /// <summary>
    /// Loads the specified file into a new instance of <see cref="ExternalChannelConfig"/> class.
    /// </summary>
    /// <param name="fileName">Name of the file to load.</param>
    /// <returns>A new <see cref="ExternalChannelConfig"/> class instance.</returns>
    public static ExternalChannelConfig Load(string fileName)
    {
      ExternalChannelConfig newECC = new ExternalChannelConfig(fileName);

      XmlDocument doc = new XmlDocument();
      doc.Load(fileName);

      newECC.PauseTime = GetInt(doc, "PauseTime", DefaultPauseTime);
      newECC.UsePreChangeCommand = GetBool(doc, "UsePreChangeCommand", DefaultUsePreChangeCommand);
      newECC.SendSelect = GetBool(doc, "SendSelect", DefaultSendSelect);
      newECC.DoubleChannelSelect = GetBool(doc, "DoubleChannelSelect", DefaultDoubleChannelSelect);
      newECC.RepeatChannelCommands = GetInt(doc, "RepeatChannelCommands", DefaultRepeatChannelCommands);
      newECC.ChannelDigits = GetInt(doc, "ChannelDigits", DefaultChannelDigits);
      newECC.RepeatPauseTime = GetInt(doc, "RepeatDelay", DefaultRepeatPauseTime);

      newECC.SelectCommand = GetString(doc, "SelectCommand", String.Empty);
      newECC.PreChangeCommand = GetString(doc, "PreChangeCommand", String.Empty);

      for (int index = 0; index < 10; index++)
        newECC.Digits[index] = GetString(doc, "Digit" + index, String.Empty);

      return newECC;
    }
  }
}