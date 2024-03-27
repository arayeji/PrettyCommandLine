using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using PrettyCommandLine;

namespace PrettyCommandLineExample;
public class Start
{

    static CommandLineBind commandLine;
    public static void Main(string[] args)
    {
        commandLine = new CommandLineBind();
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

        Commands age = new Commands(CommandsBase.CommandTypes.Command) { Command = "my age and your age is" };
        age.Variables.Add(new Variable(typeof(int)) { Name = "MyAge" });
        age.Variables.Add(new Variable(typeof(int)) { Name = "YourAge", Required = true });
        age.Executable += delegate (object sender, CommandExecuteEventArgs e) { 
            
            Console.WriteLine("Your age is " + e.Variables.Where(x=>x.Name=="YourAge").First().Value);
            Console.WriteLine("My age is " + e.Variables.Where(x=>x.Name=="MyAge").First().Value);

        };

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



        Commands StartService = new Commands(Commands.CommandTypes.Command) { Command = "service" };
        StartService.AddToMode(mode1);

        Commands SQAService = new Commands(Commands.CommandTypes.Command) { Command = "SQA" };
        SQAService.AddToMode(mode1);
        StartService.AddSubCommand(SQAService);

        Commands SQAServiceStart = new Commands(Commands.CommandTypes.Command) { Command = "start" };
        SQAServiceStart.AddToMode(mode1); 
        SQAService.AddSubCommand(SQAServiceStart);

        Commands SQAServiceStop = new Commands(Commands.CommandTypes.Command) { Command = "stop" };
        SQAServiceStop.AddToMode(mode1);
        SQAService.AddSubCommand(SQAServiceStop);

        commandLine.AddCommand(StartService);



        commandLine.Start();
    }




    static void Exit()
    {
        //Started = false;

        Console.WriteLine("10 Second to Close");
        Thread.Sleep(1000);
        Console.WriteLine("9 Second to Close");
        Thread.Sleep(1000);
        Console.WriteLine("8 Second to Close");
        Thread.Sleep(1000);
        Console.WriteLine("7 Second to Close");
        Thread.Sleep(1000);
        Console.WriteLine("6 Second to Close");
        Thread.Sleep(1000);
        Console.WriteLine("5 Second to Close");
        Thread.Sleep(1000);
        Console.WriteLine("4 Second to Close");
        Thread.Sleep(1000);
        Console.WriteLine("3 Second to Close");
        Thread.Sleep(1000);
        Console.WriteLine("2 Second to Close");
        Thread.Sleep(1000);
        Console.WriteLine("1 Second to Close");
        //ForceExit();
    }
}