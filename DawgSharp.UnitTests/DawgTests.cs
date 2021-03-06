﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace DawgSharp.UnitTests
{
    [TestFixture]
    public class DawgTests
    {
        [Test]
        public void AssertNodeCount ()
        {
            var dawgBuilder = new DawgBuilder<int> ();

            dawgBuilder.Insert ("tip", 3);
            dawgBuilder.Insert ("tap", 3);

            var rehydrated = GetDawg (dawgBuilder);

            Assert.AreEqual (4, rehydrated.GetNodeCount());
        }

        [Test]
        public void PersistenceTest ()
        {
            var dawgBuilder = new DawgBuilder<int> ();

            dawgBuilder.Insert ("cone", 10);
            dawgBuilder.Insert ("bone", 10);
            dawgBuilder.Insert ("gone", 9);
            dawgBuilder.Insert ("go", 5);
            dawgBuilder.Insert ("tip", 3);
            dawgBuilder.Insert ("tap", 3);

            var rehydrated = GetDawg (dawgBuilder);

            Assert.AreEqual (10, rehydrated ["cone"]);
            Assert.AreEqual (10, rehydrated ["bone"]);
            Assert.AreEqual (0, rehydrated ["cones"]);
            Assert.AreEqual (9, rehydrated ["gone"]);
            Assert.AreEqual (0, rehydrated ["g"]);
            Assert.AreEqual (5, rehydrated ["go"]);
            Assert.AreEqual (0, rehydrated ["god"]);
            Assert.AreEqual (3, rehydrated ["tip"]);
            Assert.AreEqual (3, rehydrated ["tap"]);
            Assert.AreEqual (0, rehydrated [""]);
        }

        [Test]
        public void EmptyNodeTest ()
        {
            var dawgBuilder = new DawgBuilder<int> ();

            dawgBuilder.Insert ("tip", 0);

            var rehydrated = GetDawg (dawgBuilder);

            Assert.AreEqual (0, rehydrated ["tip"]);
        }

        [Test]
        public void TipTapTest ()
        {
            var dawgBuilder = new DawgBuilder<int> ();

            dawgBuilder.Insert ("tip", 3);
            dawgBuilder.Insert ("tap", 3);

            var rehydrated = GetDawg (dawgBuilder);

            Assert.AreEqual (3, rehydrated ["tap"]);
            Assert.AreEqual (3, rehydrated ["tip"]);
        }

        [Test]
        public void EmptyKey ()
        {
            var dawgBuilder = new DawgBuilder<int> ();

            dawgBuilder.Insert ("", 5);

            var rehydrated = GetDawg (dawgBuilder);

            Assert.AreEqual (5, rehydrated [""]);
        }

        [Test]
        public void LongStringTest()
        {
            var longString = Enumerable.Repeat ('a', 200 * 1000);

            var dawgBuilder = new DawgBuilder<bool> ();

            dawgBuilder.Insert (longString, true);

            var rehydrated = GetDawg (dawgBuilder);

            Assert.IsTrue (rehydrated [longString]);
        }

        [Test]
        public void EnekoWordListTest()
        {
            var words = File.ReadAllLines (Path.Combine(TestContext.CurrentContext.TestDirectory, "eneko-words.txt"));

            var dawgBuilder = new DawgBuilder<bool> ();

            foreach (var word in words)
            {
                dawgBuilder.Insert (word, true);
            }

            var rehydrated = GetDawg (dawgBuilder);

            foreach (string word in words)
            {
                Assert.IsTrue(rehydrated [word], word);
            }
        }

        [Test]
        public void MatchPrefixTest ()
        {
            var dawgBuilder = new DawgBuilder<bool> ();

            dawgBuilder.Insert ("cat", true);
            dawgBuilder.Insert ("caterpillar", true);
            dawgBuilder.Insert ("dog", true);

            var dawg = GetDawg (dawgBuilder);

            Assert.AreEqual ("cat,caterpillar", MatchJoin(dawg, "cat"));
            Assert.AreEqual ("cat,caterpillar", MatchJoin(dawg, "ca"));
            Assert.AreEqual ("cat,caterpillar,dog", MatchJoin(dawg, ""));
            Assert.AreEqual ("", MatchJoin(dawg, "boot"));
            Assert.AreEqual ("", MatchJoin(dawg, "cats"));
        }

        private static string MatchJoin(Dawg<bool> dawg, IEnumerable<char> prefix)
        {
            return string.Join(",", dawg.MatchPrefix (prefix).Select (kvp => kvp.Key));
        }

        protected virtual Dawg<TPayload> GetDawg <TPayload> (DawgBuilder<TPayload> dawgBuilder)
        {
            var dawg = dawgBuilder.BuildDawg ();

            return dawg;
        }

        [Test]
        public void EmptyDictionaryTest ()
        {
            var dawgBuilder = new DawgBuilder<bool> ();

            var dawg = GetDawg (dawgBuilder);

            Assert.IsTrue (dawg.MatchPrefix ("boot").Count () == 0);
            Assert.IsTrue (dawg.MatchPrefix ("").Count () == 0);
            Assert.IsFalse(dawg [""]);
            Assert.IsFalse(dawg ["boot"]);
        }

        /// <summary>
        /// You can do suffix matching as easily as prefix matching.  Simply Reverse() the keys:
        /// </summary>
        [Test]
        public void SuffixMatchTest ()
        {
            var dawgBuilder = new DawgBuilder<bool> ();

            dawgBuilder.Insert ( "visibility".Reverse (), true);
            dawgBuilder.Insert ("possibility".Reverse (), true);
            dawgBuilder.Insert ("dexterity".Reverse (), true);

            var dawg = GetDawg (dawgBuilder);

            Assert.IsTrue (dawg.MatchPrefix ("ility".Reverse ()).Count () == 2);
        }
    }
}
