using NUnit.Framework;
using TS3AudioBot.Config;
using TS3AudioBot.ResourceFactories;
using TS3AudioBot.ResourceFactories.Youtube;

namespace TS3ABotUnitTests
{
	[TestFixture]
	public class ResourceFactoriesTests
	{
		[Test]
		public void Factory_YoutubeFactoryTest()
		{
			var ctx = new ResolveContext(null, null);
			using IResourceResolver rfac = new YoutubeResolver(new ConfResolverYoutube());
			// matching links
			Assert.That(rfac.MatchResource(ctx, "https://www.youtube.com/watch?v=robqdGEhQWo"), Is.EqualTo(MatchCertainty.Always));
			Assert.That(rfac.MatchResource(ctx, "https://youtu.be/robqdGEhQWo"), Is.EqualTo(MatchCertainty.Always));
			Assert.That(rfac.MatchResource(ctx, "https://discarded-ideas.org/sites/discarded-ideas.org/files/music/darkforestkeep_symphonic.mp3"), Is.EqualTo(MatchCertainty.Never));
			Assert.That(rfac.MatchResource(ctx, "http://splamy.de/youtube.com/youtu.be/fake.mp3"), Is.Not.EqualTo(MatchCertainty.Always));

			// restoring links
			Assert.That(rfac.RestoreLink(ctx, new AudioResource { ResourceId = "robqdGEhQWo" }), Is.EqualTo("https://youtu.be/robqdGEhQWo"));
		}
	}
}
