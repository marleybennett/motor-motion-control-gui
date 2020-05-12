# motor-motion-control-gui

Front end GUI software for ECE 342: Junior Design Final Project -- Motor Motion Control group.

This project will ultimately serve as the front end interface for a motion motor controller. This motor is designed for use with Oregon State's Mars Rover arm.

The GUI allows users to modify motion control parameters via the text box entry forms.  You can submit a value via the "Submit" button or "Enter" key.  The input will be checked to make sure it is a float value and within the defined range.  

The GUI will also allow users to view data from a quadrature encoder that relays details about the motor's current state.

**Instructions for Adding Parameters**
1. Create 2 new rows in grid definiton on MainWindow.xaml
2. Create new a TextBlock for the parameter description, a new TextBox for user entry, and new Button for submit (use code below or copy and paste from xaml file)

                <!---Parameter X-->
            <TextBlock x:Name="descriptionX" Grid.Row="Y"
                    HorizontalAlignment="Left" VerticalAlignment="Center" 
                    TextWrapping="Wrap" Margin="10" 
                    Grid.Column="2" Grid.ColumnSpan="2"
                />

            <TextBox  x:Name="textX" Grid.Row="Y+1"
                    HorizontalAlignment="Right" VerticalAlignment="Top"
                    Margin="10" Grid.Column="2"
                    Tag="Angle" KeyDown="CheckKey"
               />

            <Button x:Name="buttonX" Grid.Row="Y+1"
                    HorizontalAlignment="Left" VerticalAlignment="Top"
                    Grid.Column="3" Margin="10" 
                    Content="Submit" Click ="ButtonSubmit" ClickMode="Release"
                />

3. Update x:Name for each new xaml entry by updating ONLY the number (X in example, eg description4).
4. Update Grid.Row with next row for TextBlock (from previous parameter) (Y in example).
5. Update Grid.Row with row after row used for TextBlock for TextBox and Button (Y+1).
6. In MainWindow.xaml.cs, in InitializeParameter() function, copy the new Parameter(...) line and modify as necessary
  - Use updated names for xaml elements
  - Add name of parameter, range values, default value, units, and optionally additional information.
  
  **Instructions for Adding Encoder Data Values**
1. Create 2 new rows in grid definiton on MainWindow.xaml
2. Create new a TextBlock for the encoder description (use code below or copy and paste from xaml file)

            <!---Encoder X-->
            <TextBlock Margin="15,15,0,0" Name="encoderX"
                HorizontalAlignment="Stretch" VerticalAlignment="Top" 
                  TextWrapping="Wrap"
                  Grid.Row="Y" Grid.Column="4" Grid.ColumnSpan="2">
            </TextBlock>
            
3. Update x:Name for each new xaml entry by updating ONLY the number (X in example, eg encoder4).
4. Update Grid.Row with the row AFTER the last row used.  (eg if encoder3.GridRow = 5, encoder4.GridRow = 7)
6. In MainWindow.xaml.cs, in InitializeEncoder() function, copy the new Parameter(...) line and modify as necessary
  - Use updated names for xaml textbox
  - Add name of encoder value, units, and optionally additional information.
  
  
  *Note: Adding additional rows may affect other components of the grid layout.  Be aware that you may have to tinker with the "Grid.RowSpan" and "Grid.ColumnSpan" for other elements to make it look the way you'd like.



