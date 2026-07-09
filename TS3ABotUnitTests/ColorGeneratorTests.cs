using NUnit.Framework;
using TS3AudioBot.CommandSystem.Text;

namespace TS3ABotUnitTests
{
	[TestFixture]
	public class ColorGeneratorTests
	{
		[Test]
		public void Color1Test()
		{
			var res = TextMod.Format("Hello {0}".Mod().Color(Color.Red).Bold(), "World".Mod().Bold());
			Assert.That(res, Is.EqualTo("[B][COLOR=red]Hello [/COLOR]World"));
		}

		[Test]
		public void Color2Test()
		{
			var res = TextMod.Format("Hello {0}".Mod().Color(Color.Blue).Bold(), "World".Mod().Bold().Italic());
			Assert.That(res, Is.EqualTo("[B][COLOR=#00F]Hello [/COLOR][I]World"));
		}

		[Test]
		public void Color3Test()
		{
			var res = TextMod.Format("Hello {0}{1}".Mod().Color(Color.Orange).Bold(),
				"World".Mod().Bold().Italic(),
				", How are you?".Mod().Underline());
			Assert.That(res, Is.EqualTo("[B][COLOR=#FF8000]Hello [/COLOR][I]World[/B][U], How are you?"));
		}

		[Test]
		public void Color4Test()
		{
			var res = TextMod.Format("Hello {0} but {1}".Mod().Color(new Color(0, 0, 1)).Bold(),
				   "World".Mod().Bold().Italic(),
				   ", How are you?".Mod().Underline());
			Assert.That(res, Is.EqualTo("[B][COLOR=#000001]Hello [/COLOR][I]World[/I][COLOR=#000001] but [/B][U], How are you?"));
		}

		[Test]
		public void Color5Test()
		{
			var res = TextMod.Format("Hello {0} but {1}".Mod().Color(new Color(255, 17, 17)).Bold(),
					"World".Mod().Bold().Italic().Strike(),
					", How are you?".Mod().Underline());
			Assert.That(res, Is.EqualTo("[B][COLOR=#F11]Hello [/COLOR][I][S]World[/I][COLOR=#F11] but [/B][U], How are you?"));
		}

		[Test]
		public void Color6Test()
		{
			var res = TextMod.Format("Hello {0} but {1}",
					"World".Mod().Bold().Color(Color.Red),
					", How are you?".Mod().Bold().Color(Color.Red));
			Assert.That(res, Is.EqualTo("Hello [B][COLOR=red]World[/B] but [B][COLOR=red], How are you?"));
		}

		[Test]
		public void Color7Test()
		{
			var res = TextMod.Format("Hello {0} but {1}".Mod().Color(Color.Red),
					"World".Mod().Color(Color.Blue),
					", How are you?".Mod().Color(Color.Blue));
			Assert.That(res, Is.EqualTo("[COLOR=red]Hello [/COLOR][COLOR=#00F]World[/COLOR][COLOR=red] but [/COLOR][COLOR=#00F], How are you?"));
		}
	}
}
