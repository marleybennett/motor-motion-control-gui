# motor-motion-control-gui

Front end GUI software for ECE 342: Junior Design Final Project -- Motor Motion Control group.

This project will ultimately serve as the front end interface for a motion motor controller. This motor is designed for use with Oregon State's Mars Rover arm.

The GUI allows users to modify motion control parameters via the text box entry forms.  You can submit a value via the "Submit" button or "Enter" key.  The input will be checked to make sure it is a float value and within the defined range.  Submitting a P,I,D value should result in immediate change in the GUI.  Submitting a change to the encoder value will take longer to change.  The GUI will poll the quadrature encoder and show the results as it changes.

**Instructions for Adding Parameters**
1. Develop the xaml representation for the parameter.  Base this on the below code that define a TextBlock (description), TextBox (text entry), and button (to submit parameter value).  Modify the rows and columns to change the position of the blocks.  Add new rows or columns in the grid definition as necessary.

                <!---Parameter X-->
            <TextBlock x:Name="description_x" Grid.Row="Y"
                    HorizontalAlignment="Left" VerticalAlignment="Center" 
                    TextWrapping="Wrap" Margin="10" 
                    Grid.Column="2" Grid.ColumnSpan="2"
                />

            <TextBox  x:Name="text_x" Grid.Row="Y+1"
                    HorizontalAlignment="Right" VerticalAlignment="Top"
                    Margin="10" Grid.Column="2"
                    Tag="Angle" KeyDown="CheckKey"
               />

            <Button x:Name="button_x" Grid.Row="Y+1"
                    HorizontalAlignment="Left" VerticalAlignment="Top"
                    Grid.Column="3" Margin="10" 
                    Content="Submit" Click ="ButtonSubmit" ClickMode="Release"
                />

2. Update x:Name for each new xaml entry by updating x to be a lowercase value that will be used as the id when communicating with the microcontroller over serial.
3. In MainWindow.xaml.cs, in InitializeParameter() function, copy the new Parameter(...) line and modify as necessary
  - Use updated names for xaml elements
  - Add name of parameter, range values, default value, units, and optionally additional information.
 
 Now, you should be able to use the parameter value as any others, including for serial messaging, provided the firmware is compatable with the message id you set up.
    
  
  *Note: Adding additional rows may affect other components of the grid layout.  Be aware that you may have to tinker with the "Grid.RowSpan" and "Grid.ColumnSpan" for other elements to make it look the way you'd like.
  
  Optionally, there is embedded support for including encoder values that simply present information on the state of the encoder (can not be changed).  This was not used in the final implementation, but can be reenabled by uncommenting "InitializeEncoders()".



