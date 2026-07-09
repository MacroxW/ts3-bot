using NUnit.Framework;
using System;
using TSLib.Full;

namespace TS3ABotUnitTests
{
	[TestFixture]
	public class RingQueueTest
	{
		[Test]
		public void RingQueueTest1()
		{
			var q = new RingQueue<int>(3, 5);

			q.Set(0, 42);

			Assert.That(q.TryPeekStart(0, out int ov), Is.True);
			Assert.That(ov, Is.EqualTo(42));

			q.Set(1, 43);

			// already set
			Assert.Throws<ArgumentOutOfRangeException>(() => q.Set(1, 99));

			Assert.That(q.TryPeekStart(0, out ov), Is.True);
			Assert.That(ov, Is.EqualTo(42));
			Assert.That(q.TryPeekStart(1, out ov), Is.True);
			Assert.That(ov, Is.EqualTo(43));

			Assert.That(q.TryDequeue(out ov), Is.True);
			Assert.That(ov, Is.EqualTo(42));

			Assert.That(q.TryPeekStart(0, out ov), Is.True);
			Assert.That(ov, Is.EqualTo(43));
			Assert.That(q.TryPeekStart(1, out ov), Is.False);

			q.Set(3, 45);
			q.Set(2, 44);

			// buffer overfull
			Assert.Throws<ArgumentOutOfRangeException>(() => q.Set(4, 99));

			Assert.That(q.TryDequeue(out ov), Is.True);
			Assert.That(ov, Is.EqualTo(43));
			Assert.That(q.TryDequeue(out ov), Is.True);
			Assert.That(ov, Is.EqualTo(44));

			q.Set(4, 46);

			// out of mod range
			Assert.Throws<ArgumentOutOfRangeException>(() => q.Set(5, 99));

			q.Set(0, 47);

			Assert.That(q.TryDequeue(out ov), Is.True);
			Assert.That(ov, Is.EqualTo(45));
			Assert.That(q.TryDequeue(out ov), Is.True);
			Assert.That(ov, Is.EqualTo(46));
			Assert.That(q.TryDequeue(out ov), Is.True);
			Assert.That(ov, Is.EqualTo(47));

			q.Set(2, 49);

			Assert.That(q.TryDequeue(out ov), Is.False);

			q.Set(1, 48);

			Assert.That(q.TryDequeue(out ov), Is.True);
			Assert.That(ov, Is.EqualTo(48));
			Assert.That(q.TryDequeue(out ov), Is.True);
			Assert.That(ov, Is.EqualTo(49));
		}

		[Test]
		public void RingQueueTest2()
		{
			var q = new RingQueue<int>(50, ushort.MaxValue + 1);

			for (int i = 0; i < ushort.MaxValue - 10; i++)
			{
				q.Set(i, i);
				Assert.That(q.TryDequeue(out var iCheck), Is.True);
				Assert.That(iCheck, Is.EqualTo(i));
			}

			var setStatus = q.IsSet(ushort.MaxValue - 20);
			Assert.That(setStatus.HasFlag(ItemSetStatus.Set), Is.True);

			for (int i = ushort.MaxValue - 10; i < ushort.MaxValue + 10; i++)
			{
				q.Set(i % (ushort.MaxValue + 1), 42);
			}
		}

		[Test]
		public void RingQueueTest3()
		{
			var q = new RingQueue<int>(100, ushort.MaxValue + 1);

			int iSet = 0;
			for (int blockSize = 1; blockSize < 100; blockSize++)
			{
				for (int i = 0; i < blockSize; i++)
				{
					q.Set(iSet++, i);
				}
				for (int i = 0; i < blockSize; i++)
				{
					Assert.That(q.TryDequeue(out var iCheck), Is.True);
					Assert.That(iCheck, Is.EqualTo(i));
				}
			}

			for (int blockSize = 1; blockSize < 100; blockSize++)
			{
				q = new RingQueue<int>(100, ushort.MaxValue + 1);
				for (int i = 0; i < blockSize; i++)
				{
					q.Set(i, i);
				}
				for (int i = 0; i < blockSize; i++)
				{
					Assert.That(q.TryDequeue(out var iCheck), Is.True);
					Assert.That(iCheck, Is.EqualTo(i));
				}
			}
		}
	}
}
