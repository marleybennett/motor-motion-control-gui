/************************
 * Motor Motion Control
 * Junior Design 2
 * Front End GUI
 * File: MainWindow.xaml.cs
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
            public float defaultVal;  // Default parameter value
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
                this.currentVal = 23; // Initialize to 0 before resetting with value from encoder
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

            /*TO-DO: Re-enable for integration*/
            InitializeSerialPort();

            /* This will call Receive() when data is available via serial port
             * This function will subsequently read in the data from the serial port
             * And store it in a FlowDocument (required for WPF)
             * Then it will call, InterpretEncoderData(flowDoc) which interprets the flowDocument
             * And updates the appropriate encoder
             * 
             TO-DO: Re-enable for integration*/
//             _port.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(Receive);

       
            // Proof of concept for encoder value update
            // Called when first "SUBMIT" button is pressed
            button_p.Click += new RoutedEventHandler(InterpretEncoderData);
        }


        /*****************
        * NAME: InterpretEncoderData
        * DESCRIPION: PROOF OF CONCEPT
        * Updates random encoder parameter with random value
        * Similar to functionality of reading flowDocument with message from SerialPort and updating appropriate encoder value
        * ***************/
        private void InterpretEncoderData(object sender, EventArgs e)
        {

            byte[] message = { 0x55, 0xAA, 0x05, 0x65, 0x00, 0x05, 0x7E, 0x40 };
            WriteData(message);


            /* var rand = new Random();

            // Generates random encoder parameter from 0-2 
            int encoderNum = (int)rand.Next(3);

            // Saves random value from 0 - 100 for given encoder
            try
            {
                encodArr[encoderNum].currentVal = (float)rand.Next(101);
                UpdateEncoderDescription(encodArr[encoderNum]);
                MessageBox.Show("Update encoder " + encodArr[encoderNum].Name + " with value " + encodArr[encoderNum].currentVal);
            }
            catch (Exception err)
            {
                MessageBox.Show("could not reset current value." + err);
            }*/

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
                new Parameter("Proportional", 0, 0xFFFFFFFF/1000, 180, "degrees", text_p, button_p, description_p, "Proportional component of PID"),
                new Parameter("Integral", 0, 0xFFFFFFFF/1000, 10.23f, "m/s", text_i, button_i, description_i, "Integral component of PID"),
                new Parameter("Derviative", 0, 0xFFFFFFFF/1000, 3.5f, "m/s", text_d, button_d, description_d, "Derviative component of PID"),
             };
            
            numParameters = paramArr.Length;

            // Initialize textbox for each parameter
            for (int i = 0; i < numParameters; i++)
                InitializeParameterXaml(paramArr[i]);
            
            generateHexString("P");
            generateHexString("I");
            generateHexString("D");
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
                new Encoder("Position", "degrees", encoder1, "Current angular position."),
                //new Encoder("Encoder 2", "m/s", encoder2, "Data from encoder parameter 2"),
                //new Encoder("Encoder 3", "m/h", encoder3, "Data from encoder parameter 3")
            };
            
            numEncoders = encodArr.Length;

            // Initialize textbox for each encoder
            for (int i = 0; i < numEncoders; i++)
                UpdateEncoderDescription(encodArr[i]);

            generateHexString("P");
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
                generateHexString(p);
                Thread.Sleep(1000);
                generateHexString("E");
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

        private void startStop(object sender, RoutedEventArgs e)
        {
            if((sender as Button).Content.ToString() == "Start Motor")
            {
                (sender as Button).Content = "Stop Motor";
                generateHexString("R");
            }
            else
            {
                (sender as Button).Content = "Start Motor";
                generateHexString("S");
            }

        }

        private void saveValues(object sender, RoutedEventArgs e)
        {
            for(int i = 0; i < numParameters; i++)
            {
                paramArr[i].defaultVal = paramArr[i].currentVal;
            }
            generateHexString("W");
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
            _port.PortName = "COM3";
            _port.BaudRate = 9600;
            _port.Parity = Parity.None;
            _port.StopBits = StopBits.One;
            _port.DataBits = 8;
            _port.ReadTimeout = 200;
            _port.WriteTimeout = 50;

            // Initialize event when data is available for reading
            _port.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(Receive);
            try
            {
                _port.Open();
            }
            catch(Exception ex)
            {
                MessageBox.Show("No device connection to " + _port.PortName + ". " + ex);
            }
        }


        /*****************
         * NAME: Receive
         * DESCRIPION: Receive data from serial port
         * ***************/
        private delegate void UpdateUiTextDelegate(byte[] message);
        private void Receive(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            MessageBox.Show("received message!");
            byte[] message = new byte[_port.ReadBufferSize];
            _port.Read(message, 0, message.Length);
            Dispatcher.Invoke(DispatcherPriority.Send, new UpdateUiTextDelegate(WriteData), message);
        }


        /*****************
         * NAME: WriteData
         * DESCRIPION: Write data to GUI
         * ***************/
        private void WriteData(byte[] message)
        {
            int i = 0;

            // Check start bits
            if (message[i] != 0x55 || message[++i] != 0xAA)
                return;

            // Get length of message
            int length = Convert.ToUInt16(message[++i]);

            // Get id
            string id = Encoding.ASCII.GetString(message, ++i, 1);
            MessageBox.Show("id: " + id);

            // Get integer value
            int valInt = 0;
            for(int j = length-2; j >= 0; j--)
            {
                int b = message[++i] << (j * 8);
                valInt = valInt | b;
            }

            // Convert to float
            float val = valInt / 1000;

            // Update appropriate parameter
            updateGui(id, val);
        }

        private void updateGui(string id, float val)
        {

            // There is currently only one encoder value so this is hard coded
            if(id == "e")
            {
                encodArr[0].currentVal = val;
                UpdateEncoderDescription(encodArr[0]);
            }
            else if (id == "p" || id == "i" || id == "d")
            {
                string tbDescriptionName = "description_" + id;
                writeParameter(tbDescriptionName, val);
                MessageBox.Show("Update w/val:  " + val);
            }
           
        }

        private void writeParameter(string tbName, float val)
        {
            for (int i = 0; i < numParameters; i++)
            {
                if (paramArr[i].TbDescription.Name == tbName)
                {
                    paramArr[i].currentVal = val;
                    UpdateParameterDescription(paramArr[i]);
                    return;
                }
            }
        }

        private byte getParameterId(Parameter p)
        {
            string id = p.TbDescription.Name.Substring(12, 1).ToUpper();
            MessageBox.Show("id: " + id);
            return (Encoding.ASCII.GetBytes(id)[0]);
        }


        /*****************
         * NAME: SendData
         * DESCRIPION: Send data to serial port
         * ***************/
        private void generateHexString(Parameter p)
        {
            int i = 0;
            int val = (int)(p.currentVal * 1000);
            byte[] hexstring = new byte[8];

            hexstring[i] = 0x55;
            hexstring[++i] = 0xAA;
            hexstring[++i] = 5;
            hexstring[++i] = getParameterId(p);

            for (int j = 3; j >= 0; j--)
            {
                hexstring[++i] = BitConverter.GetBytes((val >> (j * 8)) & 0xFF)[0];
            }

            SendData(hexstring);
        }

        private void generateHexString(string code)
        {
            byte[] hexstring = new byte[8];
            int i = 0;
            hexstring[i] = 0x55;
            hexstring[++i] = 0xAA;
            hexstring[++i] = 1;
            hexstring[++i] = (Encoding.ASCII.GetBytes(code)[0]);

            SendData(hexstring);
        }


        private void SendData(byte[] hexstring)
        {
            if (_port.IsOpen)
            {
                try
                {
                    //.Write(hexstring, 0, 8);

                    foreach (byte hexval in hexstring)
                    {
                        byte[] _hexval = new byte[] { hexval };
                        MessageBox.Show("byte to write: " + _hexval.ToString());
                        _port.Write(_hexval, 0, 1);
                        Thread.Sleep(1);
                    }

                    MessageBox.Show("Theoretically sent message...");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to send data: " + ex + "\n");

                }
            }
        }
    }
}

