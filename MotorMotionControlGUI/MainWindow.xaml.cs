
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
            private readonly float min;
            private readonly float max;
            private readonly float defaultVal;
            private readonly string units;
            public float currentVal;
            private readonly string additionalDetails;
            private TextBox tbInput;
            private Button button;
            private TextBlock tbDescription;

            public Parameter(string name, float min, float max, float defaultValue, string units, TextBox tbInput, Button button, TextBlock tbDescription, string additionalDetails = null)
            {
                this.name = name;
                this.min = min;
                this.max = max;
                this.defaultVal = defaultValue;
                this.currentVal = defaultValue;
                this.units = units;
                this.additionalDetails = additionalDetails;
                this.tbInput = tbInput;
                this.button = button;
                this.tbDescription = tbDescription;

            }

            public string Name { get { return name; } }
            public float Min { get { return min; } }
            public float Max { get { return max; } }
            public float DefaultVal { get { return defaultVal; } }
            public float CurrentVal { get { return currentVal; } }
            public string Units { get { return units; } }
            public string AdditionalDetails { get { return additionalDetails; } }
            public TextBox TbInput { get { return tbInput; } }
            public Button Button { get { return button; } }
            public TextBlock TbDescription { get { return tbDescription; } }

        };

        Parameter[] paramArr;
        int numParameters = 3;

        /*****************
         * NAME: UpdateDescription
         * DESCRIPION: Update parameter description with current value.
         * ***************/
        private void UpdateDescription(Parameter p)
        {
            p.TbDescription.Text = p.Name + "\n" + "Range: " + p.Min + " - " + p.Max + " " + p.Units + "\nCurrent Value: " + p.CurrentVal + " " + p.Units + "\n" + p.AdditionalDetails;
        }

        /*****************
         * NAME: InitializeTextBox
         * DESCRIPION: Set up textbox corresponding to parameter value.
         * ***************/
        private void InitializeTextBox(Parameter p)
        {
            p.TbInput.Tag = p.Name;
            p.TbInput.Foreground = Brushes.Gray;
            p.TbInput.Text = "Default: " + p.DefaultVal;
            p.TbInput.BorderBrush = Brushes.Black;
            p.TbInput.AcceptsReturn = false;
            p.TbInput.BorderBrush = osuBrush;
            p.Button.Tag = p.Name;
            UpdateDescription(p);
        }


        public MainWindow()
        {
            InitializeComponent();

           // Initialize array of parameter structs
           // All parameter values goes here in form: parameterName, minParameterValue, maxParameterValue, unitName, inputTextBoxName, submitButtonName, descriptionTextBoxName
           // Use "f" in front of decimal values to ensure conversion to float (rather than double) for processing with microcontroller
           paramArr = new Parameter[]
            {
                new Parameter("Position", 0, 365, 180, "degrees", text1, button1, description1),
                new Parameter("Speed", 0, 20, 10.23f, "m/s", text2, button2, description2),
                new Parameter("Param3", 0, 100, 3, "m/h", text3, button3, description3),

            };

            // Initialize textbox for each parameter
            for (int i = 0; i < numParameters; i++)
                InitializeTextBox(paramArr[i]);

        }

        /*****************
         * NAME: GetParameter
         * DESCRIPION: Return parameter based on input tag
         * ***************/
        private Parameter GetParameter(string tag)
        {
            for (int i = 0; i < numParameters; i++)
            {
                if (paramArr[i].Name == tag)
                    return paramArr[i];
            }

            return paramArr[0];
        }

        /*****************
         * NAME: GetParameter
         * DESCRIPION: Return parameter based on input button
         * ***************/
        private Parameter GetParameter(Button button)
        {
            for (int i = 0; i < numParameters; i++)
            {
                if (paramArr[i].Button.Tag == button.Tag)
                    return paramArr[i];
            }

            return paramArr[0];
        }

        /*****************
         * NAME: CheckLimits
         * DESCRIPION: Check if input value is within limits.
         * Return 0 if invalid input.
         * Return integer value of input if valid.
         * ***************/
        private float CheckLimits(Parameter p, string value)
        {
            float valueFloat;

            if (!float.TryParse(value, out valueFloat))
                return 0;

            if (valueFloat < p.Min || valueFloat > p.Max)
                return 0;

            return valueFloat;
        }

        private void SendViaSerial(Parameter p)
        {

        }


        /*****************
         * NAME: SubmitValue
         * DESCRIPION: Call SubmitValue with Parameter based on tag value.
         * ***************/
        private void SubmitValue(string tag)
        {
            SubmitValue(GetParameter(tag));
        }

        /*****************
         * NAME: SubmitValue
         * DESCRIPION: Check if input value is valid
         * If valid, update description and send value to motor.
         * ***************/
        private void SubmitValue(Parameter p)
        {
            // Get value from textbox
            string value = p.TbInput.Text;

            // Check if value is valid
            float validFloat = CheckLimits(p, value);

            // Valid
            if (validFloat != 0)
            {
                MessageBox.Show("Updated " + p.TbInput.Tag.ToString() + " with new value of " + value);
                p.currentVal = validFloat;
                UpdateDescription(p);
                SendViaSerial(p);
            }

            // Invalid
            else
            {
                MessageBox.Show("Invalid parameter value.");
            }

            p.TbInput.Text = "Default: " + p.DefaultVal;
            p.TbInput.Foreground = Brushes.Gray;
        }

        /*****************
         * NAME: GetTextBox
         * DESCRIPION: Submit value based on button input
         * ***************/
        private void GetTextBox(object sender, RoutedEventArgs e)
        {
            Parameter p = GetParameter(sender as Button);
            SubmitValue(p);
        }


        /*****************
         * NAME: CheckEnter
         * DESCRIPION: If enter key pressed, submit value.
         * If any other key was pressed, clear "Default" text.
         * ***************/
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

