namespace IRServerPluginInterface
{

  // TODO: Break out Transmit and Receive interfaces into new Interface classes.
  // TODO: Implement Async learning with callbacks (LearnStart, LearnStatus, LearnDone) or...
  // TODO: Implement Learning dialog in each IR Server plugin

  #region Delegates

  /// <summary>
  /// IR Server callback for remote button press handling.
  /// </summary>
  /// <param name="keyCode">Remote button code.</param>
  public delegate void RemoteHandler(string keyCode);

  /// <summary>
  /// IR Server callback for keyboard key press handling.
  /// </summary>
  /// <param name="vKey">Virtual key code.</param>
  /// <param name="keyUp">.</param>
  public delegate void KeyboardHandler(int vKey, bool keyUp);

  /// <summary>
  /// IR Server callback for mouse event handling.
  /// </summary>
  /// <param name="deltaX">Mouse movement on the X-axis.</param>
  /// <param name="deltaY">Mouse movement on the Y-axis.</param>
  /// <param name="rightButton">Is the right button pressed?</param>
  /// <param name="leftButton">Is the left button pressed?</param>
  public delegate void MouseHandler(int deltaX, int deltaY, int buttons);
  
  #endregion Delegates

  #region Enumerations

  /// <summary>
  /// Provides information about the status of learning an infrared command.
  /// </summary>
  public enum LearnStatus
  {
    /// <summary>
    /// Failed to learn infrared command.
    /// </summary>
    Failure,
    /// <summary>
    /// Succeeded in learning infrared command.
    /// </summary>
    Success,
    /// <summary>
    /// Infrared command learning timed out.
    /// </summary>
    Timeout
  }

  #endregion Enumerations

  /// <summary>
  /// Interface for IR Server plugins to interact with the IR Server.
  /// </summary>
  public interface IIRServerPlugin
  {

    #region Properties

    /// <summary>
    /// Name of the IR Server plugin.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// IR Server plugin version.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// The IR Server plugin's author.
    /// </summary>
    string Author { get; }

    /// <summary>
    /// A description of the IR Server plugin.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Can this IR Server plugin receive remote button presses.
    /// </summary>
    bool CanReceive { get; }

    /// <summary>
    /// Can this IR Server plugin transmit infrared commands.
    /// </summary>
    bool CanTransmit { get; }
    
    /// <summary>
    /// Can this IR Server plugin learn infrared commands.
    /// </summary>
    bool CanLearn { get; }

    /// <summary>
    /// Does this IR Server plugin have configuration options.
    /// </summary>
    bool CanConfigure { get; }

    /// <summary>
    /// Callback for remote button presses.
    /// </summary>
    RemoteHandler RemoteCallback { get; set; }

    /// <summary>
    /// Callback for keyboard presses.
    /// </summary>
    KeyboardHandler KeyboardCallback { get; set; }

    /// <summary>
    /// Callback for mouse events.
    /// </summary>
    MouseHandler MouseCallback { get; set; }

    /// <summary>
    /// Lists the available blaster ports.
    /// </summary>
    string[] AvailablePorts { get; }

    #endregion

    #region Interface

    /// <summary>
    /// Configure the IR Server plugin.
    /// </summary>
    void Configure();

    /// <summary>
    /// Start the IR Server plugin.
    /// </summary>
    /// <returns>Success.</returns>
    bool Start();

    /// <summary>
    /// Suspend the IR Server plugin when computer enters standby.
    /// </summary>
    void Suspend();

    /// <summary>
    /// Resume the IR Server plugin when the computer returns from standby.
    /// </summary>
    void Resume();

    /// <summary>
    /// Stop the IR Server plugin.
    /// </summary>
    void Stop();

    /// <summary>
    /// Transmit an infrared command.
    /// </summary>
    /// <param name="file">Infrared command file.</param>
    /// <returns>Success.</returns>
    bool Transmit(string file);

    /// <summary>
    /// Learn an infrared command.
    /// </summary>
    /// <param name="data">New infrared command.</param>
    /// <returns>Tells the calling code if the learn was Successful, Failed or Timed Out.</returns>
    LearnStatus Learn(out byte[] data);

    /// <summary>
    /// Set the transmit port to use for infrared command output.
    /// </summary>
    /// <param name="port">Port to use.</param>
    /// <returns>Success.</returns>
    bool SetPort(string port);

    #endregion Interface

  }

  /*

  public interface IConfigurable
  {

  }

  public interface ITransmitIR
  {

  }

  public interface ILearnIR
  {

  }

  public interface IRemoteReceiver
  {

  }

  public interface IKeyboardReceiver
  {

  }

  public interface IMouseReceiver
  {

  }

  */
}
