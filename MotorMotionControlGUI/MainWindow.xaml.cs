
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Media;
using System.Collections.Generic;


namespace MotorMotionControlGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 


    public partial class MainWindow : Window
    {

        SolidColorBrush osuBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#D3832B"));

        struct Parameter
        {
            private readonly string name;
            private readonly int min;
            private readonly int max;
            private readonly int defaultVal;
            private readonly string units;
            public int currentVal;
            private TextBox tbInput;
            private Button button;
            private TextBlock tbDescription;

            public Parameter(string name, int min, int max, int defaultValue, string units, TextBox tbInput, Button button, TextBlock tbDescription)
            {
                this.name = name;
                this.min = min;
                this.max = max;
                this.defaultVal = defaultValue;
                this.currentVal = defaultValue;
                this.units = units;
                this.tbInput = tbInput;
                this.button = button;
                this.tbDescription = tbDescription;

            }

            public string Name { get { return name; } }
            public int Min { get { return min; } }
            public int Max { get { return max; } }
            public int DefaultVal { get { return defaultVal; } }
            public int CurrentVal { get { return currentVal; } }
            public string Units { get { return units; } }
            public TextBox TbInput { get { return tbInput; } }
            public Button Button { get { return button; } }
            public TextBlock TbDescription { get { return tbDescription; } }

        };

        Parameter[] paramArr;
        int numParameters = 3;


        private void UpdateDescription(Parameter p)
        {
            p.TbDescription.Text = p.Name + "\n" + "Range: " + p.Min + " - " + p.Max + "\nCurrent Value: " + p.CurrentVal;
        }

        private void InitializeTextBox(Parameter p)
        {
            p.TbInput.Tag = p.Name;
            p.TbInput.Foreground = Brushes.Gray;
            p.TbInput.Text = "Default " + p.DefaultVal;
            p.TbInput.BorderBrush = Brushes.Black;
            p.TbInput.AcceptsReturn = false;
            p.Button.Tag = p.Name;
            UpdateDescription(p);
            p.TbInput.BorderBrush = osuBrush;
        }

        private Parameter GetParameter(string tag)
        {
            for(int i = 0; i < numParameters; i++)
            {
                if (paramArr[i].Name == tag)
                    return paramArr[i];
            }

            return paramArr[0];
        }

        private Parameter GetParameter(Button button)
        {
            for (int i = 0; i < numParameters; i++)
            {
                if (paramArr[i].Button.Tag == button.Tag)
                    return paramArr[i];
            }

            return paramArr[0];
        }

        public MainWindow()
        {

            InitializeComponent();

           paramArr = new Parameter[]
            {
                new Parameter("Angle", 0, 100, 1, "degrees", text1, button1, description1),
                new Parameter("Speed", 0, 100, 2, "m/s", text2, button2, description2),
                new Parameter("Ramp", 0, 100, 3, "??", text3, button3, description3),

            };

            for (int i = 0; i < numParameters; i++)
                InitializeTextBox(paramArr[i]);

        }


        private int CheckLimits(Parameter p, string value)
        {
            int valueInt;

            if (!int.TryParse(value, out valueInt))
                return 0;

            if (valueInt < p.Min || valueInt > p.Max)
                return 0;

            return valueInt;
        }

        void SubmitValue(string tag)
        {
            SubmitValue(GetParameter(tag));
        }


        void SubmitValue(Parameter p)
        {

            string value = p.TbInput.Text;
            string tag = p.TbInput.Tag.ToString();

            int validInt = CheckLimits(p, value);

            if (validInt != 0)
            {
                MessageBox.Show("Updated " + tag + " with new value of " + value);
                p.currentVal = validInt;
                UpdateDescription(p);
            }

            else
            {
                MessageBox.Show("Invalid parameter value.");
            }

            p.TbInput.Text = "Default " + p.DefaultVal;
            p.TbInput.Foreground = Brushes.Gray;
        }

        private void GetTextBox(object sender, RoutedEventArgs e)
        {
            Parameter p = GetParameter(sender as Button);
            SubmitValue(p);
        }


        private void CheckEnter(object sender, System.Windows.Input.KeyEventArgs e)
        {
            TextBox tb = sender as TextBox;

            if ((char)e.Key == 6)
                SubmitValue((string)tb.Tag);

            else if((sender as TextBox).Text.Contains("Default"))
            {
                tb.Text = string.Empty;
                tb.Foreground = Brushes.Black;
            }
                

        }
    }
}

