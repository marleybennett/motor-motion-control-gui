/************************
 * Motor Motion Control
 * Junior Design 2
 * Front End GUI
 * MainWindow.xaml.cs
 * Author: Marley Bennett
 * ***********************/


using System;
using System.IO.Ports;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Documents;
using System.Text;

namespace MotorMotionControlGUI
{
    public partial class MainWindow : Window
    {
        /* Global Variables */
        SolidColorBrush osuBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#D3832B")); //Oregon State orange
        Parameter[] paramArr;
        Encoder[] encodArr;
        int numParameters;
        int numEncoders;
        Parameter? nullParam = null;

        /* Variables related to serial port*/
        SerialPort _port = new SerialPort();
        FlowDocument flowDoc = new FlowDocument();
        FlowDocumentReader flowDocRead = new FlowDocumentReader();
        Paragraph para = new Paragraph();

        /* Struct for motion control parameters*/
        struct Parameter
        {
            private readonly string name;       // Name of paraemter
            private readonly float min;         // Min parameter value
            private readonly float max;         // Max paramerter vaue
            private readonly float defaultVal;  // Default parameter value
            private readonly string units;      // Untits for parameter
            public float currentVal;            // Current value of parameter
            private readonly string additionalDetails; // Additional details about paramter
            private TextBox tbInput;            // XAML textbox for parameter value input
            private Button button;              // XAML button for submitting input
            private TextBlock tbDescription;    // XAML textbox for parameter description

            public Parameter(string name, float min, float max, float defaultValue, string units, TextBox tbInput, Button button, TextBlock tbDescription, string additionalDetails = null)
            {
                this.name = name;
                this.min = min;
                this.max = max;
                this.defaultVal = defaultValue;
                this.currentVal = defaultValue; // Initialize current value to default value
                this.units = units;
                this.additionalDetails = additionalDetails; // Optional : set to NULL if not set
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

        /* Struct for encoder data elements */
        struct Encoder
        {
            private readonly string name;       // Name of encoder data value
            private readonly string units;      // Untis of encoder data
            public float currentVal;            // Value of encoder data
            private readonly string additionalDetails; // Additional details about encoder
            private TextBlock tbDescription;    // XAML textbox with description of encoder data

            public Encoder(string name, string units, TextBlock tbDescription, string additionalDetails = null)
            {
                this.name = name;
                this.units = units;
                this.currentVal = 0; // Initialize to 0 before resetting with value from encoder
                this.additionalDetails = additionalDetails; //Optional: set to NULL if not set
                this.tbDescription = tbDescription;
            }

            public string Name { get { return name; } }
            public float CurrentVal { get { return currentVal; } }
            public string Units { get { return units; } }
            public string AdditionalDetails { get { return additionalDetails; } }
            public TextBlock TbDescription { get { return tbDescription; } }

        };


        public MainWindow()
        {
            InitializeComponent();
            InitializeParameters();
            InitializeEncoders();

            //TO-DO: Re-enable for integration
            //InitializeSerialPort();
        }


        /*****************
         * NAME: InitailizeParameters
         * DESCRIPION: Create struct of parameters with values.
         * Add new parameters here.
         * ***************/
        private void InitializeParameters()
        {
            // Initialize array of Parameter structs
            paramArr = new Parameter[]
             {
                //FORMAT: (name, min, max, defaultVal, units, XamlInputTextBox, XamlButton, XamlDescriptionTextBox, optional additionalData)
                // Note: add 'f' after decimal value to make it a float type   
                new Parameter("Position", 0, 365, 180, "degrees", text1, button1, description1, "Sets angular position of motor."),
                new Parameter("Speed", 0, 20, 10.23f, "m/s", text2, button2, description2),
                new Parameter("Param3", 0, 100, 3, "m/h", text3, button3, description3),
             };

            numParameters = paramArr.Length;

            // Initialize textbox for each parameter
            for (int i = 0; i < numParameters; i++)
                InitializeParameterXaml(paramArr[i]);
        }


        /*****************
         * NAME: InitailizeEncoders
         * DESCRIPION: Create struct of encoders with values.
         * Add new encoder values here.
         * ***************/
        private void InitializeEncoders()
        {
            // Initialize array of Encoder structs
            encodArr = new Encoder[]
            {
                // FORMAT: (value name, units, XamlTextBox, optional additionalDetails)
                new Encoder("Encoder 1", "units", encoder1, "Data from encoder parameter 1"),
                new Encoder("Encoder 2", "m/s", encoder2, "Data from encoder parameter 2"),
                new Encoder("Encoder 3", "m/h", encoder3, "Data from encoder parameter 3")
            };
            
            numEncoders = encodArr.Length;

            // Initialize textbox for each encoder
            for (int i = 0; i < numEncoders; i++)
                UpdateEncoderDescription(encodArr[i]);
        }


        /*****************
         * NAME: InitializeParameterXaml
         * DESCRIPION: Set up textbox corresponding to parameter value.
         * ***************/
        private void InitializeParameterXaml(Parameter p)
        {
            p.TbInput.Tag = p.Name;
            p.TbInput.Foreground = Brushes.Gray;
            p.TbInput.Text = "Default: " + p.DefaultVal;
            p.TbInput.BorderBrush = Brushes.Black;
            p.TbInput.AcceptsReturn = false;
            p.TbInput.BorderBrush = osuBrush;
            p.Button.Tag = p.Name;
            UpdateParameterDescription(p);
        }


        /*****************
         * NAME: UpdateParameterDescription
         * DESCRIPION: Update parameter description on GUI with new current value.
         * ***************/
        private void UpdateParameterDescription(Parameter p)
        {
            string text = p.Name.ToUpper() + "\n" + "Range: " + p.Min + " - " + p.Max + " " + p.Units + "\nCurrent Value: " + p.CurrentVal + " " + p.Units;
            
            // Add additional details if set
            if (p.AdditionalDetails != null)
                text = text + "\nAdditional Details: " + p.AdditionalDetails;

            // Set text to textbox
            p.TbDescription.Text = text;
        }


        /*****************
        * NAME: UpdateEncoderDescription
        * DESCRIPION: Update encoder description with new current value.
        * ***************/
        private void UpdateEncoderDescription(Encoder e)
        {
            string text = e.Name.ToUpper();

            // If encoder value has not been updated
            if (e.currentVal == 0)
                text = text + "\nAwaiting feedback from encoder...";

            else
                text = text + "\nCurrent Value: " + e.currentVal + e.Units;

            // Add additional details if set
            if (e.AdditionalDetails != null)
                text = text + "\nAdditional Details: " + e.AdditionalDetails;

            e.TbDescription.Text = text;
        }


        /*****************
         * NAME: GetParameter
         * DESCRIPION: Return parameter based on input tag
         * Returns null parameter if no parameter is found
         * ***************/
        private Parameter GetParameter(string tag)
        {
            for (int i = 0; i < numParameters; i++)
            {
                if (paramArr[i].Name == tag)
                    return paramArr[i];
            }

            return (Parameter)nullParam;
        }


        /*****************
         * NAME: GetParameter
         * DESCRIPION: Return parameter based on input button
         * Returns null parameter if no parameter is found
         * ***************/
        private Parameter GetParameter(Button button)
        {
            for (int i = 0; i < numParameters; i++)
            {
                if (paramArr[i].Button.Tag == button.Tag)
                    return paramArr[i];
            }

            return (Parameter)nullParam;
        }


        /*****************
         * NAME: CheckLimits
         * DESCRIPION: Check if input value is within limits of parameter p
         * Return 0 if invalid input
         * Return float value of input if valid
         * ***************/
        private float CheckLimits(Parameter p, string value)
        {
            float valueFloat;

            // Convert string value to float
            if (!float.TryParse(value, out valueFloat))
                return 0;

            // Check range
            if (valueFloat < p.Min || valueFloat > p.Max)
                return 0;

            return valueFloat;
        }


        /*****************
         * NAME: SubmitValue
         * DESCRIPION: Call SubmitValue with Parameter based on tag value.
         * ***************/
        private void SubmitValue(string tag)
        {
            try
            {
                SubmitValue(GetParameter(tag));
            }
            catch (Exception e)
            {
                MessageBox.Show("Mismatch between tag:  " + tag + " and Parameter name.\nError Message: " + e.Message);
            }
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

            // Valid input
            if (validFloat != 0)
            {
                MessageBox.Show("Updated " + p.TbInput.Tag.ToString() + " with new value of " + value + " " + p.Units);
                p.currentVal = validFloat;
                UpdateParameterDescription(p);
                /*TO-DO: Re-enable for integration
                 * TO-DO: Determine which data to send**/
                //SendData(p);
            }

            // Invalid input
            else
            {
                MessageBox.Show("Invalid parameter value.");
            }

            p.TbInput.Text = "Default: " + p.DefaultVal;
            p.TbInput.Foreground = Brushes.Gray;
        }


        /*****************
         * NAME: ButtonSubmit
         * DESCRIPION: Submit value based on button input
         * ***************/
        private void ButtonSubmit(object sender, RoutedEventArgs e)
        {
            try
            {
                SubmitValue(GetParameter(sender as Button));
            }
            catch(Exception ex)
            {
                MessageBox.Show("Mismatch between button:  " + (sender as Button).Tag + " and Parameter name.\nError Message: " + ex.Message);

            }
        }


        /*****************
         * NAME: CheckKey
         * DESCRIPION: If enter key pressed, submit value.
         * If any other key was pressed, clear "Default" text.
         * ***************/
        private void CheckKey(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Sender is TextBox object
            TextBox tb = sender as TextBox;
            int enter = 6;

            // Check if enter key was pressed
            if ((char)e.Key == enter)
                SubmitValue((string)tb.Tag);

            // Clear textbox if another key was typed
            else if((sender as TextBox).Text.Contains("Default"))
            {
                tb.Text = string.Empty;
                tb.Foreground = Brushes.Black;
            }
        }

/**********************************************
* The following functions are for serial communication.
* These will be fully implemented in the integration stage.
* These are the bare bones for what the Serial communication will look like.
* There has been minimal adaption for this project.
* ********************************************/


        /*****************
         * NAME: InitailizeSerialPort
         * DESCRIPION: Initialize serial port with parameters from communication protocol
         * ***************/
        private void InitializeSerialPort()
        {

            /*TO-DO: Check communication protocol and verify parameters*/
            _port.PortName = "COM1";
            _port.BaudRate = 9600;
            _port.Parity = Parity.None;
            _port.StopBits = StopBits.One;
            _port.DataBits = 8;
            _port.ReadTimeout = 200;
            _port.WriteTimeout = 50;

            // Initialize event when data is available for reading
            _port.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(Receive);

            _port.Open();
        }



        /*****************
         * NAME: Receive
         * DESCRIPION: Receive data from serial port
         * ***************/
        private delegate void UpdateUiTextDelegate(string text);
        private void Receive(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            string received_data = _port.ReadExisting();
            /*TO-DO: Where to save/send data*/
            Dispatcher.Invoke(DispatcherPriority.Send, new UpdateUiTextDelegate(WriteData), received_data);
        }


        /*****************
         * NAME: WriteData
         * DESCRIPION: Write data to GUI
         * ***************/
        private void WriteData(string text)
        {
            /**TO-DO: Determine best format to get this text into the appropriate text box
             * TO-DO: Determine messaging format to identify parameter associated with data**/
            para.Inlines.Add(text);
            flowDoc.Blocks.Add(para);
            flowDocRead.Document = flowDoc;
        }

        /*****************
         * NAME: SendData
         * DESCRIPION: Send data to serial port
         * ***************/
        private void SendData(Parameter p)
        {
            /*TO-DO: Determine messaging format*/
            if (_port.IsOpen)
            {
                try
                {
                    byte[] hexstring = Encoding.ASCII.GetBytes(p.currentVal.ToString()); // Temporarily sending current value
                    // Convert byte to byte[] to write
                    foreach (byte hexval in hexstring)
                    {
                        byte[] _hexval = new byte[] { hexval };
                        _port.Write(_hexval, 0, 1);
                        Thread.Sleep(1);
                    }
                }
                /*TO-DO: Add interface support for failure to send data*/
                catch (Exception ex)
                {
                    para.Inlines.Add("Failed to send data: " + ex + "\n");
                    flowDoc.Blocks.Add(para);
                    flowDocRead.Document = flowDoc;
                }
            }
        }
    }
}

