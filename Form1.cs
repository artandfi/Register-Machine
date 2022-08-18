using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Register_Machine {
    public partial class Form1 : Form {
        private const string CMD_RGX = @"^\s*(Z\(\d\)|S\(\d\)|T\(\d,\s*\d\)|J\(\d,\s*\d,\s*([1-9]\d*|0)\))$";
        private List<TextBox> _registers;
        private List<Command> _commands = new();
        private bool _canceled = false;
        private int _delay;

        public Form1() {
            InitializeComponent();

            _registers = new() { Reg0, Reg1, Reg2, Reg3, Reg4, Reg5, Reg6, Reg7, Reg8, Reg9 };
            foreach (var reg in _registers) {
                reg.KeyPress += new KeyPressEventHandler(Reg_KeyPress);
                reg.Leave += new EventHandler(Reg_Leave);
            }
        }

        private bool ParseCommands() {
            string[] commandLines = CommandsTextBox.Lines;
            
            if (commandLines.Length == 0) {
                MessageBox.Show("No commands entered", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            for (int i = 0; i < commandLines.Length; i++) {
                string line = commandLines[i];

                if (Regex.Match(line, CMD_RGX).Success) {
                    line = line.Replace(" ", "");

                    switch (line[0]) {
                        case 'Z':
                            _commands.Add(new Z(line[2] - '0', _registers));
                            break;
                        case 'S':
                            _commands.Add(new S(line[2] - '0', _registers));
                            break;
                        case 'T':
                            _commands.Add(new T(line[2] - '0', line[4] - '0', _registers));
                            break;
                        case 'J':
                            string[] split = line.Split(',', ')');
                            _commands.Add(new J(line[2] - '0', line[4] - '0', int.Parse(split[2]), _registers));
                            break;
                    }
                }
                else {
                    MessageBox.Show($"Command \"{line}\" is incorrect", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            return true;
        }

        private async void Run() {
            if (ParseCommands()) {
                DelaySlider.Enabled = false;
                RunButton.Enabled = false;
                StopButton.Enabled = true;
                ResetButton.Enabled = false;
                HighlightAnswer.Enabled = false;
                HighlightCommands.Enabled = false;
                _canceled = false;
                _delay = DelaySlider.Value;

                foreach (var reg in _registers) {
                    reg.ReadOnly = true;
                }

                await Task.Run(() => {
                    int nextCommandId = 0;

                    while (!_canceled && nextCommandId < _commands.Count) {
                        int currentCommandId = nextCommandId;
                        bool highlight = HighlightCommands.Checked;

                        if (highlight) {
                            HighlightCommand(currentCommandId, Color.Red);
                        }

                        _commands[nextCommandId].Run(ref nextCommandId);
                        Thread.Sleep(_delay);
                        
                        if (highlight) {
                            HighlightCommand(currentCommandId, Color.White);
                        }
                    }
                });

                DelaySlider.Enabled = true;
                RunButton.Enabled = true;
                StopButton.Enabled = false;
                ResetButton.Enabled = true;
                HighlightAnswer.Enabled = true;
                HighlightCommands.Enabled = true;
                _commands = new();

                foreach (var reg in _registers) {
                    reg.ReadOnly = false;
                }

                if (!_canceled && HighlightAnswer.Checked) {
                    await Task.Run(() => {
                        for (int i = 0; i < 2; i++) {
                            Reg0.BackColor = Color.LightGreen;
                            Thread.Sleep(250);
                            Reg0.BackColor = Color.White;
                            Thread.Sleep(250);
                        }
                    });
                }
            }
        }

        private void Stop() {
            _canceled = true;
            RunButton.Enabled = true;
        }

        private void Reset() {
            foreach (var reg in _registers) {
                reg.Text = "0";
            }
        }

        private void HighlightCommand(int commandId, Color color) {
            int startIndex = LineNumberTextBox.GetFirstCharIndexFromLine(commandId);
            LineNumberTextBox.Select(startIndex, LineNumberTextBox.Lines[commandId].Length);

            LineNumberTextBox.SelectionBackColor = color;
        }

        private void RunButton_Click(object sender, EventArgs e) {
            ActiveControl = null;
            Run();
        }

        private void StopButton_Click(object sender, EventArgs e) {
            Stop();
        }

        private void ResetButton_Click(object sender, EventArgs e) {
            Reset();
        }

        private void Reg_KeyPress(object sender, KeyPressEventArgs e) {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) {
                e.Handled = true;
            }
        }

        private void Reg_Leave(object sender, EventArgs e) {
            var reg = (TextBox)sender;
            if (reg.Text == "") {
                reg.Text = "0";
            }
        }

        #region Command Lines Numbers (https://stackoverflow.com/a/58934389/9953559)
        public int GetWidth() {
            int w = 50;
            // get total lines of CommandsTextBox
            int line = CommandsTextBox.Lines.Length;

            if (line <= 99) {
                w = 20 + (int)CommandsTextBox.Font.Size;
            }
            else if (line <= 999) {
                w = 30 + (int)CommandsTextBox.Font.Size;
            }
            else {
                w = 50 + (int)CommandsTextBox.Font.Size;
            }

            return w * 2;
        }

        public void AddLineNumbers() {
            // create & set Point pt to (0,0)
            Point pt = new Point(0, 0);
            // get First Index & First Line from CommandsTextBox
            int First_Index = CommandsTextBox.GetCharIndexFromPosition(pt);
            int First_Line = CommandsTextBox.GetLineFromCharIndex(First_Index);
            // set X & Y coordinates of Point pt to ClientRectangle Width & Height respectively
            pt.X = ClientRectangle.Width;
            pt.Y = ClientRectangle.Height;
            // get Last Index & Last Line from CommandsTextBox
            int Last_Index = CommandsTextBox.GetCharIndexFromPosition(pt);
            int Last_Line = CommandsTextBox.GetLineFromCharIndex(Last_Index);
            // set Center alignment to LineNumberTextBox
            LineNumberTextBox.SelectionAlignment = HorizontalAlignment.Center;
            // set LineNumberTextBox text to null & width to getWidth() function value
            LineNumberTextBox.Text = "";
            LineNumberTextBox.Width = GetWidth();
            // now add each line number to LineNumberTextBox upto last line
            for (int i = First_Line; i <= Last_Line + 2; i++) {
                LineNumberTextBox.Text += i + 1 + "\n";
            }
        }

        private void Form1_Load(object sender, EventArgs e) {
            LineNumberTextBox.Font = CommandsTextBox.Font;
            CommandsTextBox.Select();
            AddLineNumbers();
        }

        private void CommandsTextBox_SelectionChanged(object sender, EventArgs e) {
            Point pt = CommandsTextBox.GetPositionFromCharIndex(CommandsTextBox.SelectionStart);
            if (pt.X == 1) {
                AddLineNumbers();
            }
        }

        private void CommandsTextBox_VScroll(object sender, EventArgs e) {
            LineNumberTextBox.Text = "";
            AddLineNumbers();
            LineNumberTextBox.Invalidate();
        }

        private void CommandsTextBox_TextChanged(object sender, EventArgs e) {
            AddLineNumbers();
        }

        private void CommandsTextBox_FontChanged(object sender, EventArgs e) {
            CommandsTextBox.Select();
            AddLineNumbers();
        }

        private void CommandsTextBox_MouseDown(object sender, MouseEventArgs e) {
            CommandsTextBox.Select();
            LineNumberTextBox.DeselectAll();
        }

        private void Form1_Resize(object sender, EventArgs e) {
            AddLineNumbers();
        }
        #endregion
    }
}
