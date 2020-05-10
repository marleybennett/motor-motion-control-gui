# motor-motion-control-gui

Front end GUI software for ECE 342: Junior Design Final Project -- Motor Motion Control group.

This project will ultimately serve as the front end interface for a motion motor controller. This motor is designed for use with Oregon State's Mars Rover arm.

The GUI will allow users to modify motion control parameters as well as view data from a quadrature encoder that relays details about the motor's current state.

**Instructions for Adding Parameters**
1. Create 2 new rows in grid definiton on MainWindow.xaml
2. Create new TextBlock for parameter description, new TextBox for user entry, and new Button for submit (use code below or copy and paste from xaml file)
3. Update x:Name for each new xaml entry by updating ONLY the number (X in example).
4. Update Grid.Row with next row for TextBlock (from previous parameter) (Y in example).
5. Update Grid.Row with row after row used for TextBlock for TextBox and Button.
6. In MainWindow.xaml.cs, in MainWindow function, copy the new Parameter(...) line and modify as necessary
  - Use updated names for xaml elements
  - Add name of parameter, range values, default value, units, and optional additional information.

                <!---Parameter 1-->
            <TextBlock x:Name="descriptionX" Grid.Row="Y"
                    HorizontalAlignment="Left" VerticalAlignment="Center" 
                    TextWrapping="Wrap" Margin="10" 
                    Grid.Column="2" Grid.ColumnSpan="2"
                />

            <TextBox  x:Name="textX" Grid.Row="Y+1"
                    HorizontalAlignment="Right" VerticalAlignment="Top"
                    Margin="10" Grid.Column="2"
                    Tag="Angle" KeyDown="CheckEnter"
               />

            <Button x:Name="buttonX" Grid.Row="Y+1"
                    HorizontalAlignment="Left" VerticalAlignment="Top"
                    Grid.Column="3" Margin="10" 
                    Content="Submit" Click ="GetTextBox" ClickMode="Release"
                />
