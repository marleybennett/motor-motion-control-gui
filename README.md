# motor-motion-control-gui

Front end GUI software for ECE 342: Junior Design Final Project -- Motor Motion Control group.

This project will ultimately serve as the front end interface for a motion motor controller. This motor is designed for use with Oregon State's Mars Rover arm.

The GUI will allow users to modify motion control parameters as well as view data from a quadrature encoder that relays details about the motor's current state.

**Instructions for Adding Parameters**
1. Create 2 new rows in grid definiton on MainWindow.xaml
2. Create new TextBlock for parameter description, new TextBox for user entry, and new Button for submit.
3. Edit TextBlock (parameter description) to set Grid.Row to the next row (eg. if previous parameter button was row 6, this TextBlock will be on row 7).
4. Edit Grid.Row on the TextBox and Button to the next row (row 8).
5. Update x:Name for each new xaml entry by updating only the number.
6. In MainWindow.xaml.cs, in MainWindow function, copy the new Parameter(...) line and modify as necessary
  - Use updated names for xaml elements
  - Add name of parameter, range values, default value, units, and optional additional information.


                <!---Sample Parameter-->
                <TextBlock Margin="10" x:Name="description1"
                HorizontalAlignment="Left" VerticalAlignment="Center" 
                  TextWrapping="Wrap"
                  Grid.Row="5" Grid.Column="2" Grid.ColumnSpan="2">
                </TextBlock>

                <TextBox Margin="10" x:Name="text*1*"
                HorizontalAlignment="Right" VerticalAlignment="Top"
                        Grid.Row="6" Grid.Column="2"
                        Tag="Angle" KeyDown="CheckEnter"
                        />

                <Button Margin="10" x:Name="button1"
                HorizontalAlignment="Left" VerticalAlignment="Top"
                 Content="Submit"
                    Click ="GetTextBox" ClickMode="Release"
                    Grid.Row="6" Grid.Column="3"
                    />
