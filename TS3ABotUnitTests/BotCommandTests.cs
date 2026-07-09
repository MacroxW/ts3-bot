using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using TS3AudioBot;
using TS3AudioBot.Algorithm;
using TS3AudioBot.CommandSystem;
using TS3AudioBot.CommandSystem.Ast;
using TS3AudioBot.CommandSystem.Commands;
using TS3AudioBot.Dependency;
using TS3AudioBot.Web.Api;
using TSLib;

#nullable enable
namespace TS3ABotUnitTests
{
	[TestFixture]
	public class BotCommandTests
	{
		[Test]
		public void BotCommandTest()
		{
			var execInfo = Utils.GetExecInfo("ic3");
			string? CallCommand(string command) => CommandManager.Execute(execInfo, command).GetAwaiter().GetResult().AsString();

			var output = CallCommand("!help");
			Assert.That(CallCommand("!h"), Is.EqualTo(output));
			Assert.That(CallCommand("!eval !h"), Is.EqualTo(output));
			Assert.Throws<CommandException>(() => CallCommand("!"));

			// Test random
			for (int i = 0; i < 1000; i++)
			{
				var r = int.Parse(CallCommand("!rng -10 100")!);
				Assert.That(r, Is.GreaterThanOrEqualTo(-10));
				Assert.That(r, Is.LessThan(100));
			}

			// Take
			Assert.Throws<CommandException>(() => CallCommand("!take"));
			Assert.That(CallCommand("!take 1 text"), Is.EqualTo("text"));
			Assert.Throws<CommandException>(() => CallCommand("!take 2 text"));
			Assert.Throws<CommandException>(() => CallCommand("!take -1 text"));
			Assert.That(CallCommand("!take 1 \"no more text\""), Is.EqualTo("no"));
			Assert.That(CallCommand("!take 2 \"no more text\""), Is.EqualTo("no more"));
			Assert.That(CallCommand("!take 1 1 \"no more text\""), Is.EqualTo("more"));
			Assert.That(CallCommand("!take 2 1 \"no more text\""), Is.EqualTo("more text"));
			Assert.Throws<CommandException>(() => CallCommand("!take 2 -1 \"no more text\""));
			Assert.That(CallCommand("!take 1 0 x text"), Is.EqualTo("te"));
			Assert.That(CallCommand("!take 1 1 x text"), Is.EqualTo("t"));
			Assert.That(CallCommand("!take 1 0 z text"), Is.EqualTo("text"));
			Assert.Throws<CommandException>(() => CallCommand("!take 1 1 z text"));
			Assert.That(CallCommand("!take 0 text"), Is.EqualTo(""));
			Assert.That(CallCommand("!take 0 0 text"), Is.EqualTo(""));
			Assert.That(CallCommand("!take 0 0 z text"), Is.EqualTo(""));

			// If
			Assert.Throws<CommandException>(() => CallCommand("!if a == a"));
			Assert.Throws<CommandException>(() => CallCommand("!if a == b"));
			Assert.That(CallCommand("!if a == a text"), Is.EqualTo("text"));
			Assert.That(CallCommand("!if a == b text"), Is.Null);
			Assert.That(CallCommand("!if a == b text other"), Is.EqualTo("other"));
			Assert.That(CallCommand("!if 1 == 1 text other"), Is.EqualTo("text"));
			Assert.That(CallCommand("!if 1 == 2 text other"), Is.EqualTo("other"));
			Assert.That(CallCommand("!if 1.0 == 1 text other"), Is.EqualTo("text"));
			Assert.That(CallCommand("!if 1.0 == 1.1 text other"), Is.EqualTo("other"));
			Assert.That(CallCommand("!if a == a text (!)"), Is.EqualTo("text"));
			Assert.Throws<CommandException>(() => CallCommand("!if a == b text (!)"));
		}

		[Test]
		public void TailStringTest()
		{
			var execInfo = Utils.GetExecInfo("ic3");
			string? CallCommand(string command) => CommandManager.Execute(execInfo, command).Result.AsString();
			var group = execInfo.GetModule<CommandManager>()!.RootGroup;
			group.AddCommand("cmd", new FunctionCommand(s => s));

			Assert.That(CallCommand("!cmd a"), Is.EqualTo("a"));
			Assert.That(CallCommand("!cmd a b"), Is.EqualTo("a b"));
			Assert.That(CallCommand("!cmd a \" b"), Is.EqualTo("a"));
			Assert.That(CallCommand("!cmd a b 1"), Is.EqualTo("a b 1"));
		}

		[Test]
		public void XCommandSystemFilterTest()
		{
			var filterList = new Dictionary<string, object?>
			{
				{ "help", null },
				{ "quit", null },
				{ "play", null },
				{ "ply", null }
			};

			var filter = Filter.GetFilterByName("ic3")!;

			// Exact match
			var result = filter.Filter(filterList, "help");
			Assert.That(result.Count(), Is.EqualTo(1));
			Assert.That(result.First().Key, Is.EqualTo("help"));

			// The first occurence of y
			result = filter.Filter(filterList, "y");
			Assert.That(result.Count(), Is.EqualTo(1));
			Assert.That(result.First().Key, Is.EqualTo("ply"));

			// The smallest word
			result = filter.Filter(filterList, "zorn");
			Assert.That(result.Count(), Is.EqualTo(1));
			Assert.That(result.First().Key, Is.EqualTo("ply"));

			// First letter match
			result = filter.Filter(filterList, "q");
			Assert.That(result.Count(), Is.EqualTo(1));
			Assert.That(result.First().Key, Is.EqualTo("quit"));

			// Ignore other letters
			result = filter.Filter(filterList, "palyndrom");
			Assert.That(result.Count(), Is.EqualTo(1));
			Assert.That(result.First().Key, Is.EqualTo("play"));

			filterList.Add("pla", null);

			// Ambiguous command
			result = filter.Filter(filterList, "p");
			Assert.That(result.Count(), Is.EqualTo(2));
			Assert.That(result.Any(r => r.Key == "ply"), Is.True);
			Assert.That(result.Any(r => r.Key == "pla"), Is.True);
		}

		private static string OptionalFunc(string? s = null) => s is null ? "NULL" : "NOT NULL";

		[Test]
		public void XCommandSystemTest()
		{
			var execInfo = Utils.GetExecInfo("ic3", false);
			string? CallCommand(string command) => CommandManager.Execute(execInfo, command).GetAwaiter().GetResult().AsString();

			var group = execInfo.GetModule<CommandManager>()!.RootGroup;
			group.AddCommand("one", new FunctionCommand(() => "ONE"));
			group.AddCommand("two", new FunctionCommand(() => "TWO"));
			group.AddCommand("echo", new FunctionCommand(s => s));
			group.AddCommand("optional", new FunctionCommand(GetType().GetMethod(nameof(OptionalFunc), BindingFlags.NonPublic | BindingFlags.Static)!));

			// Basic tests
			Assert.That(CommandManager.Execute(execInfo, new ICommand[] { new ResultCommand("one") }).Result.AsString(), Is.EqualTo("ONE"));
			Assert.That(CallCommand("!one"), Is.EqualTo("ONE"));
			Assert.That(CallCommand("!t"), Is.EqualTo("TWO"));
			Assert.That(CallCommand("!e TEST"), Is.EqualTo("TEST"));
			Assert.That(CallCommand("!o"), Is.EqualTo("ONE"));

			// Optional parameters
			Assert.Throws<CommandException>(() => CallCommand("!e"));
			Assert.That(CallCommand("!op"), Is.EqualTo("NULL"));
			Assert.That(CallCommand("!op 1"), Is.EqualTo("NOT NULL"));

			// Command chaining
			Assert.That(CallCommand("!e (!e TEST)"), Is.EqualTo("TEST"));
			Assert.That(CallCommand("!e (!t)"), Is.EqualTo("TWO"));
			Assert.That(CallCommand("!op (!e TEST)"), Is.EqualTo("NOT NULL"));
			Assert.That(CallCommand("!(!e on)"), Is.EqualTo("ONE"));

			// Command overloading
			var intCom = new Func<int, string>(_ => "INT");
			var strCom = new Func<string, string>(_ => "STRING");
			group.AddCommand("overlord", new OverloadedFunctionCommand(new[] {
				new FunctionCommand(intCom.Method, intCom.Target),
				new FunctionCommand(strCom.Method, strCom.Target)
			}));

			Assert.That(CallCommand("!overlord 1"), Is.EqualTo("INT"));
			Assert.That(CallCommand("!overlord a"), Is.EqualTo("STRING"));
			Assert.Throws<CommandException>(() => CallCommand("!overlord"));

			// Return unwrap
			var json = JsonValue.Create("WRAP");
			group.AddCommand("wrapjson", new FunctionCommand(new Func<JsonValue>(() => json)));
			Assert.That(CommandManager.Execute(execInfo, "!wrapjson").Result.AsRaw(), Is.EqualTo(json));
			Assert.That(CallCommand("!wrapjson"), Is.EqualTo("WRAP")); // AsString()
			Assert.That(CallCommand("!echo (!wrapjson)"), Is.EqualTo("WRAP"));
		}

		[Test]
		public void XCommandSystemTest2()
		{
			var execInfo = Utils.GetExecInfo("exact");
			string? CallCommand(string command) => CommandManager.Execute(execInfo!, command).GetAwaiter().GetResult().AsString();
			var group = execInfo.GetModule<CommandManager>()!.RootGroup;

			var o1 = new OverloadedFunctionCommand();
			o1.AddCommand(new FunctionCommand(new Action<int>((_) => { })));
			o1.AddCommand(new FunctionCommand(new Action<long>((_) => { })));
			group.AddCommand("one", o1);

			group.AddCommand("two", new FunctionCommand(new Action<StringSplitOptions>((_) => { })));

			var o2 = new CommandGroup();
			o2.AddCommand("a", new FunctionCommand(new Action(() => { })));
			o2.AddCommand("b", new FunctionCommand(new Action(() => { })));
			group.AddCommand("three", o2);

			Assert.Throws<CommandException>(() => CallCommand("!one"));
			Assert.Throws<CommandException>(() => CallCommand("!one \"\""));
			Assert.Throws<CommandException>(() => CallCommand("!one (!print \"\")"));
			Assert.Throws<CommandException>(() => CallCommand("!one string"));
			Assert.DoesNotThrow(() => CallCommand("!one 42"));
			Assert.DoesNotThrow(() => CallCommand("!one 4200000000000"));

			Assert.Throws<CommandException>(() => CallCommand("!two"));
			Assert.Throws<CommandException>(() => CallCommand("!two \"\""));
			Assert.Throws<CommandException>(() => CallCommand("!two (!print \"\")"));
			Assert.Throws<CommandException>(() => CallCommand("!two 42"));
			Assert.DoesNotThrow(() => CallCommand("!two None"));

			Assert.Throws<CommandException>(() => CallCommand("!three"));
			Assert.Throws<CommandException>(() => CallCommand("!three \"\""));
			Assert.Throws<CommandException>(() => CallCommand("!three (!print \"\")"));
			Assert.Throws<CommandException>(() => CallCommand("!three c"));
			Assert.DoesNotThrow(() => CallCommand("!three a"));
			Assert.DoesNotThrow(() => CallCommand("!three b"));
		}

		[Test]
		public void EnsureAllCommandsHaveEnglishDocumentationEntry()
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en");
			Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en");

			var execInfo = Utils.GetExecInfo("exact");
			var cmdMgr = execInfo.GetModule<CommandManager>()!;
			var errors = new List<string>();
			foreach (var cmd in cmdMgr.AllCommands)
			{
				if (string.IsNullOrEmpty(cmd.Description))
					errors.Add($"Command {cmd.FullQualifiedName} has no documentation");
			}
			if (errors.Count > 0)
				Assert.Fail(string.Join("\n", errors));
		}

		[Test]
		public void CommandParserTest()
		{
			TestStringParsing("!aaa", "aaa");
			TestStringParsing("!a\"aa", "a\"aa");
			TestStringParsing("!aaa\"", "aaa\"");
			TestStringParsing("!a'aa", "a'aa");
			TestStringParsing("!aaa'", "aaa'");
			TestStringParsing("!\"aaa\"", "aaa");
			TestStringParsing("!\"aaa", "aaa");
			TestStringParsing("!'aaa'", "aaa");
			TestStringParsing("!'aaa", "aaa");
			TestStringParsing("!\"a\"aa\"", "a");
			TestStringParsing("!'a'aa'", "a");
			TestStringParsing("!\"a'aa\"", "a'aa");
			TestStringParsing("!'a\"aa'", "a\"aa");
			TestStringParsing("!\"a\\'aa\"", "a\\'aa");
			TestStringParsing("!\"a\\\"aa\"", "a\"aa");
			TestStringParsing("!'a\\'aa'", "a'aa");
			TestStringParsing("!'a\\\"aa'", "a\\\"aa");
		}

		public static void TestStringParsing(string inp, string outp)
		{
			var astc = CommandParser.ParseCommandRequest(inp);
			var ast = ((AstCommand)astc).Parameter[0];
			Assert.That(((AstValue)ast).Value, Is.EqualTo(outp));
		}
	}

	internal static class Utils
	{
		public static ExecutionInformation GetExecInfo(string matcher, bool addMainCommands = true)
		{
			var cmdMgr = new CommandManager(null!);
			if (addMainCommands)
				cmdMgr.RegisterCollection(MainCommands.Bag);

			var execInfo = new ExecutionInformation();
			execInfo.AddModule(new CallerInfo(false) { SkipRightsChecks = true, CommandComplexityMax = int.MaxValue });
			execInfo.AddModule(new InvokerData((Uid)"InvokerUid"));
			execInfo.AddModule(Filter.GetFilterByName(matcher) ?? throw new Exception("Test filter not found"));
			execInfo.AddModule(cmdMgr);
			return execInfo;
		}
	}
}
