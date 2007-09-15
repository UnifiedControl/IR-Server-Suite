using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

using NamedPipes;
using IrssUtils;

namespace VirtualRemote
{

  static class Program
  {

    #region Constants

    const string DefaultSkin = "MCE";

    static readonly string ConfigurationFile = Common.FolderAppData + "Virtual Remote\\Virtual Remote.xml";

    #endregion Constants

    #region Variables

    static MessageQueue _messageQueue;

    static bool _registered = false;
    static bool _keepAlive = true;
    static int _echoID = -1;
    static Thread _keepAliveThread;

    static string _serverHost;
    static string _localPipeName;
    static string _lastKeyCode = String.Empty;

    static string _installFolder;

    static string _remoteSkin;

    #endregion Variables

    #region Properties

    internal static bool Registered
    {
      get { return _registered; }
    }

    internal static string ServerHost
    {
      get { return _serverHost; }
      set { _serverHost = value; }
    }

    internal static string LocalPipeName
    {
      get { return _localPipeName; }
    }

    internal static string InstallFolder
    {
      get { return _installFolder; }
    }

    internal static string RemoteSkin
    {
      get { return _remoteSkin; }
      set { _remoteSkin = value; }
    }

    #endregion Properties

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);

      // TODO: Change log level to info for release.
      IrssLog.LogLevel = IrssLog.Level.Debug;
      IrssLog.Open(Common.FolderIrssLogs + "Virtual Remote.log");

      Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);

      LoadSettings();

      _messageQueue = new MessageQueue(new MessageQueueSink(ReceivedMessage));

      if (args.Length > 0) // Command Line Start ...
      {
        List<String> virtualButtons = new List<string>();
        
        try
        {
          for (int index = 0; index < args.Length; index++)
          {
            if (args[index].Equals("-host", StringComparison.InvariantCultureIgnoreCase))
            {
              _serverHost = args[++index];
              continue;
            }
            else
            {
              virtualButtons.Add(args[index]);
            }
          }
        }
        catch (Exception ex)
        {
          IrssLog.Error("Error processing command line parameters: {0}", ex.ToString());
        }

        if (virtualButtons.Count != 0 && StartComms())
        {
          Thread.Sleep(250);

          // Wait for registered ... Give up after 10 seconds ...
          int attempt = 0;
          while (!_registered)
          {
            if (++attempt >= 10)
              break;
            else
              Thread.Sleep(1000);
          }

          if (_registered)
          {
            foreach (String button in virtualButtons)
            {
              if (button.StartsWith("~"))
              {
                Thread.Sleep(button.Length * 500);
              }
              else
              {
                PipeMessage message = new PipeMessage(Environment.MachineName, Program.LocalPipeName, PipeMessageType.ForwardRemoteEvent, PipeMessageFlags.Notify, button);
                PipeAccess.SendMessage(Common.ServerPipeName, Program.ServerHost, message);
              }
            }

            Thread.Sleep(500);
          }
          else
          {
            IrssLog.Warn("Failed to register with server host \"{0}\", custom message(s) not sent", Program.ServerHost);
          }
        }

      }
      else // GUI Start ...
      {
        if (String.IsNullOrEmpty(_serverHost))
        {
          IrssUtils.Forms.ServerAddress serverAddress = new IrssUtils.Forms.ServerAddress(Environment.MachineName);
          serverAddress.ShowDialog();

          _serverHost = serverAddress.ServerHost;
        }

        if (StartComms())
          Application.Run(new MainForm());

        SaveSettings();
      }

      StopComms();

      Application.ThreadException -= new ThreadExceptionEventHandler(Application_ThreadException);

      IrssLog.Close();
    }

    /// <summary>
    /// Handles unhandled exceptions.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="e">Event args.</param>
    public static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
    {
      IrssLog.Error(e.Exception.ToString());
    }

    static void LoadSettings()
    {
      try
      {
        _installFolder = SystemRegistry.GetInstallFolder();
        if (String.IsNullOrEmpty(_installFolder))
          _installFolder = ".";
        else
          _installFolder += "\\Virtual Remote";
      }
      catch (Exception ex)
      {
        IrssLog.Error(ex.ToString());

        _installFolder = ".";
      }

      try
      {
        XmlDocument doc = new XmlDocument();
        doc.Load(ConfigurationFile);

        _serverHost = doc.DocumentElement.Attributes["ServerHost"].Value;
        _remoteSkin = doc.DocumentElement.Attributes["RemoteSkin"].Value;
      }
      catch (Exception ex)
      {
        IrssLog.Error(ex.ToString());

        _serverHost = String.Empty;
        _remoteSkin = DefaultSkin;
      }
    }
    static void SaveSettings()
    {
      try
      {
        XmlTextWriter writer = new XmlTextWriter(ConfigurationFile, System.Text.Encoding.UTF8);
        writer.Formatting = Formatting.Indented;
        writer.Indentation = 1;
        writer.IndentChar = (char)9;
        writer.WriteStartDocument(true);
        writer.WriteStartElement("settings"); // <settings>

        writer.WriteAttributeString("ServerHost", _serverHost);
        writer.WriteAttributeString("RemoteSkin", _remoteSkin);

        writer.WriteEndElement(); // </settings>
        writer.WriteEndDocument();
        writer.Close();
      }
      catch (Exception ex)
      {
        IrssLog.Error(ex.ToString());
      }
    }

    internal static bool StartComms()
    {
      try
      {
        if (OpenLocalPipe())
        {
          _messageQueue.Start();

          _keepAliveThread = new Thread(new ThreadStart(KeepAliveThread));
          _keepAliveThread.Start();
          return true;
        }
      }
      catch (Exception ex)
      {
        IrssLog.Error(ex.ToString());
      }

      return false;
    }
    internal static void StopComms()
    {
      _keepAlive = false;

      try
      {
        if (_keepAliveThread != null && _keepAliveThread.IsAlive)
          _keepAliveThread.Abort();
      }
      catch { }

      try
      {
        if (_registered)
        {
          _registered = false;

          PipeMessage message = new PipeMessage(Environment.MachineName, _localPipeName, PipeMessageType.UnregisterClient, PipeMessageFlags.Request);
          PipeAccess.SendMessage(Common.ServerPipeName, _serverHost, message);
        }
      }
      catch { }

      _messageQueue.Stop();

      try
      {
        if (PipeAccess.ServerRunning)
          PipeAccess.StopServer();
      }
      catch { }
    }

    static bool OpenLocalPipe()
    {
      try
      {
        int pipeNumber = 1;
        bool retry = false;

        do
        {
          string localPipeTest = String.Format("irserver\\remote{0:00}", pipeNumber);

          if (PipeAccess.PipeExists(Common.LocalPipePrefix + localPipeTest))
          {
            if (++pipeNumber <= Common.MaximumLocalClientCount)
              retry = true;
            else
              throw new Exception(String.Format("Maximum local client limit ({0}) reached", Common.MaximumLocalClientCount));
          }
          else
          {
            if (!PipeAccess.StartServer(localPipeTest, new PipeMessageHandler(_messageQueue.Enqueue)))
              throw new Exception(String.Format("Failed to start local pipe server \"{0}\"", localPipeTest));

            _localPipeName = localPipeTest;
            retry = false;
          }
        }
        while (retry);

        return true;
      }
      catch (Exception ex)
      {
        IrssLog.Error(ex.ToString());
        return false;
      }
    }

    static bool ConnectToServer()
    {
      try
      {
        PipeMessage message = new PipeMessage(Environment.MachineName, _localPipeName, PipeMessageType.RegisterClient, PipeMessageFlags.Request);
        PipeAccess.SendMessage(Common.ServerPipeName, _serverHost, message);
        return true;
      }
      catch (AppModule.NamedPipes.NamedPipeIOException)
      {
        return false;
      }
      catch (Exception ex)
      {
        IrssLog.Error(ex.ToString());
        return false;
      }
    }

    static void KeepAliveThread()
    {
      Random random = new Random((int)DateTime.Now.Ticks);
      bool reconnect;
      int attempt;

      _registered = false;
      _keepAlive = true;
      while (_keepAlive)
      {
        reconnect = true;

        #region Connect to server

        IrssLog.Info("Connecting ({0}) ...", _serverHost);
        attempt = 0;
        while (_keepAlive && reconnect)
        {
          if (ConnectToServer())
          {
            reconnect = false;
          }
          else
          {
            int wait;

            if (attempt <= 50)
              attempt++;

            if (attempt > 50)
              wait = 30;      // 30 seconds
            else if (attempt > 20)
              wait = 10;      // 10 seconds
            else if (attempt > 10)
              wait = 5;       // 5 seconds
            else
              wait = 1;       // 1 second

            for (int sleeps = 0; sleeps < wait && _keepAlive; sleeps++)
              Thread.Sleep(1000);
          }
        }

        #endregion Connect to server

        #region Wait for registered

        // Give up after 10 seconds ...
        attempt = 0;
        while (_keepAlive && !_registered && !reconnect)
        {
          if (++attempt >= 10)
            reconnect = true;
          else
            Thread.Sleep(1000);
        }

        #endregion Wait for registered

        #region Ping the server repeatedly

        while (_keepAlive && _registered && !reconnect)
        {
          int pingID = random.Next();
          long pingTime = DateTime.Now.Ticks;

          try
          {
            PipeMessage message = new PipeMessage(Environment.MachineName, _localPipeName, PipeMessageType.Ping, PipeMessageFlags.Request, BitConverter.GetBytes(pingID));
            PipeAccess.SendMessage(Common.ServerPipeName, _serverHost, message);
          }
          catch
          {
            // Failed to ping ... reconnect ...
            IrssLog.Warn("Failed to ping, attempting to reconnect ...");
            _registered = false;
            reconnect = true;
            break;
          }

          // Wait 10 seconds for a ping echo ...
          bool receivedEcho = false;
          while (_keepAlive && _registered && !reconnect &&
            !receivedEcho && DateTime.Now.Ticks - pingTime < 10 * 1000 * 10000)
          {
            if (_echoID == pingID)
            {
              receivedEcho = true;
            }
            else
            {
              Thread.Sleep(1000);
            }
          }

          if (receivedEcho) // Received ping echo ...
          {
            // Wait 60 seconds before re-pinging ...
            for (int sleeps = 0; sleeps < 60 && _keepAlive && _registered; sleeps++)
              Thread.Sleep(1000);
          }
          else // Didn't receive ping echo ...
          {
            IrssLog.Warn("No echo to ping, attempting to reconnect ...");

            // Break out of pinging cycle ...
            _registered = false;
            reconnect = true;
          }
        }

        #endregion Ping the server repeatedly

      }

    }

    static void ReceivedMessage(string message)
    {
      PipeMessage received = PipeMessage.FromString(message);

      IrssLog.Debug("Received Message \"{0}\"", received.Type);

      try
      {
        switch (received.Type)
        {
          case PipeMessageType.RegisterClient:
            if ((received.Flags & PipeMessageFlags.Success) == PipeMessageFlags.Success)
            {
              //_irServerInfo = IRServerInfo.FromBytes(received.DataAsBytes);
              _registered = true;

              IrssLog.Info("Registered to IR Server");
            }
            else if ((received.Flags & PipeMessageFlags.Failure) == PipeMessageFlags.Failure)
            {
              _registered = false;
              IrssLog.Warn("IR Server refused to register");
            }
            break;

          case PipeMessageType.ServerShutdown:
            IrssLog.Warn("IR Server Shutdown - Virtual Remote disabled until IR Server returns");
            _registered = false;
            break;

          case PipeMessageType.Echo:
            _echoID = BitConverter.ToInt32(received.DataAsBytes, 0);
            return;

          case PipeMessageType.Error:
            IrssLog.Error(received.DataAsString);
            break;
        }
      }
      catch (Exception ex)
      {
        IrssLog.Error(ex.ToString());
      }
    }

  }

}
