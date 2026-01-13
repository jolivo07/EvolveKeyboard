using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WindowsInput;
using WindowsInput.Native;

namespace SharedLibrary.Services
{
    public class KeyExecutionService
    {
        private readonly IInputSimulator _inputSimulator;
        
        // Event to notify when a command should be handled by the application (like navigation)
        public event EventHandler<string>? CommandRequested;

        public KeyExecutionService()
        {
            _inputSimulator = new InputSimulator();
        }

        public void ExecuteAction(KeyboardButtonModel button)
        {
            if (button == null) return;

            switch (button.Action)
            {
                case "RunCommand":
                    RunCommand(button.Value);
                    break;
                case "SendValue":
                    if (!string.IsNullOrEmpty(button.Value))
                    {
                        // Check if the value ends with a command (e.g., "12345,ENTER")
                        int lastCommaIndex = button.Value.LastIndexOf(',');
                        bool handled = false;

                        if (lastCommaIndex >= 0 && lastCommaIndex < button.Value.Length - 1)
                        {
                            string textPart = button.Value.Substring(0, lastCommaIndex);
                            string commandPart = button.Value.Substring(lastCommaIndex + 1).Trim();

                            // Verify if the part after the comma is a valid key or combination
                            if (IsValidKeyCombination(commandPart))
                            {
                                // Type the text part first
                                if (!string.IsNullOrEmpty(textPart))
                                {
                                    _inputSimulator.Keyboard.TextEntry(textPart);
                                }
                                
                                // Small delay between text and command
                                System.Threading.Thread.Sleep(50);

                                // Execute the command part
                                SendKey(commandPart);
                                handled = true;
                            }
                        }

                        if (!handled)
                        {
                            _inputSimulator.Keyboard.TextEntry(button.Value);
                        }
                    }
                    break;
                case "Navigate":
                    if (!string.IsNullOrEmpty(button.Value))
                    {
                        CommandRequested?.Invoke(this, $"Navigate:{button.Value}");
                    }
                    break;
                case "CommandKey":
                    SendKey(button.Value);
                    break;
                // Keep SendKey for backward compatibility if needed, mapping to CommandKey logic
                case "SendKey":
                    SendKey(button.Value);
                    break;
            }
        }

        private bool IsValidKeyCombination(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;

            var parts = value.Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries)
                             .Select(p => p.Trim());

            foreach (var part in parts)
            {
                if (!TryGetKeyCode(part, out _))
                {
                    return false;
                }
            }
            return true;
        }

        private void SendKey(string value)
        {
            if (string.IsNullOrEmpty(value)) return;

            var parts = value.Split(new[] { '+' }, StringSplitOptions.RemoveEmptyEntries)
                             .Select(p => p.Trim())
                             .ToArray();

            if (parts.Length > 1)
            {
                var modifiers = new List<VirtualKeyCode>();
                var keys = new List<VirtualKeyCode>();

                foreach (var part in parts)
                {
                    if (TryGetKeyCode(part, out var keyCode))
                    {
                        if (IsModifier(keyCode))
                        {
                            modifiers.Add(keyCode);
                        }
                        else
                        {
                            keys.Add(keyCode);
                        }
                    }
                }

                if (keys.Count > 0)
                {
                    // Manually execute the sequence to ensure modifiers are registered
                    // 1. Hold down all modifiers
                    foreach (var modifier in modifiers)
                    {
                        _inputSimulator.Keyboard.KeyDown(modifier);
                    }
                    
                    // 2. Delay to ensure OS/Target App registers the modifier state
                    System.Threading.Thread.Sleep(100);

                    // 3. Press the keys (explicit KeyDown -> Delay -> KeyUp)
                    foreach (var key in keys)
                    {
                        _inputSimulator.Keyboard.KeyDown(key);
                        System.Threading.Thread.Sleep(50); // Hold key for 50ms
                        _inputSimulator.Keyboard.KeyUp(key);
                    }

                    // 4. Delay before releasing modifiers
                    System.Threading.Thread.Sleep(100);

                    // 5. Release all modifiers
                    foreach (var modifier in modifiers)
                    {
                        _inputSimulator.Keyboard.KeyUp(modifier);
                    }
                }
                else if (modifiers.Count > 0)
                {
                    // If only modifiers are present (e.g. ALT+SHIFT), press them sequentially or together?
                    // Usually we treat them as key presses.
                    foreach (var mod in modifiers)
                    {
                        _inputSimulator.Keyboard.KeyPress(mod);
                    }
                }
            }
            else
            {
                if (TryGetKeyCode(value, out var keyCode))
                {
                    _inputSimulator.Keyboard.KeyPress(keyCode);
                }
            }
        }

        private bool TryGetKeyCode(string value, out VirtualKeyCode keyCode)
        {
            // 1. Try Alias (e.g. "ALT" -> "MENU", "WIN" -> "LWIN") - Check this first for common aliases
            if (ParseModifierAlias(value, out keyCode)) return true;

            // 2. Try direct enum parse (e.g. "RETURN", "SPACE", "LWIN")
            if (Enum.TryParse<VirtualKeyCode>(value, true, out keyCode)) return true;

            // 3. Try adding VK_ prefix (e.g. "V" -> "VK_V", "F4" -> "VK_F4", "1" -> "VK_1")
            if (Enum.TryParse<VirtualKeyCode>("VK_" + value, true, out keyCode)) return true;
            
            keyCode = VirtualKeyCode.NONAME;
            return false;
        }

        private bool IsModifier(VirtualKeyCode code)
        {
            return code == VirtualKeyCode.CONTROL || code == VirtualKeyCode.LCONTROL || code == VirtualKeyCode.RCONTROL ||
                   code == VirtualKeyCode.MENU || code == VirtualKeyCode.LMENU || code == VirtualKeyCode.RMENU || // MENU is ALT
                   code == VirtualKeyCode.SHIFT || code == VirtualKeyCode.LSHIFT || code == VirtualKeyCode.RSHIFT ||
                   code == VirtualKeyCode.LWIN || code == VirtualKeyCode.RWIN;
        }

        private bool ParseModifierAlias(string alias, out VirtualKeyCode code)
        {
            alias = alias.ToUpper();
            // Use Left variants (L*) as they are more reliable for simulation than generic ones
            // Reverting to LMENU for ALT as manual sequence handles timing better
            if (alias == "ALT") { code = VirtualKeyCode.LMENU; return true; }
            if (alias == "CTRL") { code = VirtualKeyCode.LCONTROL; return true; }
            if (alias == "SHIFT") { code = VirtualKeyCode.LSHIFT; return true; }
            if (alias == "WIN") { code = VirtualKeyCode.LWIN; return true; }
            
            // Common Key Aliases
            if (alias == "ENTER") { code = VirtualKeyCode.RETURN; return true; }
            if (alias == "ESC") { code = VirtualKeyCode.ESCAPE; return true; }
            if (alias == "BACKSPACE" || alias == "BS") { code = VirtualKeyCode.BACK; return true; }
            if (alias == "TAB") { code = VirtualKeyCode.TAB; return true; }
            if (alias == "DEL") { code = VirtualKeyCode.DELETE; return true; }
            if (alias == "INS") { code = VirtualKeyCode.INSERT; return true; }
            if (alias == "PGUP") { code = VirtualKeyCode.PRIOR; return true; }
            if (alias == "PGDN") { code = VirtualKeyCode.NEXT; return true; }
            if (alias == "HOME") { code = VirtualKeyCode.HOME; return true; }
            if (alias == "END") { code = VirtualKeyCode.END; return true; }

            code = VirtualKeyCode.NONAME;
            return false;
        }

        private void RunCommand(string command)
        {
            if (string.IsNullOrEmpty(command)) return;
            
            // Raise event for the application to handle specific internal commands (like page navigation or EXIT)
            CommandRequested?.Invoke(this, command);

            // internal commands that shouldn't be executed as processes
            if (command.Equals("EXIT", StringComparison.OrdinalIgnoreCase)) return;

            // Also try to run as process
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
