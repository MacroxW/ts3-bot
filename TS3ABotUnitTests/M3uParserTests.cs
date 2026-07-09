using NUnit.Framework;
using System.IO;
using System.Text;
using TS3AudioBot.ResourceFactories.AudioTags;

namespace TS3ABotUnitTests
{
	[TestFixture]
	public class M3uParserTests
	{
		[Test]
		public void SimpleListTest()
		{
			var result = M3uReader.TryGetData(new MemoryStream(Encoding.UTF8.GetBytes(
@"#EXTINF:197,Delain - Delain - We Are The Others
/opt/music/bad/Delain.mp3
#EXTINF:314,MONO - MONO - The Hand That Holds the Truth
/opt/music/bad/MONO.mp3
#EXTINF:223,Deathstars - Deathstars - Opium
/opt/music/bad/Opium.mp3"
				))).Result;

			Assert.That(result.Count, Is.EqualTo(3));

			Assert.That(result[0].Title, Is.EqualTo("Delain - Delain - We Are The Others"));
			Assert.That(result[1].Title, Is.EqualTo("MONO - MONO - The Hand That Holds the Truth"));
			Assert.That(result[2].Title, Is.EqualTo("Deathstars - Deathstars - Opium"));

			Assert.That(result[0].TrackUrl, Is.EqualTo("/opt/music/bad/Delain.mp3"));
			Assert.That(result[1].TrackUrl, Is.EqualTo("/opt/music/bad/MONO.mp3"));
			Assert.That(result[2].TrackUrl, Is.EqualTo("/opt/music/bad/Opium.mp3"));
		}

		[Test]
		public void ListWithM3uHeaderTest()
		{
			var result = M3uReader.TryGetData(new MemoryStream(Encoding.UTF8.GetBytes(
@"#EXTM3U
#EXTINF:1337,Never gonna give you up
C:\Windows\System32\firewall32.cpl
#EXTINF:1337,Never gonna let you down
C:\Windows\System32\firewall64.cpl"
				))).Result;

			Assert.That(result.Count, Is.EqualTo(2));

			Assert.That(result[0].Title, Is.EqualTo("Never gonna give you up"));
			Assert.That(result[1].Title, Is.EqualTo("Never gonna let you down"));

			Assert.That(result[0].TrackUrl, Is.EqualTo(@"C:\Windows\System32\firewall32.cpl"));
			Assert.That(result[1].TrackUrl, Is.EqualTo(@"C:\Windows\System32\firewall64.cpl"));
		}

		[Test]
		public void ListWithoutMetaTagsTest()
		{
			var result = M3uReader.TryGetData(new MemoryStream(Encoding.UTF8.GetBytes(
@"
C:\PepeHands.jpg
./do/I/look/like/I/know/what/a/Jaypeg/is
"
				))).Result;

			Assert.That(result.Count, Is.EqualTo(2));

			Assert.That(result[0].Title, Is.Null);
			Assert.That(result[1].Title, Is.Null);

			Assert.That(result[0].TrackUrl, Is.EqualTo(@"C:\PepeHands.jpg"));
			Assert.That(result[1].TrackUrl, Is.EqualTo("./do/I/look/like/I/know/what/a/Jaypeg/is"));
		}
	}
}
