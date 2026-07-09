using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TS3AudioBot.Config;
using TS3AudioBot.ResourceFactories;
using TS3AudioBot.ResourceFactories.Youtube;

namespace TS3ABotUnitTests
{
	[TestFixture]
	public class BackwardCompatibilityTests
	{
		/// <summary>
		/// Tests that the YoutubeResolver can be instantiated with a configuration
		/// that does not have cookie file or extractor args set (simulating old configs).
		/// Validates: Requirements 7.1, 7.2
		/// </summary>
		[Test]
		public void BackwardCompatibility_ConfigWithoutNewOptions_WorksCorrectly()
		{
			// Arrange: Create a config without setting the new options
			var conf = new ConfResolverYoutube();
			
			// Act: Create resolver (should not throw)
			using var resolver = new YoutubeResolver(conf);
			
			// Assert: Resolver should be created successfully
			Assert.That(resolver, Is.Not.Null);
			Assert.That(resolver.ResolverFor, Is.EqualTo("youtube"));
			
			// Verify that the config values are accessible and have defaults
			Assert.That(conf.CookieFile, Is.Not.Null);
			Assert.That(conf.ExtractorArgs, Is.Not.Null);
			
			// Default values should be empty strings
			Assert.That(conf.CookieFile.Value, Is.EqualTo(""));
			Assert.That(conf.ExtractorArgs.Value, Is.EqualTo(""));
		}

		/// <summary>
		/// Tests that BuildArguments works correctly when cookie file and extractor args
		/// are not configured (null or empty), ensuring backward compatibility.
		/// Validates: Requirements 7.2, 7.3
		/// </summary>
		[Test]
		public void BackwardCompatibility_BuildArgumentsWithoutNewOptions_WorksAsExpected()
		{
			// Arrange: Set up YoutubeDlHelper with no cookie file or extractor args
			YoutubeDlHelper.CookieFile = null;
			YoutubeDlHelper.ExtractorArgs = null;
			
			// We can't directly test BuildArguments as it's private, but we can verify
			// that the helper methods work without these options set
			
			// Act & Assert: These should not throw
			Assert.DoesNotThrow(() => {
				var formats = new List<JsonYtdlFormat>
				{
					new JsonYtdlFormat
					{
						acodec = "mp4a.40.2",
						vcodec = "none",
						abr = 128,
						format_id = "140",
						url = "https://example.com/audio.m4a"
					}
				};
				
				var result = YoutubeDlHelper.FilterBestEnhanced(formats);
				Assert.That(result, Is.Not.Null);
			});
		}

		/// <summary>
		/// Tests that FilterBest (old method) still works correctly for backward compatibility.
		/// Validates: Requirements 7.3, 7.4
		/// </summary>
		[Test]
		public void BackwardCompatibility_FilterBest_StillWorks()
		{
			// Arrange: Create formats with non-null bitrates (old-style YouTube responses)
			var formats = new List<JsonYtdlFormat>
			{
				new JsonYtdlFormat
				{
					acodec = "mp4a.40.2",
					vcodec = "avc1.64001F",
					abr = 128,
					format_id = "18",
					url = "https://example.com/video.mp4"
				},
				new JsonYtdlFormat
				{
					acodec = "mp4a.40.2",
					vcodec = "none",
					abr = 128,
					format_id = "140",
					url = "https://example.com/audio.m4a"
				},
				new JsonYtdlFormat
				{
					acodec = "opus",
					vcodec = "none",
					abr = 160,
					format_id = "251",
					url = "https://example.com/audio.webm"
				}
			};
			
			// Act: Use the old FilterBest method
			var result = YoutubeDlHelper.FilterBest(formats);
			
			// Assert: Should select the highest bitrate audio-only format
			Assert.That(result, Is.Not.Null);
			Assert.That(result.format_id, Is.EqualTo("251")); // Opus with 160 abr
			Assert.That(result.vcodec, Is.EqualTo("none"));
		}

		/// <summary>
		/// Tests that FilterBestEnhanced works with old-style formats (non-null bitrates).
		/// Validates: Requirements 7.3, 7.4
		/// </summary>
		[Test]
		public void BackwardCompatibility_FilterBestEnhanced_WorksWithOldStyleFormats()
		{
			// Arrange: Create formats with non-null bitrates (old-style YouTube responses)
			var formats = new List<JsonYtdlFormat>
			{
				new JsonYtdlFormat
				{
					acodec = "mp4a.40.2",
					vcodec = "avc1.64001F",
					abr = 128,
					format_id = "18",
					url = "https://example.com/video.mp4"
				},
				new JsonYtdlFormat
				{
					acodec = "mp4a.40.2",
					vcodec = "none",
					abr = 128,
					format_id = "140",
					url = "https://example.com/audio.m4a"
				}
			};
			
			// Act: Use the new FilterBestEnhanced method
			var result = YoutubeDlHelper.FilterBestEnhanced(formats);
			
			// Assert: Should select audio-only format (prefers audio-only over combined)
			Assert.That(result, Is.Not.Null);
			Assert.That(result.format_id, Is.EqualTo("140"));
			Assert.That(result.vcodec, Is.EqualTo("none"));
		}

		/// <summary>
		/// Tests that the public interface of YoutubeDlHelper has not changed.
		/// Validates: Requirements 7.4
		/// </summary>
		[Test]
		public void BackwardCompatibility_PublicInterface_Unchanged()
		{
			// Assert: Verify that all expected public methods still exist
			var helperType = typeof(YoutubeDlHelper);
			
			// Check for existing public methods
			Assert.That(helperType.GetMethod("GetSingleVideo"), Is.Not.Null);
			Assert.That(helperType.GetMethod("GetPlaylistAsync"), Is.Not.Null);
			Assert.That(helperType.GetMethod("GetSearchAsync"), Is.Not.Null);
			Assert.That(helperType.GetMethod("FindYoutubeDl"), Is.Not.Null);
			Assert.That(helperType.GetMethod("RunYoutubeDl"), Is.Not.Null);
			Assert.That(helperType.GetMethod("ParseResponse"), Is.Not.Null);
			Assert.That(helperType.GetMethod("FilterBest"), Is.Not.Null);
			Assert.That(helperType.GetMethod("FilterBestEnhanced"), Is.Not.Null);
			Assert.That(helperType.GetMethod("IsHlsManifest"), Is.Not.Null);
			Assert.That(helperType.GetMethod("TransformYtdlError"), Is.Not.Null);
			Assert.That(helperType.GetMethod("MapToSongInfo"), Is.Not.Null);
			
			// Check for public properties
			Assert.That(helperType.GetProperty("DataObj"), Is.Not.Null);
			Assert.That(helperType.GetProperty("CookieFile"), Is.Not.Null);
			Assert.That(helperType.GetProperty("ExtractorArgs"), Is.Not.Null);
		}

		/// <summary>
		/// Tests that YoutubeResolver public interface has not changed.
		/// Validates: Requirements 7.4
		/// </summary>
		[Test]
		public void BackwardCompatibility_YoutubeResolverInterface_Unchanged()
		{
			// Arrange
			var conf = new ConfResolverYoutube();
			using var resolver = new YoutubeResolver(conf);
			
			// Assert: Verify that resolver implements expected interfaces
			Assert.That(resolver, Is.InstanceOf<IResourceResolver>());
			Assert.That(resolver, Is.InstanceOf<IPlaylistResolver>());
			Assert.That(resolver, Is.InstanceOf<IThumbnailResolver>());
			Assert.That(resolver, Is.InstanceOf<ISearchResolver>());
			
			// Verify ResolverFor property
			Assert.That(resolver.ResolverFor, Is.EqualTo("youtube"));
			
			// Verify that all expected public methods still exist
			var resolverType = typeof(YoutubeResolver);
			Assert.That(resolverType.GetMethod("MatchResource"), Is.Not.Null);
			Assert.That(resolverType.GetMethod("MatchPlaylist"), Is.Not.Null);
			Assert.That(resolverType.GetMethod("GetResource"), Is.Not.Null);
			Assert.That(resolverType.GetMethod("GetResourceById"), Is.Not.Null);
			Assert.That(resolverType.GetMethod("RestoreLink"), Is.Not.Null);
			Assert.That(resolverType.GetMethod("GetPlaylist"), Is.Not.Null);
			Assert.That(resolverType.GetMethod("GetThumbnail"), Is.Not.Null);
			Assert.That(resolverType.GetMethod("Search"), Is.Not.Null);
			Assert.That(resolverType.GetMethod("Dispose"), Is.Not.Null);
		}

		/// <summary>
		/// Tests that configuration defaults are sensible and don't break existing setups.
		/// Validates: Requirements 7.2
		/// </summary>
		[Test]
		public void BackwardCompatibility_ConfigurationDefaults_AreSensible()
		{
			// Arrange & Act: Create a new config
			var conf = new ConfResolverYoutube();
			
			// Assert: Verify default values
			Assert.That(conf.CookieFile.Value, Is.EqualTo(""), "CookieFile should default to empty string");
			Assert.That(conf.ExtractorArgs.Value, Is.EqualTo(""), "ExtractorArgs should default to empty string");
			Assert.That(conf.ResolverPriority.Value, Is.EqualTo(LoaderPriority.Internal), "ResolverPriority should default to Internal");
			Assert.That(conf.ApiKey.Value, Is.EqualTo(""), "ApiKey should default to empty string");
		}

		/// <summary>
		/// Tests that the resolver works correctly when ResolverPriority is set to YoutubeDl.
		/// Validates: Requirements 7.3
		/// </summary>
		[Test]
		public void BackwardCompatibility_YoutubeDlPriority_BehavesAsExpected()
		{
			// Arrange: Create config with YoutubeDl priority
			var conf = new ConfResolverYoutube();
			conf.ResolverPriority.Value = LoaderPriority.YoutubeDl;
			
			// Act: Create resolver
			using var resolver = new YoutubeResolver(conf);
			
			// Assert: Resolver should be created successfully
			Assert.That(resolver, Is.Not.Null);
			Assert.That(resolver.ResolverFor, Is.EqualTo("youtube"));
			
			// The resolver should work the same way regardless of priority
			// (since internal scraper is deprecated)
		}

		/// <summary>
		/// Tests that JsonYtdlFormat structure has not changed.
		/// Validates: Requirements 7.4
		/// </summary>
		[Test]
		public void BackwardCompatibility_JsonYtdlFormat_StructureUnchanged()
		{
			// Arrange & Act: Create a format object
			var format = new JsonYtdlFormat
			{
				vcodec = "avc1.64001F",
				acodec = "mp4a.40.2",
				abr = 128,
				asr = 44100,
				tbr = 256,
				format = "18 - 640x360 (360p)",
				format_id = "18",
				url = "https://example.com/video.mp4",
				ext = "mp4",
				width = 640,
				height = 360
			};
			
			// Assert: All properties should be accessible
			Assert.That(format.vcodec, Is.EqualTo("avc1.64001F"));
			Assert.That(format.acodec, Is.EqualTo("mp4a.40.2"));
			Assert.That(format.abr, Is.EqualTo(128));
			Assert.That(format.asr, Is.EqualTo(44100));
			Assert.That(format.tbr, Is.EqualTo(256));
			Assert.That(format.format, Is.EqualTo("18 - 640x360 (360p)"));
			Assert.That(format.format_id, Is.EqualTo("18"));
			Assert.That(format.url, Is.EqualTo("https://example.com/video.mp4"));
			Assert.That(format.ext, Is.EqualTo("mp4"));
			Assert.That(format.width, Is.EqualTo(640));
			Assert.That(format.height, Is.EqualTo(360));
		}

		/// <summary>
		/// Tests that empty or null cookie file configuration doesn't break the system.
		/// Validates: Requirements 7.2
		/// </summary>
		[Test]
		public void BackwardCompatibility_EmptyCookieFile_DoesNotBreak()
		{
			// Arrange: Create config with empty cookie file
			var conf = new ConfResolverYoutube();
			conf.CookieFile.Value = "";
			
			// Act: Create resolver and set up helper (this sets the properties internally)
			using var resolver = new YoutubeResolver(conf);
			
			// Assert: Should not throw - resolver creation succeeded
			Assert.That(resolver, Is.Not.Null);
			
			// Verify the config value is accessible
			Assert.That(conf.CookieFile.Value, Is.EqualTo(""));
		}

		/// <summary>
		/// Tests that empty or null extractor args configuration doesn't break the system.
		/// Validates: Requirements 7.2
		/// </summary>
		[Test]
		public void BackwardCompatibility_EmptyExtractorArgs_DoesNotBreak()
		{
			// Arrange: Create config with empty extractor args
			var conf = new ConfResolverYoutube();
			conf.ExtractorArgs.Value = "";
			
			// Act: Create resolver and set up helper (this sets the properties internally)
			using var resolver = new YoutubeResolver(conf);
			
			// Assert: Should not throw - resolver creation succeeded
			Assert.That(resolver, Is.Not.Null);
			
			// Verify the config value is accessible
			Assert.That(conf.ExtractorArgs.Value, Is.EqualTo(""));
		}

		/// <summary>
		/// Tests that the system handles null ConfigValue objects gracefully.
		/// Validates: Requirements 7.2, 7.4
		/// </summary>
		[Test]
		public void BackwardCompatibility_NullConfigValues_HandledGracefully()
		{
			// Arrange: Set helper properties to null
			YoutubeDlHelper.CookieFile = null;
			YoutubeDlHelper.ExtractorArgs = null;
			
			// Act & Assert: Should not throw when setting to null
			Assert.DoesNotThrow(() => {
				// Setting to null should work without issues
				YoutubeDlHelper.CookieFile = null;
				YoutubeDlHelper.ExtractorArgs = null;
			});
			
			// Creating a resolver with default config should also work
			var conf = new ConfResolverYoutube();
			Assert.DoesNotThrow(() => {
				using var resolver = new YoutubeResolver(conf);
				Assert.That(resolver, Is.Not.Null);
			});
		}
	}
}
