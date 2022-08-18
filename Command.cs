using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Register_Machine {
    public interface Command {
        public void Run(ref int commandId);
    }

    public class Z : Command {
        private int _n;
        private List<TextBox> _registers;

        public Z(int n, List<TextBox> registers) {
            _n = n;
            _registers = registers;
        }

        public void Run(ref int commandId) {
            _registers[_n].Text = "0";
            commandId++;
        }
    }

    public class S : Command {
        private int _n;
        private List<TextBox> _registers;

        public S(int n, List<TextBox> registers) {
            _n = n;
            _registers = registers;
        }

        public void Run(ref int commandId) {
            int value = int.Parse(_registers[_n].Text) + 1;
            _registers[_n].Text = value.ToString();
            commandId++;
        }
    }

    public class T : Command {
        private int _m;
        private int _n;
        private List<TextBox> _registers;

        public T(int m, int n, List<TextBox> registers) {
            _m = m;
            _n = n;
            _registers = registers;
        }

        public void Run(ref int commandId) {
            _registers[_n].Text = _registers[_m].Text;
            commandId++;
        }
    }

    public class J : Command {
        private int _m;
        private int _n;
        private int _q;
        private List<TextBox> _registers;

        public J(int m, int n, int q, List<TextBox> registers) {
            _m = m;
            _n = n;
            _q = q;
            _registers = registers;
        }

        public void Run(ref int commandId) {
            if (_registers[_m].Text == _registers[_n].Text) {
                commandId = _q - 1;
            }
            else {
                commandId++;
            }
        }
    }
}
