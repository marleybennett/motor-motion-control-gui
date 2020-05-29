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
using System.Threading.Tasks;
using System.Collections.Generic;

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
        Queue<byte[]> messages = new Queue<byte[]>();


        /* Struct for motion control parameters*/
        struct Parameter
        {
            private readonly string name;       // Name of parameter
            private readonly float min;         // Min parameter value
            private readonly float max;         // Max paramerter vaue
            public float defaultVal;            // Default parameter value
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
                this.currentVal = -1; // Initialize current value to [-1 (waiting on firmware)
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
                this.currentVal = -1; // Initialize to -1 before resetting with value from encoder
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

            // Build combo box with valid serial port options
            string[] ports = SerialPort.GetPortNames();
            portCombo.ItemsSource = ports;

            InitializeParameters();
            // Encoders are not being used for this final implementation
            //InitializeEncoders();

            // Initialize data received handler
             _port.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(Receive);
        }

        /*****************
         * NAME: getEncoder
         * DESCRIPION: Request updated encoder value
         * Asynchronously make request every 2 seconds
         * ***************/
        private async void getEncoder()
        {
            // Asynchronously generate request for encoder value
            await Task.Run(async () =>
            {
                while (true)
                {
                    generateHexString("e");
                    await Task.Delay(2000);
                }    
            });        
        }

        /*****************
         * NAME: writeData
         * DESCRIPION: Send serial message to firmware
         * Asynchronously check queue and send any messages every 500 ms
         * ***************/
        private async void writeData()
        {
            // Send serial message
            await Task.Run(async () =>
            {
                while (true)
                {
                    // If port is open and there is a message to send
                    if (_port.IsOpen && (messages.Count != 0))
                    {
                    // Get next message from queue
                    byte[] hexstring = messages.Dequeue();
                        try
                        {
                            _port.Write(hexstring, 0, hexstring.Length);
                            await Task.Delay(500);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Failed to send data: " + ex + "\n");
                        }
                    }
                }
            });
        }

        /*****************
         * NAME: addMessage
         * DESCRIPION: Check if port is open and add message to queue
         * Return 1 if port is closed, 0 otherwise
         * ***************/
        private int addMessage(byte[] msg)
        {
            if (_port.IsOpen)
                messages.Enqueue(msg);
            else
            {
                return 1;
            }
            return 0;
        }


        /*****************
         * NAME: getPort
         * DESCRIPION: Connect to COM port
         * Called when user selects a COM port from the drop down menu
         * ***************/
        private void getPort(object sender, EventArgs e)
        {
            try
            {
                // Set port to user selected port
                _port.PortName = (sender as ComboBox).SelectedItem.ToString();
                InitializeSerialPort();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
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
                new Parameter("Proportional", 0, 0xFFFFFFFF/1000, 0f, "", text_p, button_p, description_p, "Proportional component of PID"),
                new Parameter("Integral", 0, 0xFFFFFFFF/1000, 1/8f, "", text_i, button_i, description_i, "Integral component of PID"),
                new Parameter("Derviative", 0, 0xFFFFFFFF/1000, 0f, "", text_d, button_d, description_d, "Derviative component of PID"),
                new Parameter("Encoder Position", 0, 360, 0, "degrees", text_e, button_e, description_e, "Target encoder position"),
             };
            
            numParameters = paramArr.Length;
            
            // Initialize textbox for each parameter
            for (int i = 0; i < numParameters; i++)
                InitializeParameterXaml(paramArr[i]);
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
            string text = p.Name.ToUpper() + "\n" + "Range: " + p.Min + " - " + p.Max + " " + p.Units +"\nCurrent Value: ";

            // Print current value if available
            if (p.currentVal == -1)
                text = text + "Awaiting feedback from encoder...";
            else
                text = text + p.CurrentVal + " " + p.Units;

            // Add additional details if set
            if (p.AdditionalDetails != null)
                text = text + "\nAdditional Details: " + p.AdditionalDetails;

            // Set text to textbox
            p.TbDescription.Text = text;
        }


        /*****************
         * NAME: UpdateEncoderState
         * DESCRIPION: Generate request for encoder value
         * ***************/
        private void updateEncoderState(object sender, RoutedEventArgs e)
        {
            generateHexString("e");
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
                return -1;
                

            // Check range
            if (valueFloat < p.Min|| valueFloat > p.Max)
                return -1;

                
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
            if (validFloat != -1)
            {
                generateHexString(p, validFloat);
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

        /*****************
         * NAME: startStop
         * DESCRIPION: Start/stop motor based off of current text on button
         * Send appropriate message and change stop/start button text
         * ***************/
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

        /*****************
         * NAME: InitailizeSerialPort
         * DESCRIPION: Initialize serial port
         * Set up parameter and encoder values
         * ***************/
        private void InitializeSerialPort() {
        
            _port.BaudRate = 9600;
            _port.Parity = Parity.None;
            _port.StopBits = StopBits.One;
            _port.DataBits = 8;
            _port.ReadTimeout = 200;
            _port.WriteTimeout = 50;

            try
            {
                _port.Open();
                MessageBox.Show("Connected to " + _port.PortName);
                
                // Request values for p, i, d parameters
                generateHexString("p");
                generateHexString("i");
                generateHexString("d");

                // Request polling for encoder data
                getEncoder();

                // Write messages
                writeData();
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
            byte[] message = new byte[_port.ReadBufferSize];
            _port.Read(message, 0, message.Length);
            Dispatcher.Invoke(DispatcherPriority.Send, new UpdateUiTextDelegate(InterpretData), message);
        }


        /*****************
         * NAME: InterpretData
         * DESCRIPION: Decode message received from serial
         * ***************/
        private void InterpretData(byte[] message)
        {
            int i = 0;

            // Check start bits
            if (message[i] != 0x55 || message[++i] != 0xAA)
                return;

            // Get length of message
            int length = Convert.ToUInt16(message[++i]);

            // Get id
            string id = Encoding.ASCII.GetString(message, ++i, 1);

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

        /*****************
         * NAME: updateGui
         * DESCRIPION: Update appropriate value with value received from serial message
         * ***************/
        private void updateGui(string id, float val)
        {
            string tbDescriptionName = "description_" + id;
            writeParameter(tbDescriptionName, val);
        }

        /*****************
         * NAME: writeParameter
         * DESCRIPION: Write new parameter value to GUI.
         * ***************/
        private void writeParameter(string tbName, float val)
        {
            for (int i = 0; i < numParameters; i++)
            {
                // Find appropriate parameter
                if (paramArr[i].TbDescription.Name == tbName)
                {
                    paramArr[i].currentVal = val;
                    UpdateParameterDescription(paramArr[i]);
                    return;
                }
            }
        }

        /*****************
         * NAME: getParameterId
         * DESCRIPION: Find id with associated parameter
         * Return upper case ASCII value
         * ***************/
        private byte getParameterId(Parameter p)
        {
            string id = p.TbDescription.Name.Substring(12, 1).ToUpper();
            return (Encoding.ASCII.GetBytes(id)[0]);
        }


        /*****************
         * NAME: generateHexString
         * DESCRIPION: Generate hex message to send to serial
         * Use parameter p to generate 5 byte long message
         * ***************/
        private void generateHexString(Parameter p, float input)
        {
            int i = 0;
            int val = (int)(input * 1000);
            byte[] hexstring = new byte[8];

            // Start bytes
            hexstring[i] = 0x55;
            hexstring[++i] = 0xAA;

            // Length
            hexstring[++i] = 5;

            // Parameter id (upper case)
            hexstring[++i] = getParameterId(p);

            // Message
            for (int j = 3; j >= 0; j--)
            {
                hexstring[++i] = BitConverter.GetBytes((val >> (j * 8)) & 0xFF)[0];
            }

            if(addMessage(hexstring) == 1)
                MessageBox.Show("No port selected.");
            

            // Request updated parameter with lower case id
            generateHexString(Char.ConvertFromUtf32(getParameterId(p)).ToLower());
        }

        /*****************
         * NAME: generateHexString
         * DESCRIPION: Generate hex message to send to serial
         * Use id passed as parameter to generate message of length 1
         * ***************/
        private void generateHexString(string code)
        {
            byte[] hexstring = new byte[8];
            int i = 0;

            // start bytes
            hexstring[i] = 0x55;
            hexstring[++i] = 0xAA;

            // length
            hexstring[++i] = 1;

            // id (ASCII)
            hexstring[++i] = (Encoding.ASCII.GetBytes(code)[0]);

            // Send message
            addMessage(hexstring);
        }

        /*****************
         * NAME: InitializeEncoders
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
        }

        /*****************
        * NAME: UpdateEncoderDescription
        * DESCRIPION: Update encoder description with new current value.
        * ***************/
        private void UpdateEncoderDescription(Encoder e)
        {
            string text = e.Name.ToUpper() + "\nCurrent Value: ";

            // If encoder value has not been updated
            if (e.currentVal == -1)
                text = text + "Awaiting feedback from encoder...";

            else
                text = text + e.currentVal + e.Units;

            // Add additional details if set
            if (e.AdditionalDetails != null)
                text = text + "\nAdditional Details: " + e.AdditionalDetails;

            e.TbDescription.Text = text;
        }

        /* This function was not implemented across the system
        * Can be used for future development*/
        private void saveValues(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < numParameters; i++)
            {
                paramArr[i].defaultVal = paramArr[i].currentVal;
            }
            generateHexString("W");
        }



    }
}

