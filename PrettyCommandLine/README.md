PrettyCommandLine is a versatile console command line handler designed to streamline command-line interactions with ease and clarity. With this NuGet package, users can effortlessly create and manage commands, modes, and subcommands, assigning distinct actions to each.

Key features include:

Flexible Command Structure: Easily add, organize, and customize commands, modes, and subcommands according to your application's requirements.

Validation Support: Seamlessly integrate variable checking and validation methods for precise command execution and error handling.

Multi-Mode Functionality: Enable commands to bind to multiple modes, allowing for dynamic behavior and increased versatility.

Custom Color Schemes: Enhance user experience and visual clarity by assigning unique color schemes to different modes, ensuring clear differentiation and intuitive navigation.

With PrettyCommandLine, developers can create intuitive, user-friendly command-line interfaces for their applications, enhancing usability and efficiency. Whether you're building a simple script or a complex application, PrettyCommandLine empowers you to handle command-line interactions with elegance and precision.

CommandLineBind commandLine = new CommandLineBind();
Modes mode1 = new Modes { Name = "Global", isDefault = true };
Modes mode2 = new Modes { Name = "Config", isDefault = true, Color = ConsoleColor.Gray };
Modes mode3 = new Modes { Name = "Service", isDefault = true, Color = ConsoleColor.White };

commandLine.AddMode(mode1);
commandLine.AddMode(mode2);
commandLine.AddMode(mode3);

Commands Print = new Commands(CommandsBase.CommandTypes.Command) { Command = "Today is" };
Print.AddToMode(mode1);
Print.Executable += delegate { Console.WriteLine("Today Is " + DateTime.Now.DayOfWeek); };

commandLine.AddCommand(Print);

Commands age = new Commands(CommandsBase.CommandTypes.Command) { Command = "my age is" };
age.Variables.Add(new Variable(typeof(int)) { Name = "Age" });
age.Executable += delegate (object sender, CommandExecuteEventArgs e) { Console.WriteLine("Your age is " + e.Variables[0].Value); };

commandLine.AddCommand(age);


Commands test1 = new Commands(CommandsBase.CommandTypes.Command) { Command = "who" };
test1.AddToMode(mode1);
test1.Executable += delegate { Console.WriteLine("Who what?"); };

Commands test11 = new Commands(CommandsBase.CommandTypes.Command) { Command = "am" };
test11.AddToMode(mode1);
test11.Executable += delegate { Console.WriteLine("Who am I?"); };


Commands test12 = new Commands(CommandsBase.CommandTypes.Command) { Command = "i" };
test12.AddToMode(mode1);
test12.Executable += delegate { Console.WriteLine("You are C#"); };

Commands test13 = new Commands(CommandsBase.CommandTypes.Command) { Command = "are" };
test13.AddToMode(mode1);
test13.IsExecutable = true;
test13.Variables.Add(new Variable(typeof(int)) { Name = "Name" , Required = false});
test13.Executable +=
  delegate (object sender, CommandExecuteEventArgs e) {
      if (e.Variables.Count == 0)
          Console.WriteLine("Who are you?");
      else
          Console.WriteLine("Who are these " + e.Variables[0].Value +" Guys?");

  };

Commands test14 = new Commands(CommandsBase.CommandTypes.Command) { Command = "you" };
test14.AddToMode(mode1);
test14.Executable += delegate (object sender, CommandExecuteEventArgs e) { 
  if(e.Variables.Count==0 )
      Console.WriteLine("I'm C#");
 else
      Console.WriteLine("I'm " + e.Variables[0].Value);

};
test14.Variables.Add(new Variable(typeof(string)) { Name = "Name" ,Required = true});


test1.AddSubCommand(test11);
test11.AddSubCommand(test12);

test1.AddSubCommand(test13);
test13.AddSubCommand(test14);

commandLine.AddCommand(test1);

commandLine.Start();