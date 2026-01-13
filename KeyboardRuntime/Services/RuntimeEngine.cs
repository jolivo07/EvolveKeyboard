using WindowsInput;
using WindowsInput.Native;
using SharedLibrary.Models;
using SharedLibrary.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace KeyboardRuntime.Services
{
    public class RuntimeEngine
    {
        private readonly IInputSimulator _inputSimulator;
        
        // Event to notify when a command should be handled by the application (like navigation)
        public event EventHandler<string>? CommandRequested;

        public RuntimeEngine()
        {
            _inputSimulator = new InputSimulator();
        }

        public async Task<KeyboardLayoutModel?> LoadLayoutAsync(string path)
        {
            return await LayoutSerializer.LoadAsync(path);
        }

        public void ExecuteAction(KeyboardButtonModel button)
        {
            switch (button.Action)
            {
                case "SendKey":
                    SendKey(button.Value);
                    break;
                case "RunCommand":
                    RunCommand(button.Value);
                    break;
            }
        }

        private void SendKey(string value)
        {
            if (string.IsNullOrEmpty(value)) return;

            if (value.StartsWith("Text:"))
            {
                _inputSimulator.Keyboard.TextEntry(value.Substring(5));
            }
            else
            {
                if (Enum.TryParse<VirtualKeyCode>(value, true, out var keyCode))
                {
                    _inputSimulator.Keyboard.KeyPress(keyCode);
                }
            }
        }

        private void RunCommand(string command)
        {
            if (string.IsNullOrEmpty(command)) return;
            
            // Raise event for the application to handle specific commands
            CommandRequested?.Invoke(this, command);

            // Also try to run as process if it looks like a file/app (simple heuristic)
            if (command.Contains(".") || command.Contains("\\") || command.Contains("/"))
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = command,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error running command: {ex.Message}");
                }
            }
        }
    }
}
