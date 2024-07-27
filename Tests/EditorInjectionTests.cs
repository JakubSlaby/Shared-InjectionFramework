using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using WhiteSparrow.Shared.DependencyInjection.Tests.Mock;

namespace WhiteSparrow.Shared.DependencyInjection.Tests
{
    public class EditorInjectionTests
    {
        [UnityTest]
        public IEnumerator PlayModeChangeCleanup()
        {
            var applicationContext = Injection.Context.MockApplication();
            var networkingContext = Injection.Context.MockNetworking();

            Assert.IsFalse(applicationContext.IsCreated);
            Assert.AreEqual(applicationContext.Count, 0);
            Assert.IsFalse(networkingContext.IsCreated);
            Assert.AreEqual(networkingContext.Count, 0);
            
            applicationContext.Create();
            networkingContext.Create();
            
            applicationContext.Map<ITestPlayerModel>(new MockTestPlayerModel());
            applicationContext.Map<ITestCurrencyModel>(new MockTestCurrencyModel());
            networkingContext.Map<ITestMultiplayerManager>(new MockTestMultiplayerModel());
            
            var player = new TestPlayer();
            Injection.Inject(player);

            Assert.NotNull(player.PlayerModel);
            Assert.IsTrue(player.PlayerModel is MockTestPlayerModel);
            
            Assert.NotNull(player.Currency);
            Assert.IsTrue(player.Currency is MockTestCurrencyModel);
            
            Assert.NotNull(player.MultiplayerManager);
            Assert.IsTrue(player.MultiplayerManager is MockTestMultiplayerModel);

            yield return new EnterPlayMode();

            applicationContext = Injection.Context.MockApplication();
            networkingContext = Injection.Context.MockNetworking();
            
            
            Assert.IsFalse(applicationContext.IsCreated);
            Assert.AreEqual(applicationContext.Count, 0);
            Assert.IsFalse(networkingContext.IsCreated);
            Assert.AreEqual(networkingContext.Count, 0);
            
            applicationContext.Create();
            networkingContext.Create();
            
            applicationContext.Map<ITestPlayerModel>(new TestPlayerModel());
            applicationContext.Map<ITestCurrencyModel>(new TestCurrencyModel());
            networkingContext.Map<ITestMultiplayerManager>(new TestMultiplayerModel());
            
            var player2 = new TestPlayer();
            Injection.Inject(player2);
            
            Assert.NotNull(player2.PlayerModel);
            Assert.IsTrue(player2.PlayerModel is TestPlayerModel);
            
            Assert.NotNull(player2.Currency);
            Assert.IsTrue(player2.Currency is TestCurrencyModel);
            
            Assert.NotNull(player2.MultiplayerManager);
            Assert.IsTrue(player2.MultiplayerManager is TestMultiplayerModel);
            
            yield return new ExitPlayMode();
            
            applicationContext = Injection.Context.MockApplication();
            networkingContext = Injection.Context.MockNetworking();
            
            
            Assert.IsFalse(applicationContext.IsCreated);
            Assert.AreEqual(applicationContext.Count, 0);
            Assert.IsFalse(networkingContext.IsCreated);
            Assert.AreEqual(networkingContext.Count, 0);
        }

        [Test]
        public void EditorCleanup()
        {
            var applicationContext = Injection.Context.MockApplication();
            var networkingContext = Injection.Context.MockNetworking();

            Assert.IsFalse(applicationContext.IsCreated);
            Assert.AreEqual(applicationContext.Count, 0);
            Assert.IsFalse(networkingContext.IsCreated);
            Assert.AreEqual(networkingContext.Count, 0);
            
            applicationContext.Create();
            networkingContext.Create();
            
            applicationContext.Map<ITestPlayerModel>(new MockTestPlayerModel());
            applicationContext.Map<ITestCurrencyModel>(new MockTestCurrencyModel());
            networkingContext.Map<ITestMultiplayerManager>(new MockTestMultiplayerModel());
            
            Injection.Context.Impl.DestroyAll();
            
            applicationContext = Injection.Context.MockApplication();
            networkingContext = Injection.Context.MockNetworking();

            Assert.IsFalse(applicationContext.IsCreated);
            Assert.AreEqual(applicationContext.Count, 0);
            Assert.IsFalse(networkingContext.IsCreated);
            Assert.AreEqual(networkingContext.Count, 0);
        }
    }
}